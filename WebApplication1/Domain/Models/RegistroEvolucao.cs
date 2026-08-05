using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class RegistroEvolucao
{
    [Key]
    public UniqueId Id;
    private Profissional ProfissionalId { get;}
    private Paciente PacienteId { get;}

    private string QueixaPrincipal { get; }
    private string Diagnostico { get; }
    public string Observacoes { get;  }       
    private DateTime DataRegistro { get;  }


    public RegistroEvolucao(Profissional  profissional, Paciente paciente, string diagnostico, string observacoes, string QueixaPrincipal, DateTime DataRegistro)
    {
        this.ProfissionalId = profissional;
        this.PacienteId = paciente;
        this.QueixaPrincipal = QueixaPrincipal;
        this.Diagnostico = diagnostico;
        this.Observacoes = observacoes;
        this.DataRegistro = DataRegistro;
        
    }
    

}