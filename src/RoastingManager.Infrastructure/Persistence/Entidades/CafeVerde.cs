namespace RoastingManager.Infrastructure.Persistence.Entidades;

/// <summary>Café verde y su saldo: stock verde, pool de tostado y costo promedio ponderado.
/// Su fila es la unidad de bloqueo de las operaciones de stock (research R4).</summary>
public class CafeVerde
{
    public long Id { get; set; }
    public string Nombre { get; set; } = "";
    public long ProveedorCafeId { get; set; }
    public ProveedorCafe ProveedorCafe { get; set; } = null!;
    public decimal PrecioKg { get; set; }
    public long StockInicialG { get; set; }
    public long StockVerdeG { get; set; }
    public long StockTostadoG { get; set; }
    public decimal? CostoPromedioKg { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}
