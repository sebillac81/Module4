using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoastingManager.Api.Configuracion;
using RoastingManager.Api.Contracts;
using RoastingManager.Api.ErrorHandling;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Api.Endpoints;

/// <summary>Proveedores de café (FR-001) y cafés verdes con stock inicial (FR-003, FR-007).</summary>
public static class CatalogoCafeEndpoints
{
    public static void MapCatalogoCafe(this RouteGroupBuilder api)
    {
        api.MapContactos<ProveedorCafe>("/proveedores-cafe");

        api.MapPost("/cafes-verdes", async (CafeVerdeRequest req, RoastingDbContext db,
            IOptions<TopesOptions> topes, CancellationToken ct) =>
        {
            var cafe = new CafeVerde();
            Validacion.Ejecutar(
                () => cafe.Nombre = ReglasComunes.NormalizarNombre(req.Nombre),
                () => cafe.ProveedorCafeId = Requerido.Valor(req.ProveedorCafeId, "proveedorCafeId"),
                () =>
                {
                    cafe.PrecioKg = Requerido.Valor(req.PrecioKg, "precioKg");
                    ReglasComunes.ValidarMonto("precioKg", cafe.PrecioKg);
                },
                () =>
                {
                    var kg = Requerido.Valor(req.StockInicialKg, "stockInicialKg");
                    Catalogo.ValidarNoNegativo("stockInicialKg", kg);
                    ReglasBatch.ValidarTopeIngreso("stockInicialKg", kg, topes.Value.IngresoVerdeMaxKg);
                    cafe.StockInicialG = Cantidad.DesdeKg(kg, "stockInicialKg");
                });

            await Catalogo.VerificarExisteAsync<ProveedorCafe>(db, cafe.ProveedorCafeId, "proveedorCafeId",
                "El proveedor de café indicado no existe.");
            if (await Catalogo.NombreExisteAsync<CafeVerde>(db, cafe.Nombre))
            {
                return ManejadorErrores.NombreDuplicado();
            }

            cafe.StockVerdeG = cafe.StockInicialG;
            db.CafesVerdes.Add(cafe);
            await db.SaveChangesAsync(ct);
            await db.Entry(cafe).Reference(c => c.ProveedorCafe).LoadAsync(ct);
            return Results.Created($"/api/cafes-verdes/{cafe.Id}", ARespuesta(cafe));
        });

        api.MapGet("/cafes-verdes", async (RoastingDbContext db, CancellationToken ct) =>
            (await db.CafesVerdes.Include(c => c.ProveedorCafe).OrderBy(c => c.Nombre).ToListAsync(ct))
                .Select(ARespuesta));

        api.MapGet("/cafes-verdes/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await db.CafesVerdes.Include(c => c.ProveedorCafe).SingleOrDefaultAsync(c => c.Id == id, ct) is { } cafe
                ? ARespuesta(cafe)
                : throw new NoEncontradoException("El café verde indicado no existe."));
    }

    private static CafeVerdeResponse ARespuesta(CafeVerde c) => new(
        c.Id, c.Nombre, new Referencia(c.ProveedorCafe.Id, c.ProveedorCafe.Nombre),
        c.PrecioKg, Cantidad.AKg(c.StockInicialG));
}
