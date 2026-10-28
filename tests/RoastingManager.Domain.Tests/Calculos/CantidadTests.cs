using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Tests.Calculos;

// FR-026: gramos enteros internos, kg con hasta 3 decimales en la API.
public class CantidadTests
{
    [Theory]
    [InlineData("5.250", 5250)]
    [InlineData("69", 69000)]
    [InlineData("0.001", 1)]
    [InlineData("0", 0)]
    public void DesdeKg_convierte_a_gramos_exactos(string kg, long gramos)
    {
        Assert.Equal(gramos, Cantidad.DesdeKg(decimal.Parse(kg, System.Globalization.CultureInfo.InvariantCulture), "cantidadKg"));
    }

    [Fact]
    public void DesdeKg_con_mas_de_3_decimales_se_rechaza_indicando_el_campo()
    {
        var ex = Assert.Throws<ValidacionException>(() => Cantidad.DesdeKg(0.0005m, "verdeKg"));
        Assert.True(ex.Errores.ContainsKey("verdeKg"));
    }

    [Fact]
    public void AKg_convierte_gramos_a_kg()
    {
        Assert.Equal(4.250m, Cantidad.AKg(4250));
        Assert.Equal(0.001m, Cantidad.AKg(1));
    }
}
