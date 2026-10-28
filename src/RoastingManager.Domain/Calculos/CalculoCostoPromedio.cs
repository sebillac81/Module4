namespace RoastingManager.Domain.Calculos;

public static class CalculoCostoPromedio
{
    /// <summary>Costo promedio ponderado del stock tostado de un café tras sumar un batch
    /// (FR-013). Con stock existente 0 el resultado es el costo del batch.</summary>
    public static decimal Calcular(long stockG, decimal? costoPromedioActual, long tostadoG, decimal costoTostadoKg)
    {
        if (stockG == 0 || costoPromedioActual is null)
        {
            return costoTostadoKg;
        }
        return Redondeo.Aplicar(
            (stockG * costoPromedioActual.Value + tostadoG * costoTostadoKg) / (stockG + tostadoG));
    }
}
