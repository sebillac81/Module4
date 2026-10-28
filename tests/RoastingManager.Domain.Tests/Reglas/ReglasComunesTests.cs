using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;

namespace RoastingManager.Domain.Tests.Reglas;

public class ReglasComunesTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 27);

    // FR-029
    [Theory]
    [InlineData("0.01")]
    [InlineData("20")]
    [InlineData("15.50")]
    public void ValidarMonto_acepta_montos_de_hasta_2_decimales_desde_0_01(string monto)
    {
        ReglasComunes.ValidarMonto("precioKg", decimal.Parse(monto, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.005")]
    public void ValidarMonto_rechaza_montos_invalidos(string monto)
    {
        var ex = Assert.Throws<ValidacionException>(() =>
            ReglasComunes.ValidarMonto("precioKg", decimal.Parse(monto, System.Globalization.CultureInfo.InvariantCulture)));
        Assert.True(ex.Errores.ContainsKey("precioKg"));
    }

    // FR-028
    [Fact]
    public void ValidarFechaNoFutura_rechaza_mañana_y_acepta_hoy_y_ayer()
    {
        Assert.Throws<ValidacionException>(() => ReglasComunes.ValidarFechaNoFutura("fecha", Hoy.AddDays(1), Hoy));
        ReglasComunes.ValidarFechaNoFutura("fecha", Hoy, Hoy);
        ReglasComunes.ValidarFechaNoFutura("fecha", Hoy.AddDays(-1), Hoy);
    }

    // FR-027
    [Fact]
    public void NormalizarNombre_quita_espacios_al_inicio_y_al_final()
    {
        Assert.Equal("Colombia", ReglasComunes.NormalizarNombre("  Colombia "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NormalizarNombre_rechaza_nombre_vacio(string? nombre)
    {
        var ex = Assert.Throws<ValidacionException>(() => ReglasComunes.NormalizarNombre(nombre));
        Assert.True(ex.Errores.ContainsKey("nombre"));
    }

    // FR-030
    [Fact]
    public void ValidarTope_rechaza_un_valor_sobre_el_tope_informando_el_tope()
    {
        var ex = Assert.Throws<ValidacionException>(() => ReglasComunes.ValidarTope("verdeKg", 150m, 100m, "kg"));
        Assert.Contains("100", ex.Errores["verdeKg"][0]);
        ReglasComunes.ValidarTope("verdeKg", 100m, 100m, "kg");
    }

    // FR-025: todos los errores de datos se informan juntos.
    [Fact]
    public void Validacion_Ejecutar_junta_los_errores_de_todas_las_reglas()
    {
        var ex = Assert.Throws<ValidacionException>(() => Validacion.Ejecutar(
            () => ReglasComunes.ValidarMonto("precioKg", 0m),
            () => ReglasComunes.NormalizarNombre(""),
            () => ReglasComunes.ValidarFechaNoFutura("fecha", Hoy, Hoy)));
        Assert.Equal(2, ex.Errores.Count);
        Assert.True(ex.Errores.ContainsKey("precioKg"));
        Assert.True(ex.Errores.ContainsKey("nombre"));
    }
}
