using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OdontoPrime.Tests;

public class PortaoDeGravacao : SaveChangesInterceptor
{
        private int _esperadas;
        private int _chegaram;
        private TaskCompletionSource? _abrir;

        public void Armar(int quantas)
        {
            _esperadas = quantas;
            _chegaram = 0;
            _abrir = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Desarmar() => _abrir = null;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var abrir = _abrir;
            if (abrir is not null)
            {
                if (Interlocked.Increment(ref _chegaram) == _esperadas)
                    abrir.TrySetResult();   // a última a chegar abre o portão pra todas

                await abrir.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            }

            return result;
        }
    
}