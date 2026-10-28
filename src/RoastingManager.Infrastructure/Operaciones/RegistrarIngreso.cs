using Microsoft.Extensions.Logging;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Operaciones;

/// <summary>Datos ya validados (cantidad > 0, tope y fecha) por la capa de API.</summary>
public record SolicitudIngreso(long CafeVerdeId, long CantidadG, DateOnly Fecha);

/// <summary>Ingreso de stock de café verde (FR-007a), atómico (FR-023).</summary>
public class RegistrarIngreso(RoastingDbContext db, ILogger<RegistrarIngreso> log)
{
    public async Task<IngresoStock> EjecutarAsync(SolicitudIngreso s, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var cafe = await BloqueoCafe.ObtenerParaActualizarAsync(db, s.CafeVerdeId, ct);

        cafe.StockVerdeG += s.CantidadG;
        var ingreso = new IngresoStock
        {
            CafeVerde = cafe,
            CantidadG = s.CantidadG,
            Fecha = s.Fecha,
        };
        db.IngresosStock.Add(ingreso);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        log.LogInformation("Ingreso de stock: café {CafeId}, {CantidadG} g; stock verde {StockVerdeG} g",
            cafe.Id, s.CantidadG, cafe.StockVerdeG);
        return ingreso;
    }
}
