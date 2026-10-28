using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts;
using RoastingManager.Domain.Calculos;
using RoastingManager.Infrastructure.Persistence;

namespace RoastingManager.Api.Endpoints;

/// <summary>Stock por café: verde, tostado y costo promedio ponderado (FR-019).</summary>
public static class StockEndpoints
{
    public static void MapStock(this RouteGroupBuilder api)
    {
        api.MapGet("/stock", async (RoastingDbContext db, CancellationToken ct) =>
            (await db.CafesVerdes.OrderBy(c => c.Nombre).ToListAsync(ct))
                .Select(c => new StockResponse(
                    c.Id, c.Nombre, Cantidad.AKg(c.StockVerdeG), Cantidad.AKg(c.StockTostadoG), c.CostoPromedioKg)));
    }
}
