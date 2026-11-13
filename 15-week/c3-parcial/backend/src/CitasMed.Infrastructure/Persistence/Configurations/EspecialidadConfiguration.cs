using CitasMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitasMed.Infrastructure.Persistence.Configurations;

public class EspecialidadConfiguration : IEntityTypeConfiguration<Especialidad>
{
    public void Configure(EntityTypeBuilder<Especialidad> builder)
    {
        builder.ToTable("Especialidades");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(80);
        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}
