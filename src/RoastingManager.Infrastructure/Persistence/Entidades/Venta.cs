namespace RoastingManager.Infrastructure.Persistence.Entidades;

/// <summary>Venta con sus valores congelados al registrarla (FR-017, FR-018).</summary>
public class Venta
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public long CafeVerdeId { get; set; }
    public CafeVerde CafeVerde { get; set; } = null!;
    public long PresentacionId { get; set; }
    public Presentacion Presentacion { get; set; } = null!;
    public int Unidades { get; set; }
    public decimal PrecioUnitario { get; set; }
    public DateOnly Fecha { get; set; }
    public int GramosPresentacion { get; set; }
    public long GramosVendidos { get; set; }
    public decimal CostoPromedioKgAplicado { get; set; }
    public decimal Ingreso { get; set; }
    public decimal CostoCafeVendido { get; set; }
    public decimal CostosAsociados { get; set; }
    public decimal Ganancia { get; set; }
    public decimal MargenPct { get; set; }
    public List<VentaInsumo> Insumos { get; set; } = [];
    public DateTimeOffset CreadoEn { get; set; }
}

public class VentaInsumo
{
    public long VentaId { get; set; }
    public long InsumoId { get; set; }
    public string NombreInsumo { get; set; } = "";
    public decimal PrecioUnitarioAplicado { get; set; }
}
