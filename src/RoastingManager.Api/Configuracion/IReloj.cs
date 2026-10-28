using Microsoft.Extensions.Options;

namespace RoastingManager.Api.Configuracion;

public interface IReloj
{
    DateOnly Hoy();
}

public class RelojSistema(IOptions<AppOptions> opciones, TimeProvider tiempo) : IReloj
{
    private readonly TimeZoneInfo _zona = TimeZoneInfo.FindSystemTimeZoneById(opciones.Value.ZonaHoraria);

    public DateOnly Hoy() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(tiempo.GetUtcNow(), _zona).DateTime);
}
