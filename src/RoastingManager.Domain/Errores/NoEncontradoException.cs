namespace RoastingManager.Domain.Errores;

/// <summary>Un recurso o una referencia del body no existe. <see cref="Campo"/> es null
/// cuando el recurso se pidió por id en la ruta.</summary>
public class NoEncontradoException(string mensaje, string? campo = null) : Exception(mensaje)
{
    public string? Campo { get; } = campo;
}
