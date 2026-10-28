using RoastingManager.Api.Tests.Infra;

namespace RoastingManager.Api.Tests;

public class ArranqueTests(ApiFixture fixture) : PruebaApi(fixture)
{
    [Fact]
    public async Task La_api_inicia_aplica_el_esquema_y_expone_los_topes_por_defecto()
    {
        var topes = await Client.GetOk("/api/config/topes");
        Assert.Equal(1000m, topes.Dec("ingresoVerdeMaxKg"));
        Assert.Equal(100m, topes.Dec("batchVerdeMaxKg"));
        Assert.Equal(1000, topes.GetProperty("ventaMaxUnidades").GetInt32());
    }
}
