namespace RoastingManager.Api.Contracts;

// Formas de request/response de contracts/api.md. Los requests usan tipos anulables para
// informar "obligatorio" por campo (FR-025). Las cantidades viajan en kg.

public record Referencia(long Id, string Nombre);

public record ContactoRequest(string? Nombre, string? Telefono, string? Email, string? Direccion);

public record ContactoResponse(long Id, string Nombre, string? Telefono, string? Email, string? Direccion);

public record CafeVerdeRequest(string? Nombre, long? ProveedorCafeId, decimal? PrecioKg, decimal? StockInicialKg);

public record CafeVerdeResponse(long Id, string Nombre, Referencia ProveedorCafe, decimal PrecioKg, decimal StockInicialKg);

public record IngresoRequest(long? CafeVerdeId, decimal? CantidadKg, DateOnly? Fecha);

/// <summary><see cref="StockVerdeResultanteKg"/> solo se informa en el alta.</summary>
public record IngresoResponse(
    long Id, long CafeVerdeId, string CafeVerde, decimal CantidadKg, DateOnly Fecha, decimal? StockVerdeResultanteKg);

public record BatchRequest(
    long? CafeVerdeId, decimal? VerdeKg, decimal? TostadoKg, DateOnly? Fecha,
    decimal? HumedadPct, decimal? DensidadGl, decimal? DesarrolloPct);

public record BatchResponse(
    long Id, long CafeVerdeId, string CafeVerde, DateOnly Fecha, decimal VerdeKg, decimal TostadoKg,
    decimal? HumedadPct, decimal? DensidadGl, decimal? DesarrolloPct,
    decimal PrecioKgAplicado, decimal MermaPct, decimal CostoTostadoKg, decimal CostoTotal);

/// <summary><see cref="CostoPromedioKg"/> es null si el café nunca se tostó (FR-019).</summary>
public record StockResponse(
    long CafeVerdeId, string CafeVerde, decimal StockVerdeKg, decimal StockTostadoKg, decimal? CostoPromedioKg);
