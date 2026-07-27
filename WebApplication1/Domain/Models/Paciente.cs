using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Paciente : IAuditavel , ISoftDelete
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Cpf { get; private set; }
    public string Telefone { get; private set; }
    public string Email { get; private set; }
    public int ConvenioId { get; private set; }
    public Convenio Convenio { get; private set; }
    
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public bool Ativo { get; private set; } = true;
    public DateTime? DeletadoEm { get; private set; }


    private Paciente()
    {
        
    }  


    public Paciente(string nome, string cpf, string telefone, string email,  int ConvenioId)
    {
        this.Nome = nome;
        this.Cpf = cpf;
        this.Telefone = telefone;
        this.Email = email;
        this.ConvenioId = ConvenioId;
        this.Ativo = true;
    }
    
    public void Desativar()
    {
        Ativo = false;
        DeletadoEm = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DeletadoEm = null ;
    }


  
}