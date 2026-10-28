using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;

namespace RoastingManager.Domain.Tests.Reglas;

// FR-008, FR-010, FR-030
public class ReglasBatchTests
{
    [Fact]
    public void Datos_de_proceso_nulos_se_aceptan()
    {
        ReglasBatch.ValidarDatosProceso(null, null, null);
        ReglasBatch.ValidarDatosProceso(0m, 0.01m, 100m);
        ReglasBatch.ValidarDatosProceso(11.45m, 680.50m, 18.75m);
    }

    [Theory]
    [InlineData("-0.01", null, null, "humedadPct")]
    [InlineData("100.01", null, null, "humedadPct")]
    [InlineData(null, "0", null, "densidadGl")]
    [InlineData(null, "-5", null, "densidadGl")]
    [InlineData(null, null, "-1", "desarrolloPct")]
    [InlineData(null, null, "101", "desarrolloPct")]
    [InlineData("11.456", null, null, "humedadPct")]
    [InlineData(null, "680.505", null, "densidadGl")]
    [InlineData(null, null, "18.751", "desarrolloPct")]
    public void Datos_de_proceso_fuera_de_rango_o_con_mas_de_2_decimales_se_rechazan(
        string? humedad, string? densidad, string? desarrollo, string campo)
    {
        static decimal? D(string? s) => s is null ? null : decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        var ex = Assert.Throws<ValidacionException>(() =>
            ReglasBatch.ValidarDatosProceso(D(humedad), D(densidad), D(desarrollo)));
        Assert.True(ex.Errores.ContainsKey(campo));
    }

    [Fact]
    public void Verde_mayor_al_stock_lanza_stock_insuficiente_con_disponible_y_requerido()
    {
        var ex = Assert.Throws<StockInsuficienteException>(() => ReglasBatch.ValidarStockVerde(3000, 5000));
        Assert.Equal(3000, ex.DisponibleG);
        Assert.Equal(5000, ex.RequeridoG);
    }

    [Fact]
    public void Verde_igual_al_stock_se_acepta()
    {
        ReglasBatch.ValidarStockVerde(5000, 5000);
    }

    [Fact]
    public void Verde_sobre_el_tope_de_batch_se_rechaza()
    {
        var ex = Assert.Throws<ValidacionException>(() => ReglasBatch.ValidarTopeBatch(150m, 100m));
        Assert.Contains("100", ex.Errores["verdeKg"][0]);
        ReglasBatch.ValidarTopeBatch(100m, 100m);
    }

    [Fact]
    public void Ingreso_o_stock_inicial_sobre_el_tope_de_ingreso_se_rechaza()
    {
        var ex = Assert.Throws<ValidacionException>(() => ReglasBatch.ValidarTopeIngreso("cantidadKg", 1000.001m, 1000m));
        Assert.Contains("1000", ex.Errores["cantidadKg"][0]);
        ReglasBatch.ValidarTopeIngreso("stockInicialKg", 1000m, 1000m);
    }
}
