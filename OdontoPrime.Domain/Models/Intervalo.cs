namespace OdontoPrime.Domain.Models;

public sealed record Intervalo
{
    public DateTime Inicio { get; }
    public DateTime Fim { get; }

    // ctor recusa um intervalo com o fim antes do início ou igual a ele
    public Intervalo(DateTime inicio, DateTime fim)
    {
        if (fim <= inicio)
            throw new ArgumentException("O fim do intervalo tem que ser depois do início.", nameof(fim));

        Inicio = inicio;
        Fim = fim;
    }

    public TimeSpan Duracao => Fim - Inicio;

    // Diz se os dois intervalos se cruzam
    public bool Sobrepoe(Intervalo outro) => Inicio < outro.Fim && outro.Inicio < Fim;

    // Diz se o outro intervalo cabe inteiro dentro deste
    public bool Contem(Intervalo outro) => Inicio <= outro.Inicio && outro.Fim <= Fim;

    // Devolve o que sobra deste intervalo depois de tirar o outro: nenhum, um ou dois pedaços
    public IReadOnlyList<Intervalo> Subtrair(Intervalo outro)
    {
        if (!Sobrepoe(outro))
            return [this];

        var pedacos = new List<Intervalo>(2);

        if (Inicio < outro.Inicio)
            pedacos.Add(new Intervalo(Inicio, outro.Inicio)); // sobra antes

        if (outro.Fim < Fim)
            pedacos.Add(new Intervalo(outro.Fim, Fim)); // sobra depois

        return pedacos;
    }
}