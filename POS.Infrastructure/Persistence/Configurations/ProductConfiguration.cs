using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using POS.Domain.Entities;
using POS.Domain.Enums;

namespace POS.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Producto", "dbo");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("Id");

        builder.Property(p => p.Name).HasColumnName("Nombre").HasMaxLength(200).IsRequired();

        builder.Property(p => p.CategoryId).HasColumnName("CategoriaId").IsRequired();

        builder.Property(p => p.Price).HasColumnName("Precio").HasPrecision(18, 2).IsRequired();

        builder
            .Property(p => p.TaxPercent)
            .HasColumnName("ImpuestoPorc")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(p => p.Stock).HasColumnName("Stock").IsRequired();

        builder.Property(p => p.ImageUrl).HasColumnName("ImagenUrl").HasMaxLength(512);

        // Map enum <-> string ("Activo"/"Inactivo") and accept legacy values ("1","true")
        var statusConverter = new ValueConverter<ProductStatus, string>(
            toDb => toDb == ProductStatus.Active ? "Activo" : "Inactivo",
            fromDb =>
                string.Equals(fromDb, "Activo", StringComparison.OrdinalIgnoreCase)
                || fromDb == "1"
                || string.Equals(fromDb, "true", StringComparison.OrdinalIgnoreCase)
                    ? ProductStatus.Active
                    : ProductStatus.Inactive
        );

        builder
            .Property(p => p.Status)
            .HasColumnName("EstadoProducto")
            .HasConversion(statusConverter)
            .HasMaxLength(10)
            .IsRequired();

        // Timestamps – let SQL Server set defaults; UpdatedAt handled in SaveChanges
        builder
            .Property(p => p.CreatedAt)
            .HasColumnName("FechaRegistro")
            .HasDefaultValueSql("GETDATE()")
            .ValueGeneratedOnAdd();

        builder
            .Property(p => p.UpdatedAt)
            .HasColumnName("FechaModificacion")
            .HasDefaultValueSql("GETDATE()")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(p => p.Name).HasDatabaseName("IX_Producto_Nombre");
    }
}
