namespace OdontoPrime.Tests;

public class RelogioDeTeste : TimeProvider 
{
    
    private DateTimeOffset _agora = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _agora;

    public void Ajustar(DateTimeOffset agora) => _agora = agora.ToUniversalTime();
}

    
