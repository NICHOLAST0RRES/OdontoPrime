using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class ProfissionalConfiguration : IEntityTypeConfiguration<Profissional>
{
    public void Configure(EntityTypeBuilder<Profissional> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Telefone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Cro)
            .HasMaxLength(20);

        builder.Property(p => p.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(p => p.Cro)
            .IsUnique()
            .HasFilter("\"Cro\" IS NOT NULL");

        builder.HasQueryFilter(p => p.Ativo);

        builder.HasOne(p => p.TipoProfissional)
            .WithMany()
            .HasForeignKey(p => p.TipoProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

