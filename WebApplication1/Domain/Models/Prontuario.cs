using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Prontuario
{
    [Key]
    public UniqueId Id;
    private DateTime DataDeAbertura { get;}
    private RegistroEvolucao registroEvolucao { get; set; }


    public Prontuario()
    {
        DataDeAbertura = DateTime.Now;
        
    }
}


