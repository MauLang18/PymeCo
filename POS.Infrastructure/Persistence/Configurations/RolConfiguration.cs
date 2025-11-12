using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Rol", "dbo");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("Id");

        builder.Property(r => r.Nombre)
               .HasColumnName("Nombre")
               .HasMaxLength(50)
               .IsRequired();

        builder.HasIndex(r => r.Nombre)
               .IsUnique()
               .HasDatabaseName("UQ_Rol_Nombre");

        builder.HasData(
                new Rol { Id = 1, Nombre = "Admin" },
                new Rol { Id = 2, Nombre = "Vendedor" }
);
    }
}
