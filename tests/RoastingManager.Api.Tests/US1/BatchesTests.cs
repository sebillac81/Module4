using System.Net;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US1;

public class BatchesTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Independent_test_de_US1()
    {
        var cafe = await Datos.CafeVerde(Client, "Colombia", precioKg: 20.00m, stockInicialKg: 2m);
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = cafe, cantidadKg = 8m });

        var batch = await Client.PostOk("/api/batches", new
        {
            cafeVerdeId = cafe, verdeKg = 5m, tostadoKg = 4.25m, humedadPct = 11.5m, densidadGl = 680m,
        });

        Assert.Equal(15.00m, batch.Dec("mermaPct"));
        Assert.Equal(23.53m, batch.Dec("costoTostadoKg"));
        Assert.Equal(100.00m, batch.Dec("costoTotal"));
        Assert.Equal(20.00m, batch.Dec("precioKgAplicado"));
        Assert.Equal("Colombia", batch.GetProperty("cafeVerde").GetString());
        Assert.Equal(11.5m, batch.Dec("humedadPct"));

        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(5.000m, stock.Dec("stockVerdeKg"));
        Assert.Equal(4.250m, stock.Dec("stockTostadoKg"));
        Assert.Equal(23.53m, stock.Dec("costoPromedioKg"));

        var porId = await Client.GetOk($"/api/batches/{batch.Id()}");
        Assert.Equal(23.53m, porId.Dec("costoTostadoKg"));
    }

    [Fact]
    public async Task Batch_sin_stock_suficiente_responde_409_con_disponible_y_requerido_y_no_cambia_nada()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 3m);

        var (status, json) = await Client.Post("/api/batches", new { cafeVerdeId = cafe, verdeKg = 5m, tostadoKg = 4m });

        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Equal(3m, json.Dec("disponibleKg"));
        Assert.Equal(5m, json.Dec("requeridoKg"));
        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(3m, stock.Dec("stockVerdeKg"));
        Assert.Equal(0m, stock.Dec("stockTostadoKg"));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, stock.GetProperty("costoPromedioKg").ValueKind);
        Assert.Equal(0, (await Client.GetOk("/api/batches")).GetArrayLength());
    }

    [Fact]
    public async Task Batch_que_consume_todo_el_stock_se_acepta_y_lo_deja_en_0()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 5m);
        await Datos.Batch(Client, cafe, 5m, 4.25m);
        Assert.Equal(0m, (await Datos.StockDe(Client, cafe)).Dec("stockVerdeKg"));
    }

    [Fact]
    public async Task Datos_invalidos_y_stock_insuficiente_a_la_vez_responde_400_y_no_409()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 5m);

        var (status, json) = await Client.Post("/api/batches", new { cafeVerdeId = cafe, verdeKg = 150m, tostadoKg = 120m });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(json.GetProperty("errors").TryGetProperty("verdeKg", out _));
    }

    [Fact]
    public async Task Batch_invalido_informa_todos_los_errores_de_datos_juntos()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 10m);

        var (status, json) = await Client.Post("/api/batches", new
        {
            cafeVerdeId = cafe, verdeKg = 5m, tostadoKg = 5m, humedadPct = 150m, fecha = "2026-10-28",
        });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        var errores = json.GetProperty("errors");
        Assert.True(errores.TryGetProperty("tostadoKg", out _));
        Assert.True(errores.TryGetProperty("humedadPct", out _));
        Assert.True(errores.TryGetProperty("fecha", out _));
    }

    [Theory]
    [InlineData("0", "4")]
    [InlineData("5", "0")]
    [InlineData("5.0005", "4")]
    public async Task Cantidades_invalidas_responden_400(string verde, string tostado)
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 10m);
        var (status, _) = await Client.Post("/api/batches", new
        {
            cafeVerdeId = cafe,
            verdeKg = decimal.Parse(verde, System.Globalization.CultureInfo.InvariantCulture),
            tostadoKg = decimal.Parse(tostado, System.Globalization.CultureInfo.InvariantCulture),
        });
        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact]
    public async Task Cafe_inexistente_responde_404()
    {
        var (status, json) = await Client.Post("/api/batches", new { cafeVerdeId = 999, verdeKg = 5m, tostadoKg = 4m });
        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal("cafeVerdeId", json.GetProperty("campo").GetString());
    }

    [Fact]
    public async Task Dos_batches_seguidos_actualizan_el_costo_promedio_ponderado()
    {
        var cafe = await Datos.CafeVerde(Client, precioKg: 20.00m, stockInicialKg: 10m);
        await Datos.Batch(Client, cafe, 5m, 4.25m); // 23,53
        var b2 = await Datos.Batch(Client, cafe, 2m, 1.6m); // 20 × 2 / 1,6 = 25,00
        Assert.Equal(25.00m, b2.Dec("costoTostadoKg"));

        // (4250 × 23,53 + 1600 × 25,00) / 5850 = 23,932… → 23,93
        var stock = await Datos.StockDe(Client, cafe);
        Assert.Equal(5.85m, stock.Dec("stockTostadoKg"));
        Assert.Equal(23.93m, stock.Dec("costoPromedioKg"));
    }

    [Fact]
    public async Task Fecha_pasada_se_acepta_y_por_defecto_es_hoy()
    {
        var cafe = await Datos.CafeVerde(Client, stockInicialKg: 10m);
        var conFecha = await Client.PostOk("/api/batches", new { cafeVerdeId = cafe, verdeKg = 1m, tostadoKg = 0.8m, fecha = "2026-10-01" });
        var sinFecha = await Datos.Batch(Client, cafe, 1m, 0.8m);

        Assert.Equal("2026-10-01", conFecha.GetProperty("fecha").GetString());
        Assert.Equal("2026-10-27", sinFecha.GetProperty("fecha").GetString());
    }
}
