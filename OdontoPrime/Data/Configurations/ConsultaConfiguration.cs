using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class ConsultaConfiguration : IEntityTypeConfiguration<Consulta>
{
    public void Configure(EntityTypeBuilder<Consulta> builder)
    {
        builder.ToTable("Consultas");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DataHora)
            .IsRequired();

        builder.Property(c => c.Observacao)
            .HasMaxLength(500);

        builder.Property(c => c.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(c => c.Paciente)
            .WithMany()
            .HasForeignKey(c => c.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Profissional)
            .WithMany()
            .HasForeignKey(c => c.ProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.StatusConsulta)
            .WithMany()
            .HasForeignKey(c => c.StatusConsultaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ProfissionalId, c.DataHora });

        builder.HasQueryFilter(c => c.Ativo);
    }
    
}