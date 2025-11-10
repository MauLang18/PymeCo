using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Persistence.COnfigurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.ToTable("Cliente", "dbo");

        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("Id");

        b.Property(c => c.Name).HasColumnName("Nombre").HasMaxLength(200).IsRequired();

        b.Property(c => c.NationalId).HasColumnName("Cedula").HasMaxLength(40);

        b.Property(c => c.Email).HasColumnName("Correo").HasMaxLength(200);

        b.Property(c => c.Phone).HasColumnName("Telefono").HasMaxLength(50);

        b.Property(c => c.Address).HasColumnName("Direccion").HasMaxLength(500);

        b.Property(c => c.CreatedAt)
            .HasColumnName("FechaRegistro")
            .HasDefaultValueSql("GETDATE()")
            .ValueGeneratedOnAdd();

        b.Property(c => c.UpdatedAt)
            .HasColumnName("FechaModificacion")
            .HasDefaultValueSql("GETDATE()")
            .ValueGeneratedOnAddOrUpdate();

        b.HasIndex(c => c.NationalId).HasDatabaseName("IX_Cliente_Cedula");
    }
}
