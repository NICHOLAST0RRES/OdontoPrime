using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain;
using WebApplication1.Domain.Models;

namespace WebApplication1.Data;

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
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Especialidade> Especialidades => Set<Especialidade>();
    
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    
    public DbSet<Convenio> Convenios => Set<Convenio>();

    
    

        

}

    
