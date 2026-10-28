namespace RoastingManager.Domain.Calculos;

public record ResultadoVenta(
    decimal Ingreso, decimal CostoCafeVendido, decimal CostosAsociados, decimal Ganancia, decimal MargenPct);

public static class CalculoVenta
{
    /// <summary>FR-017 y FR-018, en ese orden; cada valor se redondea antes de usarse en el
    /// siguiente (FR-024).</summary>
    public static ResultadoVenta Calcular(decimal precioUnitario, int unidades, long gramosVendidos,
        decimal costoPromedioKg, IReadOnlyCollection<decimal> preciosInsumos)
    {
        var ingreso = Redondeo.Aplicar(precioUnitario * unidades);
        var costoCafe = Redondeo.Aplicar(costoPromedioKg * gramosVendidos / 1000m);
        var costosAsociados = Redondeo.Aplicar(unidades * preciosInsumos.Sum());
        var ganancia = Redondeo.Aplicar(ingreso - (costoCafe + costosAsociados));
        var margen = Redondeo.Aplicar(ganancia / ingreso * 100m);
        return new ResultadoVenta(ingreso, costoCafe, costosAsociados, ganancia, margen);
    }
}
