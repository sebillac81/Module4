using RoastingManager.Domain.Errores;

namespace RoastingManager.Domain.Reglas;

public static class Validacion
{
    /// <summary>Ejecuta todas las reglas y lanza una única ValidacionException con los errores
    /// de todas, para informarlos juntos (FR-025).</summary>
    public static void Ejecutar(params Action[] reglas)
    {
        var errores = new Dictionary<string, string[]>();
        foreach (var regla in reglas)
        {
            try
            {
                regla();
            }
            catch (ValidacionException ex)
            {
                foreach (var (campo, mensajes) in ex.Errores)
                {
                    errores[campo] = errores.TryGetValue(campo, out var previos)
                        ? [.. previos, .. mensajes]
                        : mensajes;
                }
            }
        }
        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }
    }
}
