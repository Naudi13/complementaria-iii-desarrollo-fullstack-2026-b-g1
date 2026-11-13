using CitasMed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitasMed.Infrastructure.Persistence.Configurations;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("Citas");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Motivo).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(c => c.Paciente)
               .WithMany(u => u.CitasComoPaciente)
               .HasForeignKey(c => c.PacienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Medico)
               .WithMany(m => m.Citas)
               .HasForeignKey(c => c.MedicoId)
               .OnDelete(DeleteBehavior.Restrict);

        // Acelera la validación de "horario ocupado" (fase 3).
        builder.HasIndex(c => new { c.MedicoId, c.FechaHora });
    }
}
