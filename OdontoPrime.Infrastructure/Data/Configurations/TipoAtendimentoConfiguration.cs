using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class TipoAtendimentoConfiguration : IEntityTypeConfiguration<TipoAtendimento>
{
    public void Configure(EntityTypeBuilder<TipoAtendimento> builder)
    {
        builder.ToTable("TiposAtendimento");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Nome).IsRequired().HasMaxLength(50);

        builder.HasData(
            new TipoAtendimento { Id = TipoAtendimento.AvaliacaoId,   Nome = "Avaliação",   DuracaoPadraoMinutos = 30, DuracaoMaximaMinutos = 60 },
            new TipoAtendimento { Id = TipoAtendimento.LimpezaId,     Nome = "Limpeza",     DuracaoPadraoMinutos = 45, DuracaoMaximaMinutos = 90 },
            new TipoAtendimento { Id = TipoAtendimento.RestauracaoId, Nome = "Restauração", DuracaoPadraoMinutos = 60, DuracaoMaximaMinutos = 120 },
            new TipoAtendimento { Id = TipoAtendimento.CanalId,       Nome = "Canal",       DuracaoPadraoMinutos = 90, DuracaoMaximaMinutos = 180 },
            new TipoAtendimento { Id = TipoAtendimento.UrgenciaId,    Nome = "Urgência",    DuracaoPadraoMinutos = 30, DuracaoMaximaMinutos = 60 });
    }
}