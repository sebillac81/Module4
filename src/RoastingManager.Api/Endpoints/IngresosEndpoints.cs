using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoastingManager.Api.Configuracion;
using RoastingManager.Api.Contracts;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Operaciones;
using RoastingManager.Infrastructure.Persistence;

namespace RoastingManager.Api.Endpoints;

/// <summary>Ingresos de stock de café verde (FR-007a, FR-007b).</summary>
public static class IngresosEndpoints
{
    public static void MapIngresos(this RouteGroupBuilder api)
    {
        api.MapPost("/ingresos-stock", async (IngresoRequest req, RegistrarIngreso operacion,
            IOptions<TopesOptions> topes, IReloj reloj, CancellationToken ct) =>
        {
            long cafeId = 0, cantidadG = 0;
            var hoy = reloj.Hoy();
            var fecha = req.Fecha ?? hoy;
            Validacion.Ejecutar(
                () => cafeId = Requerido.Valor(req.CafeVerdeId, "cafeVerdeId"),
                () =>
                {
                    var kg = Requerido.Valor(req.CantidadKg, "cantidadKg");
                    ReglasComunes.ValidarPositivo("cantidadKg", kg);
                    ReglasBatch.ValidarTopeIngreso("cantidadKg", kg, topes.Value.IngresoVerdeMaxKg);
                    cantidadG = Cantidad.DesdeKg(kg, "cantidadKg");
                },
                () => ReglasComunes.ValidarFechaNoFutura("fecha", fecha, hoy));

            var ingreso = await operacion.EjecutarAsync(new SolicitudIngreso(cafeId, cantidadG, fecha), ct);
            return Results.Created($"/api/ingresos-stock/{ingreso.Id}", new IngresoResponse(
                ingreso.Id, ingreso.CafeVerdeId, ingreso.CafeVerde.Nombre, Cantidad.AKg(ingreso.CantidadG),
                ingreso.Fecha, Cantidad.AKg(ingreso.CafeVerde.StockVerdeG)));
        });

        api.MapGet("/ingresos-stock", async (long? cafeVerdeId, RoastingDbContext db, CancellationToken ct) =>
        {
            var consulta = db.IngresosStock.Include(i => i.CafeVerde).AsQueryable();
            if (cafeVerdeId is not null)
            {
                consulta = consulta.Where(i => i.CafeVerdeId == cafeVerdeId);
            }
            // FR-028: fecha descendente y, a igual fecha, orden de registro descendente.
            var ingresos = await consulta.OrderByDescending(i => i.Fecha).ThenByDescending(i => i.Id).ToListAsync(ct);
            return ingresos.Select(i => new IngresoResponse(
                i.Id, i.CafeVerdeId, i.CafeVerde.Nombre, Cantidad.AKg(i.CantidadG), i.Fecha, null));
        });
    }
}
