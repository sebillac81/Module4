using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Reglas;

public static class ReglasComunes
{
    public const int LargoMaximoNombre = 120;

    /// <summary>Montos cargados por el usuario: ≥ 0,01 y hasta 2 decimales (FR-029).</summary>
    public static void ValidarMonto(string campo, decimal monto)
    {
        if (monto < 0.01m)
        {
            throw new ValidacionException(campo, "Debe ser mayor a cero (mínimo 0,01).");
        }
        ValidarMaximoDecimales(campo, monto, 2);
    }

    public static void ValidarMaximoDecimales(string campo, decimal valor, int decimales)
    {
        var escalado = valor * (decimal)Math.Pow(10, decimales);
        if (escalado != decimal.Truncate(escalado))
        {
            throw new ValidacionException(campo, $"Admite hasta {decimales} decimales.");
        }
    }

    public static void ValidarPositivo(string campo, decimal valor)
    {
        if (valor <= 0)
        {
            throw new ValidacionException(campo, "Debe ser mayor a cero.");
        }
    }

    /// <summary>FR-028: se admiten fechas pasadas y se rechazan futuras.</summary>
    public static void ValidarFechaNoFutura(string campo, DateOnly fecha, DateOnly hoy)
    {
        if (fecha > hoy)
        {
            throw new ValidacionException(campo, "La fecha no puede ser futura.");
        }
    }

    /// <summary>FR-027: nombre obligatorio, guardado sin espacios al inicio ni al final.</summary>
    public static string NormalizarNombre(string? nombre, string campo = "nombre")
    {
        var normalizado = nombre?.Trim() ?? "";
        if (normalizado.Length == 0)
        {
            throw new ValidacionException(campo, "El nombre es obligatorio.");
        }
        if (normalizado.Length > LargoMaximoNombre)
        {
            throw new ValidacionException(campo, $"El nombre admite hasta {LargoMaximoNombre} caracteres.");
        }
        return normalizado;
    }

    /// <summary>FR-030: topes por operación para prevenir errores de carga.</summary>
    public static void ValidarTope(string campo, decimal valor, decimal tope, string unidad)
    {
        if (valor > tope)
        {
            throw new ValidacionException(campo, $"No puede superar el tope de {tope} {unidad}.");
        }
    }

    /// <summary>FR-010, FR-016, FR-022: el stock nunca queda negativo.</summary>
    public static void ValidarStockSuficiente(long disponibleG, long requeridoG)
    {
        if (requeridoG > disponibleG)
        {
            throw new StockInsuficienteException(disponibleG, requeridoG);
        }
    }
}
