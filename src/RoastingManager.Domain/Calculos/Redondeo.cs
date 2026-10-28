namespace RoastingManager.Domain.Calculos;

/// <summary>Único punto de redondeo de montos y porcentajes (FR-024, principio V).</summary>
public static class Redondeo
{
    public static decimal Aplicar(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
