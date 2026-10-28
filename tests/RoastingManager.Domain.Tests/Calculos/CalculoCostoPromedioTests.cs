using RoastingManager.Domain.Calculos;

namespace RoastingManager.Domain.Tests.Calculos;

// FR-013, FR-024
public class CalculoCostoPromedioTests
{
    [Fact]
    public void Ejemplo_de_la_spec_4kg_a_20_mas_2kg_a_26_da_22()
    {
        Assert.Equal(22.00m, CalculoCostoPromedio.Calcular(
            stockG: 4000, costoPromedioActual: 20.00m, tostadoG: 2000, costoTostadoKg: 26.00m));
    }

    [Fact]
    public void Con_stock_0_y_sin_costo_previo_toma_el_costo_del_batch()
    {
        Assert.Equal(23.53m, CalculoCostoPromedio.Calcular(0, null, 4250, 23.53m));
    }

    [Fact]
    public void Con_stock_0_y_costo_previo_toma_el_costo_del_batch()
    {
        Assert.Equal(30.00m, CalculoCostoPromedio.Calcular(0, 22.00m, 1000, 30.00m));
    }

    [Fact]
    public void Usa_el_costo_del_batch_ya_redondeado_y_redondea_el_resultado()
    {
        // (3250 × 23,53 + 4250 × 23,53) / 7500 = 23,53
        Assert.Equal(23.53m, CalculoCostoPromedio.Calcular(3250, 23.53m, 4250, 23.53m));
        // (1000 × 20,00 + 2000 × 21,01) / 3000 = 20,6733… → 20,67
        Assert.Equal(20.67m, CalculoCostoPromedio.Calcular(1000, 20.00m, 2000, 21.01m));
    }
}
