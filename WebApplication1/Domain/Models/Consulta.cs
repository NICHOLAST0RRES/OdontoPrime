using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace WebApplication1.Domain.Models;

public class Consulta :  IAuditavel, ISoftDelete
{
    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public Paciente Paciente { get; private set; } = null!;
    public Guid ProfissionalId { get; private set; }
    public Profissional Profissional { get; private set; } = null!;
    public int StatusConsultaId { get; private set; }
    public StatusConsulta StatusConsulta { get; private set; } = null!;
    public DateTime DataHora { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime? LembreteEnviadoEm { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? DeletadoEm { get; private set; }

    private Consulta()
    {
    }

    public Consulta(Guid pacienteId, Guid profissionalId, DateTime dataHora, string? observacao)
    {
        if (dataHora <= DateTime.UtcNow)
        {
            throw new ArgumentException("Consulta não pode ser marcada no passado.", nameof(dataHora));
        }

        Id = Guid.CreateVersion7();
        PacienteId = pacienteId;
        ProfissionalId = profissionalId;
        DataHora = dataHora;
        Observacao = observacao;
        StatusConsultaId = StatusConsulta.AgendadaId;
        Ativo = true;
    }

    public void Reagendar(DateTime novaDataHora)
    {
        if (StatusConsultaId != StatusConsulta.AgendadaId)
        {
            throw new InvalidOperationException("Só consulta agendada pode ser reagendada.");
        }

        if (novaDataHora <= DateTime.UtcNow)
        {
            throw new ArgumentException("Consulta não pode ser marcada no passado.", nameof(novaDataHora));
        }

        DataHora = novaDataHora;
        LembreteEnviadoEm = null;
    }

    public void Cancelar()
    {
        if (StatusConsultaId == StatusConsulta.RealizadaId)
        {
            throw new InvalidOperationException("Consulta realizada não pode ser cancelada.");
        }

        StatusConsultaId = StatusConsulta.CanceladaId;
    }

    public void MarcarComoRealizada()
    {
        if (StatusConsultaId != StatusConsulta.AgendadaId)
        {
            throw new InvalidOperationException("Só consulta agendada pode ser marcada como realizada.");
        }

        StatusConsultaId = StatusConsulta.RealizadaId;
    }
    
    public void MarcarLembreteEnviado()
    {
        LembreteEnviadoEm = DateTime.UtcNow;
    }

    public void AtualizarObservacao(string? observacao)
    {
        Observacao = observacao;
    }

 
}

