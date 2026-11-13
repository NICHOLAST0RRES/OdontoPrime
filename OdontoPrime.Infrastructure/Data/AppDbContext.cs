using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OdontoPrime.Application;
using OdontoPrime.Domain;
using OdontoPrime.Domain.Models;
using OdontoPrime.Infra.Mensageria.Outbox;

namespace OdontoPrime.Data;

public class AppDbContext : DbContext , IClinicaDbContext
{
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        const int tentativas = 5;

        for (var tentativa = 1; ; tentativa++)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                               {
                                                   SqlState: PostgresErrorCodes.ExclusionViolation,
                                                   ConstraintName: "EX_Consultas_SemSobreposicao"
                                               })
            {
                throw new HorarioOcupadoException(ex);
            }
            catch (DbUpdateException ex) when (tentativa < tentativas && ex.InnerException is PostgresException
                                               {
                                                   SqlState: PostgresErrorCodes.DeadlockDetected
                                               })
            {
                // O Postgres desfez a transação inteira pra quebrar o impasse. Tentar de novo é seguro:
                // o EF mantém as entidades como novas, e a nova tentativa enxerga quem venceu.
            }
        }
    }
    
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
    
    public DbSet<OutboxMensagem> OutboxMensagens => Set<OutboxMensagem>();
    public DbSet<Profissional> Profissionais { get; set; }
    public DbSet<TipoProfissional> TipoProfissionais { get; set; }
    
    public DbSet<Consulta> Consultas { get; set; }
    
    public DbSet<TipoAtendimento> TiposAtendimento => Set<TipoAtendimento>();


    
    

        

}

    
