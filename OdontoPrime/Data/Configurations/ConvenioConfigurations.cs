using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data.Configurations;

public class ConvenioConfigurations: IEntityTypeConfiguration<Convenio>
{
    public void Configure(EntityTypeBuilder<Convenio> builder)
    {
        builder.ToTable("Convenios");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        
    }
}

    
