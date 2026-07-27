using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public abstract class Remedio
{
    [Key]
    public UniqueId Id;
    private String Nome ;
    private String Bula;
    private String Validade;
    

    public Remedio(String nome, String bula, String validade)
    {
        this.Nome = nome;
        this.Bula = bula;
        this.Validade = validade;
        
    }
    
    
}