namespace RoastingManager.Api.Configuracion;

/// <summary>Topes por operación (FR-030). Se sobrescriben por entorno, p. ej.
/// <c>Topes__BatchVerdeMaxKg=150</c>.</summary>
public class TopesOptions
{
    public const string Seccion = "Topes";

    public decimal IngresoVerdeMaxKg { get; set; } = 1000m;
    public decimal BatchVerdeMaxKg { get; set; } = 100m;
    public int VentaMaxUnidades { get; set; } = 1000;
}
