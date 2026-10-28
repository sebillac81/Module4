using System.Net;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US2;

public class VentasTests(ApiFixture fixture) : PruebaApi(fixture)
{
    private async Task<(long Cafe, long Presentacion, long Cliente)> Escenario()
    {
        var cafe = await DatosVenta.CafeCon5KgTostadoA22(Client);
        return (cafe, await DatosVenta.Bolsa250(Client), await DatosVenta.Cliente(Client));
    }

    private Task<System.Text.Json.JsonElement> Vender(long cafe, long presentacion, long cliente, int unidades = 4, decimal precio = 15.00m) =>
        Client.PostOk("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades, precioUnitario = precio,
        });

    [Fact]
    public async Task Independent_test_de_US2()
    {
        var (cafe, presentacion, cliente) = await Escenario();
        Assert.Equal(22.00m, (await Datos.StockDe(Client, cafe)).Dec("costoPromedioKg"));

        var venta = await Vender(cafe, presentacion, cliente);

        Assert.Equal(60.00m, venta.Dec("ingreso"));
        Assert.Equal(22.00m, venta.Dec("costoCafeVendido"));
        Assert.Equal(2.40m, venta.Dec("costosAsociados"));
        Assert.Equal(35.60m, venta.Dec("ganancia"));
        Assert.Equal(59.33m, venta.Dec("margenPct"));
        Assert.Equal(1.000m, venta.Dec("cantidadVendidaKg"));
        Assert.Equal("Bar Central", venta.GetProperty("cliente").GetProperty("nombre").GetString());
        Assert.Equal(250, venta.GetProperty("presentacion").GetProperty("gramos").GetInt32());

        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(4.000m, stock.Dec("stockTostadoKg"));
        Assert.Equal(22.00m, stock.Dec("costoPromedioKg")); // FR-015: la venta no cambia el costo promedio
    }

    [Fact]
    public async Task La_venta_devuelve_los_precios_de_insumos_y_el_costo_promedio_aplicados()
    {
        var (cafe, presentacion, cliente) = await Escenario();
        var venta = await Vender(cafe, presentacion, cliente);

        Assert.Equal(22.00m, venta.Dec("costoPromedioKgAplicado"));
        var insumos = venta.GetProperty("insumos").EnumerateArray()
            .ToDictionary(i => i.GetProperty("nombre").GetString()!, i => i.Dec("precioUnitarioAplicado"));
        Assert.Equal(0.50m, insumos["Bolsa"]);
        Assert.Equal(0.10m, insumos["Etiqueta"]);
    }

    [Fact]
    public async Task Venta_sin_stock_suficiente_responde_409_sin_efectos()
    {
        var (cafe, presentacion, cliente) = await Escenario();

        var (status, json) = await Client.Post("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 21, precioUnitario = 15m,
        });

        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Equal(5m, json.Dec("disponibleKg"));
        Assert.Equal(5.25m, json.Dec("requeridoKg"));
        Assert.Equal(5m, (await Datos.StockDe(Client, cafe)).Dec("stockTostadoKg"));
        Assert.Equal(0, (await Client.GetOk("/api/ventas")).GetArrayLength());
    }

    [Fact]
    public async Task Venta_de_un_cafe_nunca_tostado_responde_409()
    {
        var (_, presentacion, cliente) = await Escenario();
        var otro = await Datos.CafeVerde(Client, "Brasil");

        var (status, _) = await Client.Post("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = otro, presentacionId = presentacion, unidades = 1, precioUnitario = 15m,
        });

        Assert.Equal(HttpStatusCode.Conflict, status);
    }

    [Fact]
    public async Task Un_batch_posterior_no_cambia_la_venta_guardada()
    {
        var (cafe, presentacion, cliente) = await Escenario();
        var venta = await Vender(cafe, presentacion, cliente);

        await Datos.Batch(Client, cafe, 2m, 1.5m); // costo 17,60 × 2 / 1,5 = 23,47 → cambia el promedio

        var guardada = await Client.GetOk($"/api/ventas/{venta.Id()}");
        Assert.Equal(22.00m, guardada.Dec("costoPromedioKgAplicado"));
        Assert.Equal(35.60m, guardada.Dec("ganancia"));
        Assert.Equal(59.33m, guardada.Dec("margenPct"));
        Assert.NotEqual(22.00m, (await Datos.StockDe(Client, cafe)).Dec("costoPromedioKg"));
    }

    [Fact]
    public async Task Vender_otro_cafe_no_toca_el_stock_de_Colombia()
    {
        var (colombia, presentacion, cliente) = await Escenario();
        var brasil = await DatosVenta.CafeCon5KgTostadoA22(Client, "Brasil");

        await Vender(brasil, presentacion, cliente);

        Assert.Equal(5m, (await Datos.StockDe(Client, colombia)).Dec("stockTostadoKg"));
        Assert.Equal(4m, (await Datos.StockDe(Client, brasil)).Dec("stockTostadoKg"));
    }

    [Fact]
    public async Task Batch_tras_vender_todo_el_tostado_toma_el_costo_del_batch()
    {
        // Movido desde T028 (F1): necesita ventas, que recién existen en US2.
        var (cafe, presentacion, cliente) = await Escenario();
        await Vender(cafe, presentacion, cliente, unidades: 20); // 5 kg: deja el tostado en 0

        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(0m, stock.Dec("stockTostadoKg"));
        Assert.Equal(22.00m, stock.Dec("costoPromedioKg")); // FR-019: conserva el último costo

        await Datos.Batch(Client, cafe, 2m, 1.5m); // 17,60 × 2 / 1,5 = 23,47
        Assert.Equal(23.47m, (await Datos.StockDe(Client, cafe)).Dec("costoPromedioKg"));

        var (status, _) = await Client.Post("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 7, precioUnitario = 15m,
        });
        Assert.Equal(HttpStatusCode.Conflict, status); // 1,75 kg > 1,5 kg
    }

    [Fact]
    public async Task Precio_bajo_registra_la_venta_con_margen_negativo()
    {
        var (cafe, presentacion, cliente) = await Escenario();
        var venta = await Vender(cafe, presentacion, cliente, unidades: 4, precio: 1.00m);

        Assert.Equal(-20.40m, venta.Dec("ganancia")); // 4,00 − (22,00 + 2,40)
        Assert.Equal(-510.00m, venta.Dec("margenPct"));
    }

    [Fact]
    public async Task Datos_invalidos_responden_400_con_todos_los_campos()
    {
        var (cafe, presentacion, cliente) = await Escenario();

        var (status, json) = await Client.Post("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 0, precioUnitario = 0m,
            fecha = "2026-10-28",
        });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        var errores = json.GetProperty("errors");
        Assert.True(errores.TryGetProperty("unidades", out _));
        Assert.True(errores.TryGetProperty("precioUnitario", out _));
        Assert.True(errores.TryGetProperty("fecha", out _));
    }

    [Fact]
    public async Task Unidades_sobre_el_tope_responden_400_aunque_falte_stock()
    {
        var (cafe, presentacion, cliente) = await Escenario();
        var (status, json) = await Client.Post("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 1001, precioUnitario = 15m,
        });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("unidades", out _));
    }

    [Fact]
    public async Task Referencias_inexistentes_responden_404_con_el_campo()
    {
        var (cafe, presentacion, cliente) = await Escenario();

        var (s1, j1) = await Client.Post("/api/ventas", new { clienteId = 999, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 1, precioUnitario = 15m });
        var (s2, j2) = await Client.Post("/api/ventas", new { clienteId = cliente, cafeVerdeId = cafe, presentacionId = 999, unidades = 1, precioUnitario = 15m });
        var (s3, j3) = await Client.Post("/api/ventas", new { clienteId = cliente, cafeVerdeId = 999, presentacionId = presentacion, unidades = 1, precioUnitario = 15m });

        Assert.Equal(HttpStatusCode.NotFound, s1);
        Assert.Equal("clienteId", j1.GetProperty("campo").GetString());
        Assert.Equal(HttpStatusCode.NotFound, s2);
        Assert.Equal("presentacionId", j2.GetProperty("campo").GetString());
        Assert.Equal(HttpStatusCode.NotFound, s3);
        Assert.Equal("cafeVerdeId", j3.GetProperty("campo").GetString());
    }
}
