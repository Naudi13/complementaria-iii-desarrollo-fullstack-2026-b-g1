using CitasMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitasMed.Infrastructure.Persistence.Configurations;

public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("Medicos");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.RegistroProfesional).IsRequired().HasMaxLength(30);
        builder.HasIndex(m => m.RegistroProfesional).IsUnique();

        // 1 a 1: un usuario con rol Medico tiene un perfil de médico.
        builder.HasOne(m => m.User)
               .WithOne(u => u.Medico)
               .HasForeignKey<Medico>(m => m.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Especialidad)
               .WithMany(e => e.Medicos)
               .HasForeignKey(m => m.EspecialidadId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
