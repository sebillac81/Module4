namespace RoastingManager.Infrastructure.Persistence.Entidades;

public class IngresoStock
{
    public long Id { get; set; }
    public long CafeVerdeId { get; set; }
    public CafeVerde CafeVerde { get; set; } = null!;
    public long CantidadG { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}
