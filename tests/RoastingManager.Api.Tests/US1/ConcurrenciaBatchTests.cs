using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US1;

// FR-022, SC-004: el stock nunca queda negativo con operaciones simultáneas.
public class ConcurrenciaBatchTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Cinco_batches_simultaneos_de_3kg_sobre_10kg_aceptan_exactamente_3()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 10m);

        var respuestas = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            Client.PostAsJsonAsync("/api/batches", new { cafeVerdeId = cafe, verdeKg = 3m, tostadoKg = 2.5m })));

        Assert.Equal(3, respuestas.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(2, respuestas.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(1m, stock.Dec("stockVerdeKg"));
        Assert.Equal(7.5m, stock.Dec("stockTostadoKg"));
    }
}
