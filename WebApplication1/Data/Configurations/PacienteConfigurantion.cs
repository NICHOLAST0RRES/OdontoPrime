using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Models;

namespace WebApplication1.Data.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Cpf).HasMaxLength(11).IsRequired();
        builder.HasIndex(p => p.Cpf).IsUnique();
        builder.HasQueryFilter(p => p.Ativo);

        
    }
}