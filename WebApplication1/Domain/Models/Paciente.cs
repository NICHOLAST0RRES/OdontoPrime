using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Paciente : IAuditavel , ISoftDelete
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public int ConvenioId { get; private set; }
    public Convenio Convenio { get; private set; } = null!;

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? DeletadoEm { get; private set; }

    private Paciente()
    {
    }

    public Paciente(string nome, string cpf, string telefone, string email, int convenioId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("CPF é obrigatório.", nameof(cpf));
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new ArgumentException("Telefone é obrigatório.", nameof(telefone));
        }

        Id = Guid.CreateVersion7();
        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
        ConvenioId = convenioId;
        Ativo = true;
    }

    public void Atualizar(string nome, string telefone, string email, int convenioId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new ArgumentException("Telefone é obrigatório.", nameof(telefone));
        }

        Nome = nome;
        Telefone = telefone;
        Email = email;
        ConvenioId = convenioId;
    }

    public void Desativar()
    {
        Ativo = false;
        DeletadoEm = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DeletadoEm = null;
    }


  
}