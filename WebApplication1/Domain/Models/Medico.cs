using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Medico : IAuditavel , ISoftDelete
{
    
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Crm { get; set; }
    public DateOnly DataNascimento { get; set; }
    public Especialidade  Especialidade { get; set; }
    public int EspecialidadeId { get; set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public bool Ativo { get; private set; } = true;
    public DateTime? DeletadoEm { get; private set; }

    private Medico()
    {
        
    }

    public Medico(string nome, DateOnly dataNascimento, int EspecialidadeId, string crm)
    {
        this.Nome = nome;
        this.DataNascimento = dataNascimento;   
        this.EspecialidadeId = EspecialidadeId;
        this.Crm = crm ; 
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