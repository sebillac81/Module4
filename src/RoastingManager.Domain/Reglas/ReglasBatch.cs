using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Reglas;

public static class ReglasBatch
{
    /// <summary>FR-008: datos de proceso opcionales, en rangos físicos y con hasta 2 decimales.</summary>
    public static void ValidarDatosProceso(decimal? humedadPct, decimal? densidadGl, decimal? desarrolloPct)
    {
        Validacion.Ejecutar(
            () => ValidarPorcentaje("humedadPct", humedadPct),
            () =>
            {
                if (densidadGl is null) return;
                ReglasComunes.ValidarPositivo("densidadGl", densidadGl.Value);
                ReglasComunes.ValidarMaximoDecimales("densidadGl", densidadGl.Value, 2);
            },
            () => ValidarPorcentaje("desarrolloPct", desarrolloPct));
    }

    private static void ValidarPorcentaje(string campo, decimal? valor)
    {
        if (valor is null) return;
        if (valor < 0 || valor > 100)
        {
            throw new ValidacionException(campo, "Debe estar entre 0 y 100.");
        }
        ReglasComunes.ValidarMaximoDecimales(campo, valor.Value, 2);
    }

    /// <summary>FR-010: no se tuesta más café verde que el disponible.</summary>
    public static void ValidarStockVerde(long disponibleG, long requeridoG) =>
        ReglasComunes.ValidarStockSuficiente(disponibleG, requeridoG);

    public static void ValidarTopeBatch(decimal verdeKg, decimal topeKg) =>
        ReglasComunes.ValidarTope("verdeKg", verdeKg, topeKg, "kg");

    /// <summary>FR-030: el tope de ingreso aplica también al stock inicial (FR-007).</summary>
    public static void ValidarTopeIngreso(string campo, decimal kg, decimal topeKg) =>
        ReglasComunes.ValidarTope(campo, kg, topeKg, "kg");
}
