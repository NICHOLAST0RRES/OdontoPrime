using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Domain;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Data;

public class AppDbContext : DbContext
{
    //ctor
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    //on model create que chama o Configuration que pega os detalhes de cada entidade
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parametro = Expression.Parameter(entityType.ClrType, "e");
                var propriedade = Expression.Property(parametro, nameof(ISoftDelete.Ativo));
                var lambda = Expression.Lambda(propriedade, parametro);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
    
    // tables 
    
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    
    public DbSet<Convenio> Convenios => Set<Convenio>();
    
    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<TipoProfissional> TipoProfissionais { get; set; }
    
    public DbSet<Consulta> Consultas { get; set; }


    
    

        

}

    
