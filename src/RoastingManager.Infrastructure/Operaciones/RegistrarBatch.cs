using Microsoft.Extensions.Logging;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Operaciones;

/// <summary>Datos ya validados por la capa de API (cantidades, tope, fecha, datos de proceso).</summary>
public record SolicitudBatch(
    long CafeVerdeId, long VerdeG, long TostadoG, DateOnly Fecha,
    decimal? HumedadPct, decimal? DensidadGl, decimal? DesarrolloPct);

/// <summary>Registro de un batch (FR-009 a FR-013): descuenta verde, suma tostado y recalcula
/// el costo promedio del café, todo o nada (FR-023).</summary>
public class RegistrarBatch(RoastingDbContext db, ILogger<RegistrarBatch> log)
{
    public async Task<Batch> EjecutarAsync(SolicitudBatch s, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var cafe = await BloqueoCafe.ObtenerParaActualizarAsync(db, s.CafeVerdeId, ct);

        try
        {
            ReglasBatch.ValidarStockVerde(cafe.StockVerdeG, s.VerdeG);
        }
        catch (StockInsuficienteException)
        {
            log.LogWarning("Batch rechazado por stock insuficiente: café {CafeId}, requerido {RequeridoG} g, disponible {DisponibleG} g",
                cafe.Id, s.VerdeG, cafe.StockVerdeG);
            throw;
        }

        var calculo = CalculoBatch.Calcular(s.VerdeG, s.TostadoG, cafe.PrecioKg);
        var costoPromedio = CalculoCostoPromedio.Calcular(
            cafe.StockTostadoG, cafe.CostoPromedioKg, s.TostadoG, calculo.CostoTostadoKg);

        cafe.StockVerdeG -= s.VerdeG;
        cafe.StockTostadoG += s.TostadoG;
        cafe.CostoPromedioKg = costoPromedio;

        var batch = new Batch
        {
            CafeVerde = cafe,
            VerdeG = s.VerdeG,
            TostadoG = s.TostadoG,
            Fecha = s.Fecha,
            HumedadPct = s.HumedadPct,
            DensidadGl = s.DensidadGl,
            DesarrolloPct = s.DesarrolloPct,
            PrecioKgAplicado = cafe.PrecioKg,
            MermaPct = calculo.MermaPct,
            CostoTostadoKg = calculo.CostoTostadoKg,
            CostoTotal = calculo.CostoTotal,
        };
        db.Batches.Add(batch);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        log.LogInformation("Batch {BatchId}: café {CafeId}, verde {VerdeG} g, tostado {TostadoG} g, costo promedio {CostoPromedio}",
            batch.Id, cafe.Id, s.VerdeG, s.TostadoG, costoPromedio);
        return batch;
    }
}
