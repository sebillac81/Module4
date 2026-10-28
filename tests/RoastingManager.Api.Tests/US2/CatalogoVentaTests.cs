using System.Net;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US2;

public class CatalogoVentaTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Alta_de_proveedor_de_insumos_insumo_presentacion_y_cliente()
    {
        var proveedor = await DatosVenta.ProveedorInsumo(Client);
        var insumo = await Client.PostOk("/api/insumos", new { nombre = "Bolsa", proveedorInsumoId = proveedor, precioUnitario = 0.50m });
        Assert.Equal(0.50m, insumo.Dec("precioUnitario"));
        Assert.Equal("Envases SA", insumo.GetProperty("proveedorInsumo").GetProperty("nombre").GetString());

        var presentacion = await Client.PostOk("/api/presentaciones",
            new { nombre = "Bolsa 250 g", gramos = 250, insumoIds = new[] { insumo.Id() } });
        Assert.Equal(250, presentacion.GetProperty("gramos").GetInt32());
        var insumos = presentacion.GetProperty("insumos");
        Assert.Equal(1, insumos.GetArrayLength());
        Assert.Equal(0.50m, insumos[0].Dec("precioUnitario"));

        var sinInsumos = await Client.PostOk("/api/presentaciones", new { nombre = "Granel 1 kg", gramos = 1000, insumoIds = Array.Empty<long>() });
        Assert.Equal(0, sinInsumos.GetProperty("insumos").GetArrayLength());

        var cliente = await Client.PostOk("/api/clientes", new { nombre = "Bar Central", email = "bar@central.com" });
        Assert.Equal("Bar Central", cliente.GetProperty("nombre").GetString());

        Assert.Equal(2, (await Client.GetOk("/api/presentaciones")).GetArrayLength());
        Assert.Equal(1, (await Client.GetOk("/api/insumos")).GetArrayLength());
        Assert.Equal(1, (await Client.GetOk("/api/clientes")).GetArrayLength());
        Assert.Equal(1, (await Client.GetOk("/api/proveedores-insumo")).GetArrayLength());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.5")]
    [InlineData("0.005")]
    public async Task Insumo_con_precio_invalido_responde_400(string precio)
    {
        var proveedor = await DatosVenta.ProveedorInsumo(Client);
        var (status, json) = await Client.Post("/api/insumos", new
        {
            nombre = "Bolsa", proveedorInsumoId = proveedor,
            precioUnitario = decimal.Parse(precio, System.Globalization.CultureInfo.InvariantCulture),
        });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("precioUnitario", out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-250)]
    public async Task Presentacion_con_gramos_menores_o_iguales_a_cero_responde_400(int gramos)
    {
        var (status, json) = await Client.Post("/api/presentaciones", new { nombre = "Bolsa", gramos, insumoIds = Array.Empty<long>() });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("gramos", out _));
    }

    [Fact]
    public async Task Presentacion_con_insumo_inexistente_responde_404()
    {
        var (status, json) = await Client.Post("/api/presentaciones", new { nombre = "Bolsa", gramos = 250, insumoIds = new[] { 999 } });
        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal("insumoIds", json.GetProperty("campo").GetString());
    }

    [Fact]
    public async Task Email_con_formato_invalido_responde_400()
    {
        var (status, json) = await Client.Post("/api/clientes", new { nombre = "Bar", email = "no-es-un-email" });
        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Nombres_duplicados_responden_409()
    {
        var proveedor = await DatosVenta.ProveedorInsumo(Client);
        await DatosVenta.Insumo(Client, "Bolsa", 0.5m, proveedor);
        await DatosVenta.Cliente(Client, "Bar Central");
        await Client.PostOk("/api/presentaciones", new { nombre = "Bolsa 250 g", gramos = 250 });

        var (s1, _) = await Client.Post("/api/insumos", new { nombre = "BOLSA", proveedorInsumoId = proveedor, precioUnitario = 1m });
        var (s2, _) = await Client.Post("/api/clientes", new { nombre = " bar central" });
        var (s3, _) = await Client.Post("/api/presentaciones", new { nombre = "bolsa 250 G", gramos = 500 });
        var (s4, _) = await Client.Post("/api/proveedores-insumo", new { nombre = "envases sa" });

        Assert.All(new[] { s1, s2, s3, s4 }, s => Assert.Equal(HttpStatusCode.Conflict, s));
    }
}
