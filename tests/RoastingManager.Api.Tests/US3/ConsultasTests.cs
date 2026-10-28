using System.Text.Json;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US3;

public class ConsultasTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Listados_vacios_devuelven_arreglos_vacios()
    {
        foreach (var ruta in new[]
                 {
                     "/api/stock", "/api/batches", "/api/ventas", "/api/ingresos-stock", "/api/cafes-verdes",
                     "/api/proveedores-cafe", "/api/proveedores-insumo", "/api/clientes", "/api/insumos", "/api/presentaciones",
                 })
        {
            Assert.Equal(0, (await Client.GetOk(ruta)).GetArrayLength());
        }
    }

    [Fact]
    public async Task Stock_por_cafe_ordenado_por_nombre_con_costo_promedio_o_null_si_nunca_se_tosto()
    {
        var colombia = await DatosVenta.CafeCon5KgTostadoA22(Client, "Colombia");
        await Datos.CafeVerde(Client, "Brasil", stockInicialKg: 12m);

        var stock = (await Client.GetOk("/api/stock")).EnumerateArray().ToList();

        Assert.Equal(new[] { "Brasil", "Colombia" }, stock.Select(s => s.GetProperty("cafeVerde").GetString()));
        Assert.Equal(12m, stock[0].Dec("stockVerdeKg"));
        Assert.Equal(JsonValueKind.Null, stock[0].GetProperty("costoPromedioKg").ValueKind);
        Assert.Equal(3.75m, stock[1].Dec("stockVerdeKg"));
        Assert.Equal(5m, stock[1].Dec("stockTostadoKg"));
        Assert.Equal(22.00m, stock[1].Dec("costoPromedioKg"));
        Assert.Equal(colombia, stock[1].GetProperty("cafeVerdeId").GetInt64());
    }

    [Fact]
    public async Task Stock_con_tostado_en_0_conserva_el_ultimo_costo_promedio()
    {
        var cafe = await DatosVenta.CafeCon5KgTostadoA22(Client);
        var presentacion = await DatosVenta.Bolsa250(Client);
        var cliente = await DatosVenta.Cliente(Client);
        await Client.PostOk("/api/ventas", new
        {
            clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 20, precioUnitario = 15m,
        });

        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(0m, stock.Dec("stockTostadoKg"));
        Assert.Equal(22.00m, stock.Dec("costoPromedioKg"));
    }

    [Fact]
    public async Task Historial_de_batches_por_fecha_descendente_con_su_costo_de_produccion()
    {
        var cafe = await Datos.CafeVerde(Client, precioKg: 20m, stockInicialKg: 20m);
        await Client.PostOk("/api/batches", new { cafeVerdeId = cafe, verdeKg = 1m, tostadoKg = 0.8m, fecha = "2026-10-20" });
        await Client.PostOk("/api/batches", new { cafeVerdeId = cafe, verdeKg = 5m, tostadoKg = 4.25m, fecha = "2026-10-26" });
        await Client.PostOk("/api/batches", new { cafeVerdeId = cafe, verdeKg = 2m, tostadoKg = 1.6m, fecha = "2026-10-10" }); // carga atrasada

        var historial = (await Client.GetOk("/api/batches")).EnumerateArray().ToList();

        Assert.Equal(new[] { "2026-10-26", "2026-10-20", "2026-10-10" }, historial.Select(b => b.GetProperty("fecha").GetString()));
        Assert.Equal(23.53m, historial[0].Dec("costoTostadoKg"));
        Assert.Equal(100.00m, historial[0].Dec("costoTotal"));
        Assert.Equal(15.00m, historial[0].Dec("mermaPct"));
        Assert.Equal(20.00m, historial[0].Dec("precioKgAplicado"));
    }

    [Fact]
    public async Task Historial_de_ventas_por_fecha_descendente_con_costos_asociados_y_rentabilidad()
    {
        var cafe = await DatosVenta.CafeCon5KgTostadoA22(Client);
        var presentacion = await DatosVenta.Bolsa250(Client);
        var cliente = await DatosVenta.Cliente(Client);
        foreach (var (fecha, unidades) in new[] { ("2026-10-25", 1), ("2026-10-27", 4), ("2026-10-25", 2) })
        {
            await Client.PostOk("/api/ventas", new
            {
                clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades, precioUnitario = 15m, fecha,
            });
        }

        var historial = (await Client.GetOk("/api/ventas")).EnumerateArray().ToList();

        // Fecha descendente y, a igual fecha, orden de registro descendente (FR-028).
        Assert.Equal(new[] { 4, 2, 1 }, historial.Select(v => v.GetProperty("unidades").GetInt32()));
        Assert.Equal(2.40m, historial[0].Dec("costosAsociados"));
        Assert.Equal(35.60m, historial[0].Dec("ganancia"));
        Assert.Equal(59.33m, historial[0].Dec("margenPct"));
        Assert.Equal("Bar Central", historial[0].GetProperty("cliente").GetProperty("nombre").GetString());
    }
}
