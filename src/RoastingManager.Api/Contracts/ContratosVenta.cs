namespace RoastingManager.Api.Contracts;

public record InsumoRequest(string? Nombre, long? ProveedorInsumoId, decimal? PrecioUnitario);

public record InsumoResponse(long Id, string Nombre, Referencia ProveedorInsumo, decimal PrecioUnitario);

public record PresentacionRequest(string? Nombre, int? Gramos, long[]? InsumoIds);

public record InsumoDePresentacion(long Id, string Nombre, decimal PrecioUnitario);

public record PresentacionResponse(long Id, string Nombre, int Gramos, IReadOnlyList<InsumoDePresentacion> Insumos);

public record VentaRequest(
    long? ClienteId, long? CafeVerdeId, long? PresentacionId, int? Unidades, decimal? PrecioUnitario, DateOnly? Fecha);

public record PresentacionVendida(long Id, string Nombre, int Gramos);

public record InsumoAplicado(string Nombre, decimal PrecioUnitarioAplicado);

/// <summary>Venta con los valores congelados al registrarla (FR-017, FR-018).</summary>
public record VentaResponse(
    long Id, DateOnly Fecha, Referencia Cliente, Referencia CafeVerde, PresentacionVendida Presentacion,
    int Unidades, decimal PrecioUnitario, decimal CantidadVendidaKg, decimal CostoPromedioKgAplicado,
    IReadOnlyList<InsumoAplicado> Insumos,
    decimal Ingreso, decimal CostoCafeVendido, decimal CostosAsociados, decimal Ganancia, decimal MargenPct);
