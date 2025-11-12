using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence.Configurations;

public class PedidoDetalleConfiguration : IEntityTypeConfiguration<PedidoDetalle>
{
    public void Configure(EntityTypeBuilder<PedidoDetalle> builder)
    {
        builder.ToTable("PedidoDetalle", "dbo");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("Id");

        builder.Property(d => d.PedidoId)
               .HasColumnName("PedidoId")
               .IsRequired();

        builder.Property(d => d.ProductoId)
               .HasColumnName("ProductoId")
               .IsRequired();

        builder.Property(d => d.Cantidad)
               .HasColumnName("Cantidad")
               .IsRequired();

        builder.Property(d => d.PrecioUnit)
               .HasColumnName("PrecioUnitario")
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(d => d.Descuento)
               .HasColumnName("DescuentoPorc")
               .HasPrecision(5, 2)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(d => d.ImpuestoPorc)
               .HasColumnName("ImpuestoPorc")
               .HasPrecision(5, 2)
               .HasDefaultValue(13)
               .IsRequired();

        builder.Property(d => d.TotalLinea)
               .HasColumnName("TotalLinea")
               .HasPrecision(18, 2)
               .IsRequired();

        builder.HasOne(d => d.Producto)
               .WithMany()
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(d => d.ProductoId).HasDatabaseName("IX_PedidoDetalle_ProductoId");
    }
}
