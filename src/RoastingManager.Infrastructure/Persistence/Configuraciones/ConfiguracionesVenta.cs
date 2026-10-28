using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Persistence.Configuraciones;

public class ProveedorInsumoConfiguracion : IEntityTypeConfiguration<ProveedorInsumo>
{
    public void Configure(EntityTypeBuilder<ProveedorInsumo> e) => ConfiguracionContacto.Aplicar(e, "proveedor_insumo");
}

public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> e) => ConfiguracionContacto.Aplicar(e, "cliente");
}

public class InsumoConfiguracion : IEntityTypeConfiguration<Insumo>
{
    public void Configure(EntityTypeBuilder<Insumo> e)
    {
        e.ToTable("insumo");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.PrecioUnitario).HasColumnType("numeric(12,2)");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}

public class PresentacionConfiguracion : IEntityTypeConfiguration<Presentacion>
{
    public void Configure(EntityTypeBuilder<Presentacion> e)
    {
        e.ToTable("presentacion");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        e.HasMany(x => x.Insumos).WithOne().HasForeignKey(x => x.PresentacionId);
    }
}

public class PresentacionInsumoConfiguracion : IEntityTypeConfiguration<PresentacionInsumo>
{
    public void Configure(EntityTypeBuilder<PresentacionInsumo> e)
    {
        e.ToTable("presentacion_insumo");
        e.HasKey(x => new { x.PresentacionId, x.InsumoId });
    }
}

public class VentaConfiguracion : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> e)
    {
        e.ToTable("venta");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.PrecioUnitario).HasColumnType("numeric(12,2)");
        e.Property(x => x.CostoPromedioKgAplicado).HasColumnType("numeric(12,2)");
        e.Property(x => x.Ingreso).HasColumnType("numeric(12,2)");
        e.Property(x => x.CostoCafeVendido).HasColumnType("numeric(12,2)");
        e.Property(x => x.CostosAsociados).HasColumnType("numeric(12,2)");
        e.Property(x => x.Ganancia).HasColumnType("numeric(12,2)");
        e.Property(x => x.MargenPct).HasColumnType("numeric(9,2)");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        e.HasMany(x => x.Insumos).WithOne().HasForeignKey(x => x.VentaId);
    }
}

public class VentaInsumoConfiguracion : IEntityTypeConfiguration<VentaInsumo>
{
    public void Configure(EntityTypeBuilder<VentaInsumo> e)
    {
        e.ToTable("venta_insumo");
        e.HasKey(x => new { x.VentaId, x.InsumoId });
        e.Property(x => x.PrecioUnitarioAplicado).HasColumnType("numeric(12,2)");
    }
}
