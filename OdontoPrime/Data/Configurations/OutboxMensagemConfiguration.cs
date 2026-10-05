using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Infra.Mensageria.Outbox;

namespace OdontoPrime.Data.Configurations;

public class OutboxMensagemConfiguration : IEntityTypeConfiguration<OutboxMensagem>
{
    public void Configure(EntityTypeBuilder<OutboxMensagem> builder)
    {
        builder.ToTable("OutboxMensagens");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Tipo).HasMaxLength(100).IsRequired();
        builder.Property(m => m.RoutingKey).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.CriadoEm).IsRequired();
        builder.Property(m => m.UltimoErro).HasMaxLength(2000);

        // Só as pendentes entram no índice: é tudo que o relay procura.
        builder.HasIndex(m => m.CriadoEm)
            .HasFilter("\"PublicadoEm\" IS NULL")
            .HasDatabaseName("IX_OutboxMensagens_Pendentes");
    }
}
