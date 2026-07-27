using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Models;

namespace WebApplication1.Data.Configurations;



public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("Medicos");
        
        builder.Property(p => p.Nome).HasMaxLength(128).IsRequired();
        builder.Property(p => p.Crm).HasMaxLength(128).IsRequired();
        builder.Property(p => p.DataNascimento).IsRequired();
        builder.HasQueryFilter(p => p.Ativo);

        
        
    }
    
}