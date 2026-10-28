using RoastingManager.Domain.Calculos;

namespace RoastingManager.Domain.Tests.Calculos;

// FR-017, FR-018, FR-024
public class CalculoVentaTests
{
    private static readonly decimal[] BolsaYEtiqueta = [0.50m, 0.10m];

    [Fact]
    public void Ejemplo_de_la_spec_4_bolsas_de_250g_a_15_con_costo_promedio_22()
    {
        var r = CalculoVenta.Calcular(precioUnitario: 15.00m, unidades: 4, gramosVendidos: 1000,
            costoPromedioKg: 22.00m, preciosInsumos: BolsaYEtiqueta);

        Assert.Equal(60.00m, r.Ingreso);
        Assert.Equal(22.00m, r.CostoCafeVendido);
        Assert.Equal(2.40m, r.CostosAsociados);
        Assert.Equal(35.60m, r.Ganancia);
        Assert.Equal(59.33m, r.MargenPct);
    }

    [Fact]
    public void Con_costo_promedio_23_53_la_ganancia_es_34_07_y_el_margen_56_78()
    {
        var r = CalculoVenta.Calcular(15.00m, 4, 1000, 23.53m, BolsaYEtiqueta);
        Assert.Equal(23.53m, r.CostoCafeVendido);
        Assert.Equal(34.07m, r.Ganancia);
        Assert.Equal(56.78m, r.MargenPct);
    }

    [Fact]
    public void Presentacion_sin_insumos_tiene_costos_asociados_0()
    {
        var r = CalculoVenta.Calcular(15.00m, 4, 1000, 22.00m, []);
        Assert.Equal(0.00m, r.CostosAsociados);
        Assert.Equal(38.00m, r.Ganancia);
    }

    [Fact]
    public void Costo_del_cafe_vendido_se_redondea_antes_de_calcular_la_ganancia()
    {
        // 23,53 × 0,250 kg = 5,8825 → 5,88; ganancia = 10,00 − (5,88 + 0,60) = 3,52
        var r = CalculoVenta.Calcular(10.00m, 1, 250, 23.53m, BolsaYEtiqueta);
        Assert.Equal(5.88m, r.CostoCafeVendido);
        Assert.Equal(3.52m, r.Ganancia);
        Assert.Equal(35.20m, r.MargenPct);
    }

    [Fact]
    public void Precio_bajo_da_ganancia_y_margen_negativos_redondeados_alejandose_de_cero()
    {
        // ingreso 8,00; costo 8,01 → ganancia −0,01; margen −0,125 → −0,13
        var r = CalculoVenta.Calcular(2.00m, 4, 1000, 8.01m, []);
        Assert.Equal(-0.01m, r.Ganancia);
        Assert.Equal(-0.13m, r.MargenPct);
    }
}
