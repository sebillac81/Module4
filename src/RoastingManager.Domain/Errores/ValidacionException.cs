namespace RoastingManager.Domain.Errores;

/// <summary>Datos inválidos: uno o más campos con su regla incumplida (FR-025).</summary>
public class ValidacionException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public ValidacionException(string campo, string mensaje)
        : this(new Dictionary<string, string[]> { [campo] = [mensaje] })
    {
    }

    public ValidacionException(IReadOnlyDictionary<string, string[]> errores)
        : base(string.Join(" ", errores.SelectMany(e => e.Value)))
    {
        Errores = errores;
    }
}
