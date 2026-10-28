using Microsoft.EntityFrameworkCore;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Persistence;

/// <summary>Mapea el esquema creado por DbUp; EF no genera migraciones.</summary>
public class RoastingDbContext(DbContextOptions<RoastingDbContext> options) : DbContext(options)
{
    public DbSet<ProveedorCafe> ProveedoresCafe => Set<ProveedorCafe>();
    public DbSet<CafeVerde> CafesVerdes => Set<CafeVerde>();
    public DbSet<IngresoStock> IngresosStock => Set<IngresoStock>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<ProveedorInsumo> ProveedoresInsumo => Set<ProveedorInsumo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Insumo> Insumos => Set<Insumo>();
    public DbSet<Presentacion> Presentaciones => Set<Presentacion>();
    public DbSet<Venta> Ventas => Set<Venta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoastingDbContext).Assembly);
    }
}
