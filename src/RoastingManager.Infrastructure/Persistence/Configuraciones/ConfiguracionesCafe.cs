using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoastingManager.Infrastructure.Persistence.Entidades;

namespace RoastingManager.Infrastructure.Persistence.Configuraciones;

/// <summary>Mapeo común de las tablas de contactos (proveedores y clientes).</summary>
internal static class ConfiguracionContacto
{
    public static void Aplicar<T>(EntityTypeBuilder<T> e, string tabla) where T : Contacto
    {
        e.ToTable(tabla);
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}

public class ProveedorCafeConfiguracion : IEntityTypeConfiguration<ProveedorCafe>
{
    public void Configure(EntityTypeBuilder<ProveedorCafe> e) => ConfiguracionContacto.Aplicar(e, "proveedor_cafe");
}

public class CafeVerdeConfiguracion : IEntityTypeConfiguration<CafeVerde>
{
    public void Configure(EntityTypeBuilder<CafeVerde> e)
    {
        e.ToTable("cafe_verde");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.PrecioKg).HasColumnType("numeric(12,2)");
        e.Property(x => x.CostoPromedioKg).HasColumnType("numeric(12,2)");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}

public class IngresoStockConfiguracion : IEntityTypeConfiguration<IngresoStock>
{
    public void Configure(EntityTypeBuilder<IngresoStock> e)
    {
        e.ToTable("ingreso_stock");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}

public class BatchConfiguracion : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> e)
    {
        e.ToTable("batch");
        e.Property(x => x.Id).UseIdentityAlwaysColumn();
        e.Property(x => x.HumedadPct).HasColumnType("numeric(5,2)");
        e.Property(x => x.DensidadGl).HasColumnType("numeric(8,2)");
        e.Property(x => x.DesarrolloPct).HasColumnType("numeric(5,2)");
        e.Property(x => x.PrecioKgAplicado).HasColumnType("numeric(12,2)");
        e.Property(x => x.MermaPct).HasColumnType("numeric(9,2)");
        e.Property(x => x.CostoTostadoKg).HasColumnType("numeric(12,2)");
        e.Property(x => x.CostoTotal).HasColumnType("numeric(12,2)");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
    }
}
