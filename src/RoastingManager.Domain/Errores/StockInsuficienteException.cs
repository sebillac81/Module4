namespace RoastingManager.Domain.Errores;

/// <summary>No hay stock suficiente para la operación (FR-010, FR-016).</summary>
public class StockInsuficienteException(long disponibleG, long requeridoG)
    : Exception("No hay stock suficiente para registrar la operación.")
{
    public long DisponibleG { get; } = disponibleG;
    public long RequeridoG { get; } = requeridoG;
}
