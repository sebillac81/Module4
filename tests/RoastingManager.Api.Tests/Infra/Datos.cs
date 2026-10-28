using System.Text.Json;

namespace RoastingManager.Api.Tests.Infra;

/// <summary>Altas de catálogo usadas por varios tests.</summary>
public static class Datos
{
    public static async Task<long> ProveedorCafe(HttpClient c, string nombre = "Finca Huila") =>
        (await c.PostOk("/api/proveedores-cafe", new { nombre })).Id();

    public static async Task<long> CafeVerde(HttpClient c, string nombre = "Colombia", decimal precioKg = 20.00m,
        decimal stockInicialKg = 10m, long? proveedorId = null)
    {
        var proveedor = proveedorId ?? await ProveedorCafe(c, $"Proveedor de {nombre}");
        return (await c.PostOk("/api/cafes-verdes",
            new { nombre, proveedorCafeId = proveedor, precioKg, stockInicialKg })).Id();
    }

    public static Task<JsonElement> Batch(HttpClient c, long cafeId, decimal verdeKg, decimal tostadoKg) =>
        c.PostOk("/api/batches", new { cafeVerdeId = cafeId, verdeKg, tostadoKg });

    public static async Task<JsonElement> StockDe(HttpClient c, long cafeId)
    {
        var stock = await c.GetOk("/api/stock");
        return stock.EnumerateArray().Single(s => s.GetProperty("cafeVerdeId").GetInt64() == cafeId);
    }
}

public static class DatosVenta
{
    public static async Task<long> ProveedorInsumo(HttpClient c, string nombre = "Envases SA") =>
        (await c.PostOk("/api/proveedores-insumo", new { nombre })).Id();

    public static async Task<long> Insumo(HttpClient c, string nombre, decimal precioUnitario, long proveedorId) =>
        (await c.PostOk("/api/insumos", new { nombre, proveedorInsumoId = proveedorId, precioUnitario })).Id();

    public static async Task<long> Cliente(HttpClient c, string nombre = "Bar Central") =>
        (await c.PostOk("/api/clientes", new { nombre })).Id();

    /// <summary>"Bolsa 250 g" con bolsa ($0,50) y etiqueta ($0,10), como en la spec.</summary>
    public static async Task<long> Bolsa250(HttpClient c)
    {
        var proveedor = await ProveedorInsumo(c);
        var bolsa = await Insumo(c, "Bolsa", 0.50m, proveedor);
        var etiqueta = await Insumo(c, "Etiqueta", 0.10m, proveedor);
        return (await c.PostOk("/api/presentaciones",
            new { nombre = "Bolsa 250 g", gramos = 250, insumoIds = new[] { bolsa, etiqueta } })).Id();
    }

    /// <summary>Café con 5 kg de tostado a $22,00/kg: 6,25 kg de verde a $17,60/kg → 5 kg tostado.</summary>
    public static async Task<long> CafeCon5KgTostadoA22(HttpClient c, string nombre = "Colombia")
    {
        var cafe = await Datos.CafeVerde(c, nombre, precioKg: 17.60m, stockInicialKg: 10m);
        await Datos.Batch(c, cafe, 6.25m, 5m);
        return cafe;
    }
}
