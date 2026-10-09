// a funcao dessa classe e desse projeto separado é  definir o formato das mensagens
// Projeto separado pra que os dois usem a mesma definição sem o worker precisar depender da API inteira.

namespace Clinica.Contratos;
                                                // Em C#, o nome do arquivo não precisa ser igual ao nome da classe. Você pode colocar quantas
                                                // classes quiser em um arquivo e dar a ele qualquer nome. Isso é diferente do Java, que exige que a
                                                // classe pública tenha o mesmo nome do arquivo.
    public record ConsultaAgendada(
        Guid ConsultaId,
        string PacienteNome,
        string PacienteTelefone,
        string ProfissionalNome,
        DateTimeOffset DataHora,
        DateTimeOffset OcorridoEm
    );

    public record ConsultaCancelada(
        Guid ConsultaId,
        string PacienteNome,
        string PacienteTelefone,
        DateTimeOffset DataHoraOriginal,
        DateTimeOffset OcorridoEm
    );

    public record ConsultaReagendada(
        Guid ConsultaId,
        string PacienteNome,
        string PacienteTelefone,
        string ProfissionalNome,
        DateTimeOffset DataHoraAnterior,
        DateTimeOffset DataHoraNova,
        DateTimeOffset OcorridoEm
    );

    public record LembreteDeConsulta(
        Guid ConsultaId,
        string PacienteNome,
        string PacienteTelefone,
        string ProfissionalNome,
        DateTimeOffset DataHora,
        DateTimeOffset OcorridoEm
    );
    
