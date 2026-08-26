using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OdontoPrime.Domain;

namespace OdontoPrime.Infra.Interceptors;

public class SoftDeleteInterceptor :  SaveChangesInterceptor
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

        var entradas = context.ChangeTracker
            .Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entrada in entradas)
        {
            entrada.State = EntityState.Modified;
            entrada.Property(nameof(ISoftDelete.Ativo)).CurrentValue = false;
            entrada.Property(nameof(ISoftDelete.DeletadoEm)).CurrentValue = DateTime.UtcNow;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}