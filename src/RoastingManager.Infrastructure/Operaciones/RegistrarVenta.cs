using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoastingManager.Domain.Calculos;
using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;
using RoastingManager.Infrastructure.Persistence;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Operaciones;

/// <summary>Datos ya validados por la capa de API (unidades, tope, precio, fecha).</summary>
public record SolicitudVenta(
    long ClienteId, long CafeVerdeId, long PresentacionId, int Unidades, decimal PrecioUnitario, DateOnly Fecha);

/// <summary>Registro de una venta (FR-014 a FR-018): descuenta el tostado del café vendido sin
/// tocar su costo promedio y congela costos y precios, todo o nada (FR-023).</summary>
public class RegistrarVenta(RoastingDbContext db, ILogger<RegistrarVenta> log)
{
    public async Task<Venta> EjecutarAsync(SolicitudVenta s, CancellationToken ct)
    {
        var cliente = await db.Clientes.FindAsync([s.ClienteId], ct)
            ?? throw new NoEncontradoException("El cliente indicado no existe.", "clienteId");
        var presentacion = await db.Presentaciones
            .Include(p => p.Insumos).ThenInclude(pi => pi.Insumo)
            .SingleOrDefaultAsync(p => p.Id == s.PresentacionId, ct)
            ?? throw new NoEncontradoException("La presentación indicada no existe.", "presentacionId");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var cafe = await BloqueoCafe.ObtenerParaActualizarAsync(db, s.CafeVerdeId, ct);

        var gramosVendidos = (long)s.Unidades * presentacion.Gramos;
        try
        {
            ReglasVenta.ValidarStockTostado(cafe.StockTostadoG, gramosVendidos);
        }
        catch (StockInsuficienteException)
        {
            log.LogWarning("Venta rechazada por stock insuficiente: café {CafeId}, requerido {RequeridoG} g, disponible {DisponibleG} g",
                cafe.Id, gramosVendidos, cafe.StockTostadoG);
            throw;
        }

        // Con stock tostado > 0 el café ya fue tostado, así que tiene costo promedio.
        var costoPromedio = cafe.CostoPromedioKg!.Value;
        var insumos = presentacion.Insumos.Select(pi => pi.Insumo).ToList();
        var calculo = CalculoVenta.Calcular(
            s.PrecioUnitario, s.Unidades, gramosVendidos, costoPromedio, insumos.Select(i => i.PrecioUnitario).ToList());

        cafe.StockTostadoG -= gramosVendidos; // FR-015: el costo promedio no cambia

        var venta = new Venta
        {
            Cliente = cliente,
            CafeVerde = cafe,
            Presentacion = presentacion,
            Unidades = s.Unidades,
            PrecioUnitario = s.PrecioUnitario,
            Fecha = s.Fecha,
            GramosPresentacion = presentacion.Gramos,
            GramosVendidos = gramosVendidos,
            CostoPromedioKgAplicado = costoPromedio,
            Ingreso = calculo.Ingreso,
            CostoCafeVendido = calculo.CostoCafeVendido,
            CostosAsociados = calculo.CostosAsociados,
            Ganancia = calculo.Ganancia,
            MargenPct = calculo.MargenPct,
            Insumos = insumos.Select(i => new VentaInsumo
            {
                InsumoId = i.Id,
                NombreInsumo = i.Nombre,
                PrecioUnitarioAplicado = i.PrecioUnitario,
            }).ToList(),
        };
        db.Ventas.Add(venta);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        log.LogInformation("Venta {VentaId}: café {CafeId}, {GramosVendidos} g; stock tostado {StockTostadoG} g",
            venta.Id, cafe.Id, gramosVendidos, cafe.StockTostadoG);
        return venta;
    }
}
