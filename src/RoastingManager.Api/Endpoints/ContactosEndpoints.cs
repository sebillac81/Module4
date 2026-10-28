using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts;
using RoastingManager.Api.ErrorHandling;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Api.Endpoints;

/// <summary>Alta y consulta de proveedores de café, proveedores de insumos y clientes
/// (FR-001, FR-002, FR-006): las tres tablas tienen la misma forma.</summary>
public static class ContactosEndpoints
{
    public static void MapContactos<T>(this RouteGroupBuilder api, string ruta) where T : Contacto, new()
    {
        api.MapPost(ruta, async (ContactoRequest req, RoastingDbContext db, CancellationToken ct) =>
        {
            var contacto = new T();
            Validacion.Ejecutar(
                () => contacto.Nombre = ReglasComunes.NormalizarNombre(req.Nombre),
                () => contacto.Telefono = Catalogo.Opcional(req.Telefono, "telefono", 40),
                () => contacto.Email = Catalogo.Email(req.Email),
                () => contacto.Direccion = Catalogo.Opcional(req.Direccion, "direccion", 200));

            if (await Catalogo.NombreExisteAsync<T>(db, contacto.Nombre))
            {
                return ManejadorErrores.NombreDuplicado();
            }
            db.Set<T>().Add(contacto);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api{ruta}/{contacto.Id}", ARespuesta(contacto));
        });

        api.MapGet(ruta, async (RoastingDbContext db, CancellationToken ct) =>
            (await db.Set<T>().OrderBy(x => x.Nombre).ToListAsync(ct)).Select(ARespuesta));

        api.MapGet(ruta + "/{id:long}", async (long id, RoastingDbContext db, CancellationToken ct) =>
            await db.Set<T>().FindAsync([id], ct) is { } contacto
                ? ARespuesta(contacto)
                : throw new NoEncontradoException("El registro indicado no existe."));
    }

    private static ContactoResponse ARespuesta(Contacto c) => new(c.Id, c.Nombre, c.Telefono, c.Email, c.Direccion);
}
