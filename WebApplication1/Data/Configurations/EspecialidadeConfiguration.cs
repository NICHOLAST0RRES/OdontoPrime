using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Models;

namespace WebApplication1.Data.Configurations;

public class EspecialidadeConfiguration  : IEntityTypeConfiguration<Especialidade>
{
    public void Configure(EntityTypeBuilder<Especialidade> builder)
    {
        builder.ToTable("Especialidades");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(128);
    }
}