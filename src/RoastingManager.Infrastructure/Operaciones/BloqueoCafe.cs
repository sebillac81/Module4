using Microsoft.EntityFrameworkCore;
using RoastingManager.Domain.Errores;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Operaciones;

public static class BloqueoCafe
{
    /// <summary>Lee la fila del café con <c>FOR UPDATE</c> dentro de la transacción en curso:
    /// las operaciones de stock sobre el mismo café se serializan (research R4).</summary>
    public static async Task<CafeVerde> ObtenerParaActualizarAsync(RoastingDbContext db, long cafeVerdeId, CancellationToken ct)
    {
        // ToListAsync evita que EF envuelva la consulta en un subselect.
        var filas = await db.CafesVerdes
            .FromSqlInterpolated($"SELECT * FROM cafe_verde WHERE id = {cafeVerdeId} FOR UPDATE")
            .ToListAsync(ct);
        return filas.SingleOrDefault()
            ?? throw new NoEncontradoException("El café verde indicado no existe.", "cafeVerdeId");
    }
}
