namespace RoastingManager.Infrastructure.Persistence.Entidades;

public class Batch
{
    public long Id { get; set; }
    public long CafeVerdeId { get; set; }
    public CafeVerde CafeVerde { get; set; } = null!;
    public long VerdeG { get; set; }
    public long TostadoG { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal? HumedadPct { get; set; }
    public decimal? DensidadGl { get; set; }
    public decimal? DesarrolloPct { get; set; }
    public decimal PrecioKgAplicado { get; set; }
    public decimal MermaPct { get; set; }
    public decimal CostoTostadoKg { get; set; }
    public decimal CostoTotal { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}
