
namespace OdontoPrime.Application;

public class Result
{
    public bool Sucesso { get; }
    public string? Erro { get; }
    public TipoError? TipoErro { get; }

    protected Result(bool sucesso, string? erro, TipoError? tipoErro)
    {
        Sucesso = sucesso;
        Erro = erro;
        TipoErro = tipoErro;
    }

    public static Result Ok()
    {
        return new Result(true, null, null);
    }

    public static Result Falha(string erro, TipoError tipoErro)
    {
        return new Result(false, erro, tipoErro);
    }
    
}

public class Result<T> : Result
{
    public T? Valor { get; }

    private Result(bool sucesso, T? valor, string? erro, TipoError? tipoErro)
        : base(sucesso, erro, tipoErro)
    {
        Valor = valor;
    }

    public static Result<T> Ok(T valor)                                     // entender isso depois
    {
        return new Result<T>(true, valor, null, null);
    }

    public static new Result<T> Falha(string erro, TipoError tipoErro)
    {
        return new Result<T>(false, default, erro, tipoErro);
    }
}