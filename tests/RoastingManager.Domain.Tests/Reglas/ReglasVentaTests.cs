using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;

namespace RoastingManager.Domain.Tests.Reglas;

// FR-014, FR-016, FR-030
public class ReglasVentaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Unidades_menores_o_iguales_a_cero_o_sobre_el_tope_se_rechazan(int unidades)
    {
        var ex = Assert.Throws<ValidacionException>(() => ReglasVenta.ValidarUnidades(unidades, 1000));
        Assert.True(ex.Errores.ContainsKey("unidades"));
    }

    [Fact]
    public void Unidades_en_el_tope_se_aceptan()
    {
        ReglasVenta.ValidarUnidades(1000, 1000);
        ReglasVenta.ValidarUnidades(1, 1000);
    }

    [Fact]
    public void Gramos_vendidos_mayores_al_stock_tostado_lanzan_stock_insuficiente()
    {
        var ex = Assert.Throws<StockInsuficienteException>(() => ReglasVenta.ValidarStockTostado(750, 1000));
        Assert.Equal(750, ex.DisponibleG);
        Assert.Equal(1000, ex.RequeridoG);
    }

    [Fact]
    public void Gramos_vendidos_iguales_al_stock_se_aceptan()
    {
        ReglasVenta.ValidarStockTostado(1000, 1000);
    }
}
