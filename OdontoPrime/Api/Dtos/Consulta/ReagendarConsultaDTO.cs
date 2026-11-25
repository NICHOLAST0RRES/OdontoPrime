namespace OdontoPrime.Api.Dtos.Consulta;

public record ReagendarConsultaDTO
{
    public DateTime NovaDataHora { get; set; }   // em UTC
    public string? MotivoEncaixe { get; set; }   // preenchido só quando a recepção confirma o encaixe
}
