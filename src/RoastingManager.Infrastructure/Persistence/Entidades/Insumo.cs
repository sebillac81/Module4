namespace RoastingManager.Infrastructure.Persistence.Entidades;

public class Insumo
{
    public long Id { get; set; }
    public string Nombre { get; set; } = "";
    public long ProveedorInsumoId { get; set; }
    public ProveedorInsumo ProveedorInsumo { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}
