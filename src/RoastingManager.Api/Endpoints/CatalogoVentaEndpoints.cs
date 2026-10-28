using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts;
using RoastingManager.Api.ErrorHandling;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Api.Endpoints;

/// <summary>Proveedores de insumos, insumos, presentaciones y clientes (FR-002, FR-004 a FR-006).</summary>
public static class CatalogoVentaEndpoints
{
    public static void MapCatalogoVenta(this RouteGroupBuilder api)
    {
        api.MapContactos<ProveedorInsumo>("/proveedores-insumo");
        api.MapContactos<Cliente>("/clientes");
        MapInsumos(api);
        MapPresentaciones(api);
    }

    private static void MapInsumos(RouteGroupBuilder api)
    {
        api.MapPost("/insumos", async (InsumoRequest req, RoastingDbContext db, CancellationToken ct) =>
        {
            var insumo = new Insumo();
            Validacion.Ejecutar(
                () => insumo.Nombre = ReglasComunes.NormalizarNombre(req.Nombre),
                () => insumo.ProveedorInsumoId = Requerido.Valor(req.ProveedorInsumoId, "proveedorInsumoId"),
                () =>
                {
                    insumo.PrecioUnitario = Requerido.Valor(req.PrecioUnitario, "precioUnitario");
                    ReglasComunes.ValidarMonto("precioUnitario", insumo.PrecioUnitario);
                });

            await Catalogo.VerificarExisteAsync<ProveedorInsumo>(db, insumo.ProveedorInsumoId, "proveedorInsumoId",
                "El proveedor de insumos indicado no existe.");
            if (await Catalogo.NombreExisteAsync<Insumo>(db, insumo.Nombre))
            {
                return ManejadorErrores.NombreDuplicado();
            }
            db.Insumos.Add(insumo);
            await db.SaveChangesAsync(ct);
            await db.Entry(insumo).Reference(i => i.ProveedorInsumo).LoadAsync(ct);
            return Results.Created($"/api/insumos/{insumo.Id}", AInsumo(insumo));
        });

        api.MapGet("/insumos", async (RoastingDbContext db, CancellationToken ct) =>
            (await db.Insumos.Include(i => i.ProveedorInsumo).OrderBy(i => i.Nombre).ToListAsync(ct)).Select(AInsumo));

        api.MapGet("/insumos/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await db.Insumos.Include(i => i.ProveedorInsumo).SingleOrDefaultAsync(i => i.Id == id, ct) is { } insumo
                ? AInsumo(insumo)
                : throw new NoEncontradoException("El insumo indicado no existe."));
    }

    private static void MapPresentaciones(RouteGroupBuilder api)
    {
        api.MapPost("/presentaciones", async (PresentacionRequest req, RoastingDbContext db, CancellationToken ct) =>
        {
            var presentacion = new Presentacion();
            Validacion.Ejecutar(
                () => presentacion.Nombre = ReglasComunes.NormalizarNombre(req.Nombre),
                () =>
                {
                    presentacion.Gramos = Requerido.Valor(req.Gramos, "gramos");
                    ReglasComunes.ValidarPositivo("gramos", presentacion.Gramos);
                });

            // Cero o más insumos; cada uno cuenta una vez por unidad vendida.
            var insumoIds = (req.InsumoIds ?? []).Distinct().ToList();
            var insumos = await db.Insumos.Where(i => insumoIds.Contains(i.Id)).ToListAsync(ct);
            if (insumos.Count != insumoIds.Count)
            {
                throw new NoEncontradoException("Alguno de los insumos indicados no existe.", "insumoIds");
            }
            if (await Catalogo.NombreExisteAsync<Presentacion>(db, presentacion.Nombre))
            {
                return ManejadorErrores.NombreDuplicado();
            }

            presentacion.Insumos = insumos.Select(i => new PresentacionInsumo { Insumo = i }).ToList();
            db.Presentaciones.Add(presentacion);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/presentaciones/{presentacion.Id}", APresentacion(presentacion));
        });

        api.MapGet("/presentaciones", async (RoastingDbContext db, CancellationToken ct) =>
            (await db.Presentaciones.Include(p => p.Insumos).ThenInclude(pi => pi.Insumo)
                .OrderBy(p => p.Nombre).ToListAsync(ct)).Select(APresentacion));

        api.MapGet("/presentaciones/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await db.Presentaciones.Include(p => p.Insumos).ThenInclude(pi => pi.Insumo)
                .SingleOrDefaultAsync(p => p.Id == id, ct) is { } presentacion
                ? APresentacion(presentacion)
                : throw new NoEncontradoException("La presentación indicada no existe."));
    }

    private static InsumoResponse AInsumo(Insumo i) =>
        new(i.Id, i.Nombre, new Referencia(i.ProveedorInsumo.Id, i.ProveedorInsumo.Nombre), i.PrecioUnitario);

    private static PresentacionResponse APresentacion(Presentacion p) => new(
        p.Id, p.Nombre, p.Gramos,
        p.Insumos.Select(pi => new InsumoDePresentacion(pi.Insumo.Id, pi.Insumo.Nombre, pi.Insumo.PrecioUnitario))
            .OrderBy(i => i.Nombre).ToList());
}
