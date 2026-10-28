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

/// <summary>Ventas (FR-014 a FR-018) e historial con rentabilidad (FR-021).</summary>
public static class VentasEndpoints
{
    public static void MapVentas(this RouteGroupBuilder api)
    {
        api.MapPost("/ventas", async (VentaRequest req, RegistrarVenta operacion,
            IOptions<TopesOptions> topes, IReloj reloj, CancellationToken ct) =>
        {
            long clienteId = 0, cafeId = 0, presentacionId = 0;
            int unidades = 0;
            decimal precio = 0;
            var hoy = reloj.Hoy();
            var fecha = req.Fecha ?? hoy;

            // FR-025: primero todos los errores de datos (400); referencias (404) y stock (409) después.
            Validacion.Ejecutar(
                () => clienteId = Requerido.Valor(req.ClienteId, "clienteId"),
                () => cafeId = Requerido.Valor(req.CafeVerdeId, "cafeVerdeId"),
                () => presentacionId = Requerido.Valor(req.PresentacionId, "presentacionId"),
                () =>
                {
                    unidades = Requerido.Valor(req.Unidades, "unidades");
                    ReglasVenta.ValidarUnidades(unidades, topes.Value.VentaMaxUnidades);
                },
                () =>
                {
                    precio = Requerido.Valor(req.PrecioUnitario, "precioUnitario");
                    ReglasComunes.ValidarMonto("precioUnitario", precio);
                },
                () => ReglasComunes.ValidarFechaNoFutura("fecha", fecha, hoy));

            var venta = await operacion.EjecutarAsync(
                new SolicitudVenta(clienteId, cafeId, presentacionId, unidades, precio, fecha), ct);
            return Results.Created($"/api/ventas/{venta.Id}", ARespuesta(venta));
        });

        api.MapGet("/ventas", async (RoastingDbContext db, CancellationToken ct) =>
            (await ConDetalle(db).OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id).ToListAsync(ct))
                .Select(ARespuesta));

        api.MapGet("/ventas/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await ConDetalle(db).SingleOrDefaultAsync(v => v.Id == id, ct) is { } venta
                ? ARespuesta(venta)
                : throw new NoEncontradoException("La venta indicada no existe."));
    }

    private static IQueryable<Venta> ConDetalle(RoastingDbContext db) => db.Ventas
        .Include(v => v.Cliente).Include(v => v.CafeVerde).Include(v => v.Presentacion).Include(v => v.Insumos)
        .AsSplitQuery();

    private static VentaResponse ARespuesta(Venta v) => new(
        v.Id, v.Fecha,
        new Referencia(v.Cliente.Id, v.Cliente.Nombre),
        new Referencia(v.CafeVerde.Id, v.CafeVerde.Nombre),
        new PresentacionVendida(v.Presentacion.Id, v.Presentacion.Nombre, v.GramosPresentacion),
        v.Unidades, v.PrecioUnitario, Cantidad.AKg(v.GramosVendidos), v.CostoPromedioKgAplicado,
        v.Insumos.OrderBy(i => i.NombreInsumo).Select(i => new InsumoAplicado(i.NombreInsumo, i.PrecioUnitarioAplicado)).ToList(),
        v.Ingreso, v.CostoCafeVendido, v.CostosAsociados, v.Ganancia, v.MargenPct);
}
