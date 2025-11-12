using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "dbo");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("Id");

        builder.Property(u => u.Nombre)
               .HasColumnName("Nombre")
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(u => u.EstadoUsuario)
               .HasColumnName("EstadoUsuario")
               .HasMaxLength(20)
               .HasDefaultValue("Activo")
               .IsRequired();

        builder.Property(u => u.RolId)
               .HasColumnName("RolId");

        builder.HasOne(u => u.Rol)
               .WithMany()
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(u => u.RolId)
               .HasDatabaseName("IX_Usuario_RolId");
    }
}
