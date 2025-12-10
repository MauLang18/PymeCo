using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedido", "dbo");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("Id");

        builder.Property(p => p.ClienteId)
               .HasColumnName("ClienteId")
               .IsRequired();

        builder.Property(p => p.UsuarioId)
               .HasColumnName("UsuarioId")
               .HasMaxLength(450) // ✅ string de Identity
               .IsRequired();

        builder.Property(p => p.Fecha)
               .HasColumnName("Fecha")
               .HasDefaultValueSql("GETDATE()")
               .ValueGeneratedOnAdd();

        builder.Property(p => p.Subtotal)
               .HasColumnName("Subtotal")
               .HasPrecision(18, 2)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(p => p.Impuestos)
               .HasColumnName("Impuestos")
               .HasPrecision(18, 2)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(p => p.Total)
               .HasColumnName("Total")
               .HasPrecision(18, 2)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(p => p.Estado)
               .HasColumnName("EstadoPedido")
               .HasMaxLength(50)
               .HasDefaultValue("Pendiente")
               .IsRequired();

        builder.HasOne(p => p.Cliente)
               .WithMany()
               .HasForeignKey(p => p.ClienteId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Usuario)
               .WithMany()
               .HasForeignKey(p => p.UsuarioId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Detalles)
               .WithOne(d => d.Pedido!)
               .HasForeignKey(d => d.PedidoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Fecha).HasDatabaseName("IX_Pedido_Fecha");
        builder.HasIndex(p => p.Estado).HasDatabaseName("IX_Pedido_Estado");
        builder.HasIndex(p => p.ClienteId).HasDatabaseName("IX_Pedido_ClienteId");
    }
}