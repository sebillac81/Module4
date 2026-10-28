namespace RoastingManager.Infrastructure.Persistence.Entidades;

/// <summary>Forma común de proveedores de café, proveedores de insumos y clientes.</summary>
public abstract class Contacto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}

public class ProveedorCafe : Contacto;

public class ProveedorInsumo : Contacto;

public class Cliente : Contacto;
