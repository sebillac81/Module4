using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;

namespace RoastingManager.Api.Endpoints;

/// <summary>Reglas compartidas por las altas de catálogo.</summary>
public static class Catalogo
{
    /// <summary>FR-027: el nombre es único por tipo, sin distinguir mayúsculas ni espacios.
    /// El índice único lower(nombre) cubre además las altas simultáneas (research R6).</summary>
    public static Task<bool> NombreExisteAsync<T>(RoastingDbContext db, string nombre) where T : class =>
        db.Set<T>().AnyAsync(x => EF.Property<string>(x, "Nombre").ToLower() == nombre.ToLower());

    public static async Task VerificarExisteAsync<T>(RoastingDbContext db, long id, string campo, string mensaje)
        where T : class
    {
        if (!await db.Set<T>().AnyAsync(x => EF.Property<long>(x, "Id") == id))
        {
            throw new NoEncontradoException(mensaje, campo);
        }
    }

    /// <summary>Texto opcional: vacío se guarda como null; se valida el largo máximo.</summary>
    public static string? Opcional(string? valor, string campo, int largoMaximo)
    {
        var texto = valor?.Trim();
        if (string.IsNullOrEmpty(texto)) return null;
        if (texto.Length > largoMaximo)
        {
            throw new ValidacionException(campo, $"Admite hasta {largoMaximo} caracteres.");
        }
        return texto;
    }

    public static string? Email(string? valor)
    {
        var email = Opcional(valor, "email", 160);
        if (email is not null && (!MailAddress.TryCreate(email, out var direccion) || direccion.Address != email))
        {
            throw new ValidacionException("email", "El email no tiene un formato válido.");
        }
        return email;
    }

    public static void ValidarNoNegativo(string campo, decimal valor)
    {
        if (valor < 0)
        {
            throw new ValidacionException(campo, "No puede ser negativo.");
        }
    }
}
