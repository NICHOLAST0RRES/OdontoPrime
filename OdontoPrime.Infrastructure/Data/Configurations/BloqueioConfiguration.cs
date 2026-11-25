using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class BloqueioConfiguration : IEntityTypeConfiguration<Bloqueio>
{
    public void Configure(EntityTypeBuilder<Bloqueio> builder)
    {
        builder.ToTable("Bloqueios");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Motivo)
            .IsRequired()
            .HasMaxLength(Bloqueio.TamanhoMaximoDoMotivo);

        builder.Ignore(b => b.Periodo);

        builder.HasOne<Profissional>()
            .WithMany()
            .HasForeignKey(b => b.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);

        // A busca da parte D é sempre "bloqueios deste dentista neste período".
        builder.HasIndex(b => new { b.ProfissionalId, b.Inicio });
    }
    
}