namespace OdontoPrime.Domain.Models;

public enum SituacaoDoHorario
{
    Livre,
    ForaDaJornada,
    Bloqueado
}

// Decide se um período pode receber consulta, olhando a jornada do dia e os bloqueios do dentista.
public static class RegraDaAgenda
{
    public static SituacaoDoHorario Avaliar(
        Intervalo periodo,
        IReadOnlyList<Intervalo> jornadaDoDia,
        IReadOnlyList<Intervalo> bloqueios)
    {
        // Vem primeiro: um bloqueio não aceita encaixe.
        if (bloqueios.Any(b => b.Sobrepoe(periodo)))
            return SituacaoDoHorario.Bloqueado;

        // A consulta tem que caber inteira em um dos períodos da jornada.
        if (!jornadaDoDia.Any(j => j.Contem(periodo)))
            return SituacaoDoHorario.ForaDaJornada;

        return SituacaoDoHorario.Livre;
    }
}
