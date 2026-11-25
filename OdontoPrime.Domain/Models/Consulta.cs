using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace OdontoPrime.Domain.Models;

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
    public DateTime? LembrarEm { get; private set; }
    public int TipoAtendimentoId { get; private set; }
    public TipoAtendimento TipoAtendimento { get; private set; } = null!;
    public DateTime Fim { get; private set; }
    public TimeSpan Duracao => Fim - DataHora;   // calculada, não vai pro banco

    public const int TamanhoMaximoDoMotivoDeEncaixe = 200;
    public bool Encaixe { get; private set; }
    public string? MotivoEncaixe { get; private set; }
    public Intervalo Periodo => new(DataHora, Fim);   // calculado, não vai pro banco

    private Consulta()
    {
    }
    
    public Consulta(Guid pacienteId, Guid profissionalId, TipoAtendimento tipo, int? duracaoMinutos,
        DateTime dataHora, string? observacao, DateTime agoraUtc, DateTime? lembrarEm)
    {
        if (dataHora <= agoraUtc)
        {
            throw new ArgumentException("Consulta não pode ser marcada no passado.", nameof(dataHora));
        }

        var minutos = duracaoMinutos ?? tipo.DuracaoPadraoMinutos;   // sem ajuste, vale a padrão
        tipo.ValidarDuracao(minutos);

        Id = Guid.CreateVersion7();
        PacienteId = pacienteId;
        ProfissionalId = profissionalId;
        TipoAtendimentoId = tipo.Id;   // só o Id, veja abaixo
        DataHora = dataHora;
        Fim = dataHora.AddMinutes(minutos);
        Observacao = observacao;
        StatusConsultaId = StatusConsulta.AgendadaId;
        Ativo = true;
        LembrarEm = lembrarEm;
    }

    public void Reagendar(DateTime novaDataHora , DateTime agoraUtc , DateTime? lembrarEm )
    {
        if (StatusConsultaId != StatusConsulta.AgendadaId)
        {
            throw new InvalidOperationException("Só consulta agendada pode ser reagendada.");
        }

        if (novaDataHora <= agoraUtc)
        {
            throw new ArgumentException("Consulta não pode ser marcada no passado.", nameof(novaDataHora));
        }

        var duracao = Duracao;   // guarda antes de mudar o início
        DataHora = novaDataHora;
        Fim = novaDataHora + duracao;
        LembrarEm = lembrarEm;
        LembreteEnviadoEm = null;

        // O novo horário é avaliado de novo pelo service: pode deixar de ser, ou passar a ser, encaixe.
        Encaixe = false;
        MotivoEncaixe = null;
    }

    public void MarcarComoEncaixe(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Informe o motivo do encaixe.", nameof(motivo));

        if (motivo.Trim().Length > TamanhoMaximoDoMotivoDeEncaixe)
            throw new ArgumentException($"O motivo do encaixe pode ter no máximo {TamanhoMaximoDoMotivoDeEncaixe} caracteres.", nameof(motivo));

        Encaixe = true;
        MotivoEncaixe = motivo.Trim();
    }

    public void Cancelar()
    {
        if (StatusConsultaId == StatusConsulta.RealizadaId)
        {
            throw new InvalidOperationException("Consulta realizada não pode ser cancelada.");
        }

        StatusConsultaId = StatusConsulta.CanceladaId;
        LembrarEm = null;
    }

    public void MarcarComoRealizada()
    {
        if (StatusConsultaId != StatusConsulta.AgendadaId)
        {
            throw new InvalidOperationException("Só consulta agendada pode ser marcada como realizada.");
        }

        StatusConsultaId = StatusConsulta.RealizadaId;
        LembrarEm = null;
    }
    
    public void MarcarLembreteEnviado(DateTime agoraUtc)
    {
        LembreteEnviadoEm = agoraUtc;
    }

    public void AtualizarObservacao(string? observacao)
    {
        Observacao = observacao;
    }

    
}

