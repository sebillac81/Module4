using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;

namespace RoastingManager.Api.ErrorHandling;

/// <summary>Traduce los errores de dominio y de base a ProblemDetails (research R9).</summary>
public class ManejadorErrores : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        IResult? resultado = exception switch
        {
            ValidacionException ex => Results.ValidationProblem(
                ex.Errores.ToDictionary(e => e.Key, e => e.Value),
                title: "Datos inválidos"),
            StockInsuficienteException ex => Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Stock insuficiente",
                detail: $"No hay stock suficiente: disponible {Cantidad.AKg(ex.DisponibleG):0.000} kg, " +
                        $"requerido {Cantidad.AKg(ex.RequeridoG):0.000} kg.",
                extensions: new Dictionary<string, object?>
                {
                    ["disponibleKg"] = Cantidad.AKg(ex.DisponibleG),
                    ["requeridoKg"] = Cantidad.AKg(ex.RequeridoG),
                }),
            NoEncontradoException ex => Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "No encontrado",
                detail: ex.Message,
                extensions: ex.Campo is null ? null : new Dictionary<string, object?> { ["campo"] = ex.Campo }),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                NombreDuplicado(),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "No encontrado",
                    detail: "Una de las referencias indicadas no existe."),
            BadHttpRequestException ex => Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos inválidos",
                detail: "El cuerpo de la solicitud no tiene el formato esperado."),
            _ => null,
        };

        if (resultado is null)
        {
            return false;
        }
        await resultado.ExecuteAsync(http);
        return true;
    }

    public static IResult NombreDuplicado() => Results.Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "Nombre duplicado",
        detail: "Ya existe un registro con ese nombre.",
        extensions: new Dictionary<string, object?> { ["campo"] = "nombre" });
}
