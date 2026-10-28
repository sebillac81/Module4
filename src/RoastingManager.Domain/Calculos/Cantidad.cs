using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Calculos;

/// <summary>Conversión exacta kg ↔ gramos enteros (FR-026).</summary>
public static class Cantidad
{
    public static long DesdeKg(decimal kg, string campo)
    {
        var gramos = kg * 1000m;
        if (gramos != decimal.Truncate(gramos))
        {
            throw new ValidacionException(campo, "Admite hasta 3 decimales (gramos enteros).");
        }
        return (long)gramos;
    }

    public static decimal AKg(long gramos) => gramos / 1000m;
}
