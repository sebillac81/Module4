namespace RoastingManager.Api.Configuracion;

public class AppOptions
{
    public const string Seccion = "App";

    /// <summary>Zona horaria que define "hoy" (FR-028, research R8).</summary>
    public string ZonaHoraria { get; set; } = "America/Argentina/Buenos_Aires";
}
