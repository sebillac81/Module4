using RoastingManager.Domain.Errores;

namespace RoastingManager.Api.Endpoints;

/// <summary>Campos obligatorios del body: los DTOs usan tipos anulables para poder informar
/// "obligatorio" por campo en lugar de un error genérico de formato (FR-025).</summary>
public static class Requerido
{
    public static T Valor<T>(T? valor, string campo) where T : struct =>
        valor ?? throw new ValidacionException(campo, "Es obligatorio.");
}
