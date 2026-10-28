using RoastingManager.Domain.Calculos;

namespace RoastingManager.Domain.Tests.Calculos;

// FR-024, research R3: 2 decimales, la mitad se aleja de cero.
public class RedondeoTests
{
    [Theory]
    [InlineData("2.345", "2.35")]
    [InlineData("2.344", "2.34")]
    [InlineData("-2.345", "-2.35")]
    [InlineData("23.5294", "23.53")]
    [InlineData("0.005", "0.01")]
    [InlineData("22.00", "22.00")]
    public void Aplicar_redondea_a_2_decimales_alejandose_de_cero(string valor, string esperado)
    {
        Assert.Equal(decimal.Parse(esperado, System.Globalization.CultureInfo.InvariantCulture),
            Redondeo.Aplicar(decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Aplicar_sobre_un_valor_ya_redondeado_no_lo_cambia()
    {
        var una = Redondeo.Aplicar(23.5294m);
        Assert.Equal(una, Redondeo.Aplicar(una));
    }
}
