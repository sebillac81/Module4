using System.Diagnostics;
using System.Net.Http.Json;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests;

// RNF-01, SC-005: p95 < 3 s con 5 usuarios concurrentes. Es un test de humo, no un benchmark.
public class RendimientoTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Cinco_clientes_concurrentes_con_20_operaciones_mixtas_tienen_p95_menor_a_3_segundos()
    {
        var cafe = await Datos.CafeVerde(Client, precioKg: 20m, stockInicialKg: 900m);
        await Datos.Batch(Client, cafe, 50m, 42m);
        var presentacion = await DatosVenta.Bolsa250(Client);
        var cliente = await DatosVenta.Cliente(Client);

        Task<HttpResponseMessage> Operacion(int i) => (i % 5) switch
        {
            0 => Client.PostAsJsonAsync("/api/batches", new { cafeVerdeId = cafe, verdeKg = 1m, tostadoKg = 0.85m }),
            1 => Client.PostAsJsonAsync("/api/ventas", new
            {
                clienteId = cliente, cafeVerdeId = cafe, presentacionId = presentacion, unidades = 2, precioUnitario = 15m,
            }),
            2 => Client.GetAsync("/api/stock"),
            3 => Client.GetAsync("/api/batches"),
            _ => Client.GetAsync("/api/ventas"),
        };

        var duraciones = new System.Collections.Concurrent.ConcurrentBag<double>();
        await Task.WhenAll(Enumerable.Range(0, 5).Select(async usuario =>
        {
            for (var i = 0; i < 20; i++)
            {
                var reloj = Stopwatch.StartNew();
                var respuesta = await Operacion(usuario + i);
                duraciones.Add(reloj.Elapsed.TotalMilliseconds);
                Assert.True(respuesta.IsSuccessStatusCode, $"{(int)respuesta.StatusCode}");
            }
        }));

        var ordenadas = duraciones.Order().ToArray();
        var p95 = ordenadas[(int)Math.Ceiling(ordenadas.Length * 0.95) - 1];
        Assert.True(p95 < 3000, $"p95 = {p95:0} ms");
    }
}
