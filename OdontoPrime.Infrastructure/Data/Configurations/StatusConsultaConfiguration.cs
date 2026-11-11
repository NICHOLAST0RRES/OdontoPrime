using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class StatusConsultaConfiguration : IEntityTypeConfiguration<StatusConsulta>
{
    public void Configure(EntityTypeBuilder<StatusConsulta> builder)
    {
        builder.ToTable("StatusConsultas");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new StatusConsulta { Id = StatusConsulta.AgendadaId, Nome = "Agendada" },
            new StatusConsulta { Id = StatusConsulta.RealizadaId, Nome = "Realizada" },
            new StatusConsulta { Id = StatusConsulta.CanceladaId, Nome = "Cancelada" }
        );
    }
    
}