# 🦷 OdontoPrime

Sistema de gestão para clínica odontológica, construído em **.NET 10** com **ASP.NET Core Razor Pages** no front-end e uma **API REST** interna consumida pelas próprias páginas.

Além do CRUD de pacientes, profissionais e consultas, o projeto publica **eventos de domínio no RabbitMQ** com entrega confiável (**Transactional Outbox**), consumidos pelo worker Java [ms-notificacao-clinica](https://github.com/NICHOLAST0RRES/ms-notificacao-clinica), que cuida das notificações e lembretes aos pacientes.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Npgsql-4169E1?style=flat&logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-7.x-FF6600?style=flat&logo=rabbitmq&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-v3-5E1F87?style=flat)
![Testcontainers](https://img.shields.io/badge/Testcontainers-Postgres%20%2B%20RabbitMQ-2496ED?style=flat&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black)

---

## 📋 Índice

- [Visão geral](#-visão-geral)
- [Funcionalidades](#-funcionalidades)
- [Stack](#-stack)
- [Arquitetura](#-arquitetura)
- [Mensageria confiável](#-mensageria-confiável)
- [Lembretes e fuso horário](#-lembretes-e-fuso-horário)
- [Modelo de domínio](#-modelo-de-domínio)
- [Decisões técnicas](#-decisões-técnicas)
- [Testes automatizados](#-testes-automatizados)
- [Como executar](#-como-executar)
- [Endpoints da API](#-endpoints-da-api)
- [Páginas da interface](#-páginas-da-interface)
- [Roadmap](#-roadmap)

---

## 🔭 Visão geral

```
┌───────────────────────── OdontoPrime (.NET 10) ─────────────────────────┐
│                                                                         │
│  Razor Pages ──HttpClient──> API REST ──> ConsultaService               │
│                                              │                          │
│                                              ▼  mesma transação         │
│                               ┌──────────────────────────────┐          │
│                               │ PostgreSQL                    │          │
│                               │  Consultas  +  OutboxMensagens│          │
│                               └──────────────┬───────────────┘          │
│                                              │                          │
│  LembreteScheduler ──(grava na outbox)───────┘                          │
│                                              │                          │
│                               RelayDaOutbox (BackgroundService)         │
└──────────────────────────────────────────────┼──────────────────────────┘
                                               │ publisher confirms
                                               ▼
                              RabbitMQ — exchange topic "clinica.eventos"
                                               │
                                               ▼
                        ms-notificacao-clinica (Java 21 / Spring Boot)
                        consumidor idempotente (Inbox) + retry + DLQ
```

O contrato das mensagens fica no projeto **`Clinica.Contratos`**, uma class library separada, para que produtor e consumidor compartilhem o mesmo formato sem que o worker dependa da API inteira.

---

## ✨ Funcionalidades

### Pacientes
- Cadastro com nome, **CPF validado** (dígitos verificadores) e telefone obrigatórios
- Vínculo com convênio
- Edição, desativação (*soft delete*) e reativação

### Profissionais
- Cadastro de **Dentistas** e **Atendentes**
- Regra de negócio: dentista **exige** CRO; atendente **não pode** ter CRO
- Edição, desativação e reativação

### Consultas
- Agendamento vinculando paciente + profissional + data/hora
- Bloqueio de agendamento em data passada
- **Reagendamento** (apenas consultas `Agendada`), que recalcula o lembrete
- **Cancelamento** (bloqueado se a consulta já foi realizada)
- **Marcar como realizada**
- Observações por consulta

### Eventos e notificações
- Cada agendamento, reagendamento e cancelamento gera um **evento de domínio** publicado no RabbitMQ
- **Lembrete automático** na véspera da consulta, às 18h no horário da clínica
- Entrega **pelo menos uma vez**, mesmo com o RabbitMQ fora do ar no momento da operação

### Transversal
- Auditoria automática (`CriadoEm` / `AtualizadoEm`) via interceptor
- *Soft delete* global — registros inativos somem das consultas automaticamente
- Documentação interativa via Swagger UI
- Suíte de testes de unidade e de integração com infraestrutura real em containers

---

## 🛠 Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core Razor Pages + Controllers (API REST) |
| ORM | Entity Framework Core 10 |
| Banco | PostgreSQL (Npgsql 10) |
| Mensageria | RabbitMQ (`RabbitMQ.Client` 7) — exchange *topic* |
| Jobs | `BackgroundService` (relay da outbox, manutenção, lembretes) |
| Tempo | `TimeProvider` + `RelogioDaClinica` |
| Mapeamento | AutoMapper 16 |
| Documentação | Swashbuckle / Swagger UI |
| Testes | xUnit v3, Testcontainers (PostgreSQL e RabbitMQ), `WebApplicationFactory` |
| Front-end | Bootstrap 5, jQuery Validation Unobtrusive |

---

## 🏗 Arquitetura

A solução tem três projetos:

```
OdontoPrime.sln
├── Clinica.Contratos/           # Records dos eventos (contrato compartilhado com o worker Java)
├── OdontoPrime/                 # Aplicação web + API
└── OdontoPrime.Tests/           # Testes de unidade e de integração
```

Dentro de `OdontoPrime/`, a separação é por responsabilidade:

```
OdontoPrime/
├── Domain/                      # Entidades ricas, value object Cpf, contratos IAuditavel/ISoftDelete
├── Application/                 # ConsultaService, PoliticaDeLembrete, RelogioDaClinica, IOutbox, Result
├── Data/                        # AppDbContext + Configurations (Fluent API, seeds)
├── Infra/
│   ├── Interceptors/            # AuditoriaInterceptor, SoftDeleteInterceptor
│   ├── Jobs/                    # LembreteScheduler
│   └── Mensageria/
│       ├── Outbox/              # OutboxMensagem, OutboxEfCore, RelayDaOutbox, ManutencaoDaOutbox
│       ├── PublicadorRabbitMq.cs
│       └── RoutingKeys.cs
├── Api/                         # Controllers + DTOs
├── APIsClient/                  # Clientes HTTP que as Razor Pages usam para falar com a API
├── Mappings/                    # Profiles do AutoMapper
├── Validation/                  # Atributo [Cpf] para os formulários
├── Pages/                       # Razor Pages (interface da recepção)
├── Migrations/                  # Histórico do schema
└── local/docker-compose.yml     # RabbitMQ para desenvolvimento
```

### Fluxo de uma requisição

```
Razor Page → *ApiClient (HttpClient) → Controller → ConsultaService → DbContext → PostgreSQL
```

As Razor Pages **não acessam o `DbContext` diretamente**. Elas consomem a própria API via `HttpClient`, cujo `BaseAddress` é montado a partir da requisição atual — o front fica desacoplado e a API pode ser extraída para outro processo sem reescrever a interface.

---

## 📨 Mensageria confiável

O problema clássico: salvar a consulta no banco e publicar o evento no RabbitMQ são duas operações em sistemas diferentes. Se o broker cair entre uma e outra, o evento se perde (ou é publicado para uma consulta que nunca foi salva).

### Transactional Outbox (lado .NET)

1. O `ConsultaService` grava a consulta **e** o evento (tabela `OutboxMensagens`) na **mesma transação**. Ou os dois existem, ou nenhum.
2. O `RelayDaOutbox` (`BackgroundService`, ciclo de 2s) lê lotes de mensagens pendentes com **`FOR UPDATE SKIP LOCKED`**, o que permite mais de uma instância rodando sem publicar a mesma mensagem duas vezes ao mesmo tempo.
3. A publicação usa **publisher confirms** e **`mandatory: true`**: só marca `PublicadoEm` quando o broker confirma, e uma mensagem sem fila de destino volta como erro em vez de sumir.
4. Na primeira falha o lote **para**, preservando a ordem dos eventos; a mensagem registra `Tentativas` e `UltimoErro` e é retentada no próximo ciclo.
5. Se o RabbitMQ estiver fora, o relay só tenta reconectar no ciclo seguinte — a aplicação continua aceitando agendamentos normalmente.
6. O `ManutencaoDaOutbox` (a cada 5 min) apaga mensagens publicadas há mais de **7 dias** e loga um aviso se a pendente mais antiga estiver esperando há mais de 5 minutos.

### Eventos publicados

Exchange **`clinica.eventos`** (tipo *topic*, durável). Cada mensagem leva `MessageId` e `Type` nas propriedades AMQP.

| Evento (`Clinica.Contratos`) | Routing key |
|---|---|
| `ConsultaAgendada` | `consulta.agendada` |
| `ConsultaReagendada` | `consulta.reagendada` |
| `ConsultaCancelada` | `consulta.cancelada` |
| `LembreteDeConsulta` | `lembrete.gerado` |

Todos os eventos usam `DateTimeOffset` (com o offset da clínica) e carregam o campo **`OcorridoEm`**, usado pelo consumidor para descartar eventos que chegam fora de ordem.

### Consumidor idempotente (lado Java)

Como a outbox garante entrega *pelo menos uma vez*, duplicatas são esperadas. O [ms-notificacao-clinica](https://github.com/NICHOLAST0RRES/ms-notificacao-clinica) implementa o padrão **Inbox**: registra cada `MessageId` processado (`mensagens_processadas`) e o último evento aplicado por consulta (`ultimo_evento_por_consulta`), ignorando duplicatas e eventos mais antigos que o já processado. Falhas passam por retry e terminam em **DLX/DLQ**.

---

## ⏰ Lembretes e fuso horário

**Fuso explícito.** Toda conversão de horário passa pelo `RelogioDaClinica`, que encapsula um `TimeProvider` e o fuso configurado (`Clinica:FusoHorario`, padrão `America/Bahia`). No banco tudo é UTC; na tela e nos contratos, o horário da clínica. Isso corrigiu um bug real: uma consulta às 22h em Salvador já é "amanhã" em UTC, e o lembrete saía no dia errado.

**`LembrarEm` calculado no agendamento.** A `PoliticaDeLembrete` define o instante do lembrete (véspera, `Clinica:HoraDoLembrete` = 18h) no momento em que a consulta é agendada ou reagendada, e grava em `LembrarEm`. O `LembreteScheduler` só precisa buscar consultas `Agendada` com `LembrarEm <= agora` e `LembreteEnviadoEm` nulo — sem recalcular janelas de "amanhã" a cada execução. Reagendar recalcula o campo e libera um novo envio; cancelar impede o lembrete.

**Tempo controlável nos testes.** Como nada chama `DateTime.Now` diretamente, os testes trocam o `TimeProvider` por um relógio de teste e simulam "véspera às 18h" sem esperar o relógio real.

---

## 🧩 Modelo de domínio

```
Convenio (1) ──────< (N) Paciente
                            │
                            │ (N)
                            ∨
                        Consulta ──────> StatusConsulta
                            ∧
                            │ (N)
                            │
TipoProfissional (1) ──< (N) Profissional
```

| Entidade | Papel |
|---|---|
| **Paciente** | Auditável e com soft delete. CPF validado pelo value object `Cpf`. |
| **Profissional** | Dentista ou atendente, com validação condicional de CRO. |
| **Consulta** | Agregado que controla o ciclo de vida do agendamento e do lembrete (`LembrarEm`, `LembreteEnviadoEm`). |
| **OutboxMensagem** | Evento pendente de publicação (`Tipo`, `RoutingKey`, `Payload`, `Tentativas`, `UltimoErro`, `PublicadoEm`). |
| **Convenio** | Tabela de referência. |
| **TipoProfissional** | Tabela de referência (`Dentista` = 1, `Atendente` = 2). |
| **StatusConsulta** | Tabela de referência (`Agendada` = 1, `Realizada` = 2, `Cancelada` = 3). |

### Máquina de estados da consulta

```
                 ┌──────────────┐
   criação ────> │   Agendada   │ ──> evento ConsultaAgendada
                 └──────┬───────┘
                        │
          ┌─────────────┼─────────────┐
          ∨             ∨             ∨
    ┌───────────┐  ┌──────────┐  ┌──────────┐
    │ Realizada │  │Cancelada │  │Reagendada│ (permanece Agendada,
    └───────────┘  └──────────┘  └──────────┘  recalcula LembrarEm)
          │             │             │
          │             └─> evento     └─> evento ConsultaReagendada
          │                 ConsultaCancelada
          └──> ❌ não pode ser cancelada
```

---

## 🔍 Decisões técnicas

**Entidades ricas com `private set`**
Toda mudança de estado passa por um método com nome de negócio (`Reagendar`, `Cancelar`, `MarcarComoRealizada`, `Desativar`) que valida a invariante antes de aplicar.

**Outbox em vez de publicar direto no controller**
Publicar no RabbitMQ dentro da requisição acopla a disponibilidade da API à do broker e abre espaço para eventos perdidos ou "fantasmas". A outbox troca isso por consistência no banco + entrega assíncrona com retry.

**`FOR UPDATE SKIP LOCKED` em SQL cru**
O relay usa `FromSql` porque o EF Core não expressa esse lock em LINQ. É o que permite escalar o relay horizontalmente sem coordenação extra.

**Contratos em projeto separado**
`Clinica.Contratos` define o formato das mensagens. Renomear um record muda o `Tipo` da mensagem — é tratado como mudança de contrato, igual a mudar um campo.

**`TimeProvider` em vez de `DateTime.Now`**
Testabilidade e correção de fuso no mesmo movimento.

**Tabelas de referência no lugar de enums persistidos**
`StatusConsulta` e `TipoProfissional` são tabelas reais com seed via `HasData`, mantendo integridade referencial no banco. As constantes (`StatusConsulta.AgendadaId`) preservam a legibilidade no código.

**`Guid.CreateVersion7()` para chaves primárias**
GUIDs v7 são ordenáveis por tempo e evitam a fragmentação de índice típica dos GUIDs aleatórios.

**Soft delete global por convenção**
O `AppDbContext` aplica `HasQueryFilter` automaticamente em toda entidade que implementa `ISoftDelete`.

**Interceptors para preocupações transversais**
`AuditoriaInterceptor` preenche `CriadoEm`/`AtualizadoEm` e `SoftDeleteInterceptor` converte `DELETE` em `UPDATE`, ambos no `SaveChanges`.

---

## 🧪 Testes automatizados

O projeto `OdontoPrime.Tests` usa **xUnit v3** rodando na Microsoft.Testing.Platform.

- **Unidade** — regras puras, sem infraestrutura (ex.: `PoliticaDeLembrete`).
- **Integração** — sobem **PostgreSQL e RabbitMQ reais** com **Testcontainers** e a aplicação inteira com **`WebApplicationFactory`**, com relógio de teste controlado. Nada de banco em memória: o `FOR UPDATE SKIP LOCKED`, as migrations e os publisher confirms são testados de verdade.

| Suíte | O que garante |
|---|---|
| `PoliticaDeLembreteTests` | Lembrete na véspera às 18h; consulta tarde da noite usa o dia de Salvador, não o do UTC; sem lembrete quando a véspera já passou |
| `OutboxTests` | Agendar grava consulta e evento juntos; relay publica e marca como publicada; relay para o lote na primeira falha para manter a ordem |
| `LembreteTests` | Lembrete sai uma única vez; reagendar recalcula e libera novo envio; consulta cancelada não recebe lembrete |
| `ManutencaoDaOutboxTests` | Apaga só as mensagens publicadas há mais de 7 dias |

```bash
# na raiz do repositório (onde está o OdontoPrime.sln), com o Docker rodando
dotnet test
```

---

## 🚀 Como executar

### Pré-requisitos

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- Docker (PostgreSQL, RabbitMQ e testes de integração)
- `dotnet-ef` CLI

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clonar o repositório

```bash
git clone https://github.com/NICHOLAST0RRES/OdontoPrime.git
cd OdontoPrime
```

### 2. Subir o PostgreSQL

```bash
docker run --name clinica-postgres \
  -e POSTGRES_PASSWORD=1212 \
  -e POSTGRES_DB=ProjetoClinica \
  -p 5432:5432 \
  -d postgres:16
```

### 3. Subir o RabbitMQ

```bash
docker compose -f OdontoPrime/local/docker-compose.yml up -d
```

Painel de gerenciamento: http://localhost:15672 (usuário `admin`, senha `admin`).

> Sem o RabbitMQ a aplicação continua funcionando: os eventos ficam pendentes na outbox e são publicados quando o broker voltar.

### 4. Conferir a configuração

`OdontoPrime/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ProjetoClinica;Username=postgres;Password=1212"
  },
  "RabbitMq": {
    "ConnectionString": "amqp://admin:admin@localhost:5672"
  },
  "Clinica": {
    "FusoHorario": "America/Bahia",
    "HoraDoLembrete": 18
  }
}
```

### 5. Aplicar as migrations

```bash
cd OdontoPrime
dotnet ef database update
```

Isso cria o schema (incluindo a `OutboxMensagens`) e popula `StatusConsulta` e `TipoProfissionais`.

### 6. Popular os convênios

A tabela `Convenios` não possui seed. Insira ao menos um registro antes de cadastrar pacientes:

```sql
INSERT INTO "Convenios" ("Nome") VALUES ('Particular'), ('Unimed'), ('Amil');
```

### 7. Rodar a aplicação

```bash
dotnet run
```

| Recurso | URL |
|---|---|
| Aplicação | http://localhost:5146 |
| HTTPS | https://localhost:7219 |
| Swagger UI | http://localhost:5146/swagger |

Para ver os eventos sendo consumidos, rode também o [ms-notificacao-clinica](https://github.com/NICHOLAST0RRES/ms-notificacao-clinica).

---

## 📡 Endpoints da API

### Pacientes

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Paciente` | Lista os pacientes ativos |
| `GET` | `/Paciente/{id}` | Busca um paciente por ID |
| `POST` | `/Paciente` | Cadastra um paciente |
| `PUT` | `/Paciente/{id}` | Atualiza os dados do paciente |
| `DELETE` | `/Paciente/{id}` | Desativa o paciente (soft delete) |
| `POST` | `/Paciente/{id}/reativar` | Reativa um paciente desativado |

### Profissionais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Profissional` | Lista os profissionais ativos |
| `GET` | `/Profissional/{id}` | Busca um profissional por ID |
| `POST` | `/Profissional` | Cadastra um profissional |
| `PUT` | `/Profissional/{id}` | Atualiza os dados do profissional |
| `DELETE` | `/Profissional/{id}` | Desativa o profissional |
| `POST` | `/Profissional/{id}/reativar` | Reativa um profissional |

### Consultas

| Método | Rota | Descrição | Evento |
|---|---|---|---|
| `GET` | `/Consulta` | Lista as consultas | — |
| `GET` | `/Consulta/{id}` | Busca uma consulta por ID | — |
| `POST` | `/Consulta` | Agenda uma nova consulta | `consulta.agendada` |
| `PUT` | `/Consulta/{id}/reagendar` | Reagenda a consulta | `consulta.reagendada` |
| `POST` | `/Consulta/{id}/cancelar` | Cancela a consulta | `consulta.cancelada` |
| `POST` | `/Consulta/{id}/realizar` | Marca a consulta como realizada | — |

### Tabelas de apoio

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Convenio` | Lista os convênios disponíveis |
| `GET` | `/TipoProfissional` | Lista os tipos de profissional |

### Exemplo de requisição

```http
POST /Consulta
Content-Type: application/json

{
  "pacienteId": "0198f2c1-3a4b-7c5d-8e9f-a0b1c2d3e4f5",
  "profissionalId": "0198f2c1-9d8e-7f6a-5b4c-3d2e1f0a9b8c",
  "dataHora": "2026-11-15T14:30:00-03:00",
  "observacao": "Limpeza e avaliação de rotina"
}
```

---

## 🖥 Páginas da interface

| Rota | Descrição |
|---|---|
| `/` | Página inicial |
| `/Pacientes` | Listagem de pacientes |
| `/Pacientes/Create` | Cadastro de paciente |
| `/Pacientes/Edit/{id}` | Edição de paciente |
| `/Profissionais` | Listagem de profissionais |
| `/Profissionais/Create` | Cadastro de profissional |
| `/Profissionais/Edit/{id}` | Edição de profissional |
| `/Consultas` | Listagem de consultas |
| `/Consultas/Create` | Agendamento de consulta |
| `/Consultas/Reagendar/{id}` | Reagendamento de consulta |

---

## 🗺 Roadmap

O projeto segue um plano de evolução contínua. Concluído até aqui:

- [x] Domínio rico, soft delete, auditoria e validação de CPF
- [x] Eventos de domínio no RabbitMQ com contrato compartilhado (`Clinica.Contratos`)
- [x] **Transactional Outbox** com relay (`SKIP LOCKED`, publisher confirms, `mandatory`) e manutenção
- [x] Consumidor idempotente com Inbox, retry e DLQ no worker Java
- [x] Correção de fuso com `TimeProvider`/`RelogioDaClinica` e contratos em `DateTimeOffset` + `OcorridoEm`
- [x] `LembrarEm` calculado no agendamento
- [x] Testes de unidade e de integração com Testcontainers

Próximos passos:

- [ ] Testes no worker Java (idempotência, ordem dos eventos e contrato)
- [ ] CI com GitHub Actions nos dois repositórios
- [ ] Teste de caos: derrubar o RabbitMQ durante agendamentos e provar que nenhum evento se perde
- [ ] Jornada de trabalho por dentista e validação de conflito de horário na agenda, com **encaixe** manual registrado pela recepção
- [ ] Revisão da regra de lembrete para quem agenda depois das 18h da véspera
- [ ] **Agente de agendamento com IA**: a API expõe um servidor **MCP**; agente em Java com **Spring AI** + **Amazon Bedrock**, atendendo pacientes pelo **Telegram** e a recepção via **Claude Desktop**
- [ ] Previsão de faltas (ML), prontuário por voz e copiloto com RAG
- [ ] Autenticação e autorização
- [ ] Prontuário eletrônico — entidades `Prontuario`, `RegistroEvolucao` e `Remedio` já esboçadas, ainda não mapeadas no `DbContext`
- [ ] Containerização da aplicação e deploy na AWS (EC2 sob demanda via Terraform)

---

## 👤 Autor

Desenvolvido por [@NICHOLAST0RRES](https://github.com/NICHOLAST0RRES)
