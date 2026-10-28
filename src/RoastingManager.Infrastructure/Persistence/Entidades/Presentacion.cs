namespace RoastingManager.Infrastructure.Persistence.Entidades;

public class Presentacion
{
    public long Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Gramos { get; set; }
    public List<PresentacionInsumo> Insumos { get; set; } = [];
    public DateTimeOffset CreadoEn { get; set; }
}

/// <summary>Cada insumo aparece una vez por presentación: una unidad por unidad vendida.</summary>
public class PresentacionInsumo
{
    public long PresentacionId { get; set; }
    public long InsumoId { get; set; }
    public Insumo Insumo { get; set; } = null!;
}
