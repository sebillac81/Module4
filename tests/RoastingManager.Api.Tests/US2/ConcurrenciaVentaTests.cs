using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US2;

// FR-022, SC-004
public class ConcurrenciaVentaTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Cinco_ventas_simultaneas_de_1kg_sobre_3kg_aceptan_exactamente_3()
    {
        var cafe = await Datos.CafeVerde(Client, precioKg: 20m, stockInicialKg: 10m);
        await Datos.Batch(Client, cafe, 3.75m, 3m);
        var presentacion = await DatosVenta.Bolsa250(Client);
        var cliente = await DatosVenta.Cliente(Client);

        var respuestas = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            Client.PostAsJsonAsync("/api/ventas", new
            {
                clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 4, precioUnitario = 15m,
            })));

        Assert.Equal(3, respuestas.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(2, respuestas.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(0m, (await Datos.StockDe(Client, cafe)).Dec("stockTostadoKg"));
    }
}
