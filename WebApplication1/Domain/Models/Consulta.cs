using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Consulta
{

    public UniqueId Id { get;  }
    private Medico MedicoId { get; set; }
    private Paciente PacienteId { get; }
    private DateOnly Data { get; }
    private string Status { get; set;}


    public Consulta(Medico MedicoId, Paciente PacienteId ,DateOnly Data, string Status )
    {
        this.MedicoId = MedicoId;
        this.PacienteId = PacienteId;
        this.Data = Data;
        this.Status = Status;
    }

 
}

