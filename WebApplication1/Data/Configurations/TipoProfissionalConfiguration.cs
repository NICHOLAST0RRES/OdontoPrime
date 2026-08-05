using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Models;

namespace WebApplication1.Data.Configurations;

public class TipoProfissionalConfiguration : IEntityTypeConfiguration<TipoProfissional>
{
    public void Configure(EntityTypeBuilder<TipoProfissional> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasData(
            new TipoProfissional { Id = TipoProfissional.DentistaId, Nome = "Dentista" },
            new TipoProfissional { Id = TipoProfissional.AtendenteId, Nome = "Atendente" }
            
        );
    }
}