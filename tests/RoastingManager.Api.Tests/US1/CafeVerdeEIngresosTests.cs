using System.Net;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US1;

public class CafeVerdeEIngresosTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Alta_de_proveedor_y_cafe_verde_devuelve_201_con_el_recurso()
    {
        var proveedor = await Client.PostOk("/api/proveedores-cafe",
            new { nombre = "  Finca Huila ", telefono = "123", email = "finca@huila.co" });
        Assert.Equal("Finca Huila", proveedor.GetProperty("nombre").GetString());

        var cafe = await Client.PostOk("/api/cafes-verdes",
            new { nombre = "Colombia", proveedorCafeId = proveedor.Id(), precioKg = 20.00m, stockInicialKg = 2m });
        Assert.Equal(20.00m, cafe.Dec("precioKg"));
        Assert.Equal(2m, cafe.Dec("stockInicialKg"));
        Assert.Equal("Finca Huila", cafe.GetProperty("proveedorCafe").GetProperty("nombre").GetString());

        var porId = await Client.GetOk($"/api/cafes-verdes/{cafe.Id()}");
        Assert.Equal("Colombia", porId.GetProperty("nombre").GetString());
        var listado = await Client.GetOk("/api/cafes-verdes");
        Assert.Equal(1, listado.GetArrayLength());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("20.001")]
    public async Task Precio_invalido_responde_400_con_el_campo(string precio)
    {
        var proveedor = await Datos.ProveedorCafe(Client);
        var (status, json) = await Client.Post("/api/cafes-verdes", new
        {
            nombre = "Colombia", proveedorCafeId = proveedor,
            precioKg = decimal.Parse(precio, System.Globalization.CultureInfo.InvariantCulture), stockInicialKg = 1m,
        });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("precioKg", out _));
    }

    [Fact]
    public async Task Stock_inicial_sobre_el_tope_de_ingreso_responde_400()
    {
        var proveedor = await Datos.ProveedorCafe(Client);
        var (status, json) = await Client.Post("/api/cafes-verdes",
            new { nombre = "Colombia", proveedorCafeId = proveedor, precioKg = 20m, stockInicialKg = 1500m });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("stockInicialKg", out _));
    }

    [Fact]
    public async Task Proveedor_inexistente_responde_404_indicando_el_campo()
    {
        var (status, json) = await Client.Post("/api/cafes-verdes",
            new { nombre = "Colombia", proveedorCafeId = 999, precioKg = 20m, stockInicialKg = 1m });
        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal("proveedorCafeId", json.GetProperty("campo").GetString());
    }

    [Fact]
    public async Task Nombre_duplicado_con_otra_capitalizacion_responde_409()
    {
        await Datos.CafeVerde(Client, "Colombia");
        var proveedor = await Datos.ProveedorCafe(Client, "Otro");
        var (status, json) = await Client.Post("/api/cafes-verdes",
            new { nombre = " COLOMBIA ", proveedorCafeId = proveedor, precioKg = 20m, stockInicialKg = 1m });
        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Equal("nombre", json.GetProperty("campo").GetString());

        var (statusProv, _) = await Client.Post("/api/proveedores-cafe", new { nombre = "otro" });
        Assert.Equal(HttpStatusCode.Conflict, statusProv);
    }

    [Fact]
    public async Task Ingreso_de_8kg_suma_al_stock_verde()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 2m);
        var ingreso = await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 8m });

        Assert.Equal(8m, ingreso.Dec("cantidadKg"));
        Assert.Equal(10m, ingreso.Dec("stockVerdeResultanteKg"));
        Assert.Equal("2026-10-27", ingreso.GetProperty("fecha").GetString());
        Assert.Equal(10m, (await Datos.StockDe(Client, cafe)).Dec("stockVerdeKg"));
    }

    [Fact]
    public async Task Ingreso_invalido_responde_400_y_no_cambia_el_stock()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 2m);

        var (s1, _) = await Client.Post("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 0m });
        var (s2, _) = await Client.Post("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = -1m });
        var (s3, j3) = await Client.Post("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 1m, fecha = "2026-10-28" });
        var (s4, _) = await Client.Post("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 1001m });

        Assert.All(new[] { s1, s2, s3, s4 }, s => Assert.Equal(HttpStatusCode.BadRequest, s));
        Assert.True(j3.GetProperty("errors").TryGetProperty("fecha", out _));
        Assert.Equal(2m, (await Datos.StockDe(Client, cafe)).Dec("stockVerdeKg"));
    }

    [Fact]
    public async Task Historial_de_ingresos_ordenado_por_fecha_descendente_y_filtrable_por_cafe()
    {
        var cafe = await Datos.CafeVerde(Client, "Colombia");
        var otro = await Datos.CafeVerde(Client, "Brasil");
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 1m, fecha = "2026-10-20" });
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 2m, fecha = "2026-10-25" });
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 3m, fecha = "2026-10-25" });
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = otro, cantidadKg = 4m });

        var historial = await Client.GetOk($"/api/ingresos-stock?cafeVerdeId={cafe}");
        var cantidades = historial.EnumerateArray().Select(i => i.Dec("cantidadKg")).ToArray();
        Assert.Equal(new[] { 3m, 2m, 1m }, cantidades);

        Assert.Equal(4, (await Client.GetOk("/api/ingresos-stock")).GetArrayLength());
    }
}
