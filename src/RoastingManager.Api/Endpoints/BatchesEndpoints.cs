using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoastingManager.Api.Configuracion;
using RoastingManager.Api.Contracts;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Operaciones;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Api.Endpoints;

/// <summary>Batches de tueste (FR-008 a FR-013) e historial (FR-020).</summary>
public static class BatchesEndpoints
{
    public static void MapBatches(this RouteGroupBuilder api)
    {
        api.MapPost("/batches", async (BatchRequest req, RegistrarBatch operacion,
            IOptions<TopesOptions> topes, IReloj reloj, CancellationToken ct) =>
        {
            long cafeId = 0;
            long? verdeG = null, tostadoG = null;
            var hoy = reloj.Hoy();
            var fecha = req.Fecha ?? hoy;

            // FR-025: primero todos los errores de datos (400); el stock se controla después (409).
            Validacion.Ejecutar(
                () => cafeId = Requerido.Valor(req.CafeVerdeId, "cafeVerdeId"),
                () =>
                {
                    var kg = Requerido.Valor(req.VerdeKg, "verdeKg");
                    ReglasBatch.ValidarTopeBatch(kg, topes.Value.BatchVerdeMaxKg);
                    verdeG = Cantidad.DesdeKg(kg, "verdeKg");
                },
                () => tostadoG = Cantidad.DesdeKg(Requerido.Valor(req.TostadoKg, "tostadoKg"), "tostadoKg"),
                () => ReglasComunes.ValidarFechaNoFutura("fecha", fecha, hoy),
                () => ReglasBatch.ValidarDatosProceso(req.HumedadPct, req.DensidadGl, req.DesarrolloPct),
                () =>
                {
                    // Solo si ambas cantidades se pudieron leer, para no repetir su error.
                    if (verdeG is not null && tostadoG is not null)
                    {
                        CalculoBatch.ValidarCantidades(verdeG.Value, tostadoG.Value);
                    }
                });

            var batch = await operacion.EjecutarAsync(new SolicitudBatch(
                cafeId, verdeG!.Value, tostadoG!.Value, fecha, req.HumedadPct, req.DensidadGl, req.DesarrolloPct), ct);
            return Results.Created($"/api/batches/{batch.Id}", ARespuesta(batch));
        });

        api.MapGet("/batches", async (RoastingDbContext db, CancellationToken ct) =>
            (await db.Batches.Include(b => b.CafeVerde)
                .OrderByDescending(b => b.Fecha).ThenByDescending(b => b.Id)
                .ToListAsync(ct)).Select(ARespuesta));

        api.MapGet("/batches/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await db.Batches.Include(b => b.CafeVerde).SingleOrDefaultAsync(b => b.Id == id, ct) is { } batch
                ? ARespuesta(batch)
                : throw new NoEncontradoException("El batch indicado no existe."));
    }

    private static BatchResponse ARespuesta(Batch b) => new(
        b.Id, b.CafeVerdeId, b.CafeVerde.Nombre, b.Fecha, Cantidad.AKg(b.VerdeG), Cantidad.AKg(b.TostadoG),
        b.HumedadPct, b.DensidadGl, b.DesarrolloPct, b.PrecioKgAplicado, b.MermaPct, b.CostoTostadoKg, b.CostoTotal);
}
