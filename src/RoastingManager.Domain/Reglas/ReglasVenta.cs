using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Reglas;

public static class ReglasVenta
{
    /// <summary>FR-014 y FR-030: unidades mayores a cero y dentro del tope.</summary>
    public static void ValidarUnidades(int unidades, int tope)
    {
        if (unidades <= 0)
        {
            throw new ValidacionException("unidades", "Debe ser mayor a cero.");
        }
        ReglasComunes.ValidarTope("unidades", unidades, tope, "unidades");
    }

    /// <summary>FR-016: no se vende más café tostado que el disponible de ese café.</summary>
    public static void ValidarStockTostado(long disponibleG, long requeridoG) =>
        ReglasComunes.ValidarStockSuficiente(disponibleG, requeridoG);
}
