using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Tests.Calculos;

// FR-008, FR-011, FR-012, FR-012a
public class CalculoBatchTests
{
    [Fact]
    public void Ejemplo_de_la_spec_5kg_verde_4_25kg_tostado_a_20_por_kg()
    {
        var r = CalculoBatch.Calcular(verdeG: 5000, tostadoG: 4250, precioKg: 20.00m);

        Assert.Equal(15.00m, r.MermaPct);
        Assert.Equal(23.53m, r.CostoTostadoKg);
        Assert.Equal(100.00m, r.CostoTotal);
    }

    [Fact]
    public void Costo_total_se_redondea_alejandose_de_cero()
    {
        // 20,01 × 5,123 kg = 102,51123 → 102,51
        var r = CalculoBatch.Calcular(verdeG: 5123, tostadoG: 4300, precioKg: 20.01m);
        Assert.Equal(102.51m, r.CostoTotal);
    }

    [Fact]
    public void Cantidades_minimas_de_1_gramo_se_calculan_y_redondean()
    {
        var r = CalculoBatch.Calcular(verdeG: 2, tostadoG: 1, precioKg: 20.00m);
        Assert.Equal(50.00m, r.MermaPct);
        Assert.Equal(40.00m, r.CostoTostadoKg);
        Assert.Equal(0.04m, r.CostoTotal);

        var r2 = CalculoBatch.Calcular(verdeG: 1000, tostadoG: 1, precioKg: 20.00m);
        Assert.Equal(99.90m, r2.MermaPct);
        Assert.Equal(20000.00m, r2.CostoTostadoKg);
    }

    [Theory]
    [InlineData(5000, 5000)]
    [InlineData(5000, 6000)]
    public void Tostado_mayor_o_igual_al_verde_se_rechaza(long verdeG, long tostadoG)
    {
        var ex = Assert.Throws<ValidacionException>(() => CalculoBatch.Calcular(verdeG, tostadoG, 20m));
        Assert.True(ex.Errores.ContainsKey("tostadoKg"));
    }

    [Theory]
    [InlineData(0, 100, "verdeKg")]
    [InlineData(-1, 100, "verdeKg")]
    [InlineData(5000, 0, "tostadoKg")]
    [InlineData(5000, -5, "tostadoKg")]
    public void Cantidades_menores_o_iguales_a_cero_se_rechazan(long verdeG, long tostadoG, string campo)
    {
        var ex = Assert.Throws<ValidacionException>(() => CalculoBatch.Calcular(verdeG, tostadoG, 20m));
        Assert.True(ex.Errores.ContainsKey(campo));
    }
}
