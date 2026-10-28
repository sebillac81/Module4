using System.Net.Http.Json;
using Npgsql;
using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests.US3;

// FR-031: el stock es siempre reconstruible a partir de los registros.
public class InvarianteStockTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task Tras_una_secuencia_mixta_y_concurrente_el_stock_coincide_con_los_registros()
    {
        var colombia = await Datos.CafeVerde(Client, "Colombia", precioKg: 20m, stockInicialKg: 5m);
        var brasil = await Datos.CafeVerde(Client, "Brasil", precioKg: 15m, stockInicialKg: 0m);
        var presentacion = await DatosVenta.Bolsa250(Client);
        var cliente = await DatosVenta.Cliente(Client);

        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = colombia, cantidadKg = 8m });
        await Client.PostOk("/api/ingresos-stock", new { cafeVerdeId = brasil, cantidadKg = 6.5m });
        await Datos.Batch(Client, colombia, 5m, 4.25m);
        await Client.Post("/api/batches", new { cafeVerdeId = brasil, verdeKg = 10m, tostadoKg = 8m }); // rechazado

        // Operaciones concurrentes mezcladas sobre ambos cafés; algunas se rechazan.
        var operaciones = new List<Task<HttpResponseMessage>>();
        for (var i = 0; i < 4; i++)
        {
            operaciones.Add(Client.PostAsJsonAsync("/api/batches", new { cafeVerdeId = colombia, verdeKg = 2.5m, tostadoKg = 2.1m }));
            operaciones.Add(Client.PostAsJsonAsync("/api/batches", new { cafeVerdeId = brasil, verdeKg = 2m, tostadoKg = 1.7m }));
            operaciones.Add(Client.PostAsJsonAsync("/api/ventas", new
            {
                clienteId = cliente, cafeVerdeId = colombia, presentacionId = presentacion, unidades = 6, precioUnitario = 12m,
            }));
            operaciones.Add(Client.PostAsJsonAsync("/api/ingresos-stock", new { cafeVerdeId = brasil, cantidadKg = 0.75m }));
        }
        await Task.WhenAll(operaciones);

        await using var conexion = new NpgsqlConnection(Fixture.ConnectionString);
        await conexion.OpenAsync();
        await using var comando = new NpgsqlCommand("""
            SELECT c.id, c.stock_verde_g, c.stock_tostado_g,
                   c.stock_inicial_g
                     + COALESCE((SELECT sum(cantidad_g) FROM ingreso_stock i WHERE i.cafe_verde_id = c.id), 0)
                     - COALESCE((SELECT sum(verde_g) FROM batch b WHERE b.cafe_verde_id = c.id), 0) AS verde_esperado,
                   COALESCE((SELECT sum(tostado_g) FROM batch b WHERE b.cafe_verde_id = c.id), 0)
                     - COALESCE((SELECT sum(gramos_vendidos) FROM venta v WHERE v.cafe_verde_id = c.id), 0) AS tostado_esperado
            FROM cafe_verde c
            """, conexion);
        await using var lector = await comando.ExecuteReaderAsync();
        var filas = 0;
        while (await lector.ReadAsync())
        {
            filas++;
            Assert.True(lector.GetInt64(1) >= 0 && lector.GetInt64(2) >= 0);
            Assert.Equal(lector.GetInt64(3), lector.GetInt64(1));
            Assert.Equal(lector.GetInt64(4), lector.GetInt64(2));
        }
        Assert.Equal(2, filas);
    }
}
