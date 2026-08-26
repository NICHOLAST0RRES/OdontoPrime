using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OdontoPrime.Domain;

namespace OdontoPrime.Infra.Interceptors;

public class AuditoriaInterceptor :  SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var agora = DateTime.UtcNow;

        var entradas = context.ChangeTracker
            .Entries<IAuditavel>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entrada in entradas)
        {
            if (entrada.State == EntityState.Added)
            {
                entrada.Property(nameof(IAuditavel.CriadoEm)).CurrentValue = agora;
            }
            else
            {
                entrada.Property(nameof(IAuditavel.AtualizadoEm)).CurrentValue = agora;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}