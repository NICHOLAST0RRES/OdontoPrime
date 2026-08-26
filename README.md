# 🦷 WebApp Clínica Médica

Sistema web para gestão de uma clínica odontológica, construído em **.NET 10** com **ASP.NET Core Razor Pages** no front-end e uma **API REST** interna consumida pelas próprias páginas.

O projeto foi desenvolvido com foco em **modelagem de domínio rica**, aplicando encapsulamento real (`private set` + construtores validantes), *soft delete* automático, auditoria transparente via interceptors do EF Core e tabelas de referência no lugar de enums persistidos.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Npgsql-4169E1?style=flat&logo=postgresql&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black)

---

## 📋 Índice

- [Funcionalidades](#-funcionalidades)
- [Stack](#-stack)
- [Arquitetura](#-arquitetura)
- [Modelo de domínio](#-modelo-de-domínio)
- [Decisões técnicas](#-decisões-técnicas)
- [Como executar](#-como-executar)
- [Endpoints da API](#-endpoints-da-api)
- [Páginas da interface](#-páginas-da-interface)
- [Roadmap](#-roadmap)

---

## ✨ Funcionalidades

### Pacientes
- Cadastro com validação de nome, CPF e telefone obrigatórios
- Vínculo com convênio
- Edição de dados cadastrais
- Desativação (*soft delete*) e reativação

### Profissionais
- Cadastro de **Dentistas** e **Atendentes**
- Regra de negócio: dentista **exige** CRO; atendente **não pode** ter CRO
- Edição, desativação e reativação

### Consultas
- Agendamento vinculando paciente + profissional + data/hora
- Bloqueio de agendamento em data passada
- **Reagendamento** (permitido apenas para consultas com status `Agendada`)
- **Cancelamento** (bloqueado se a consulta já foi realizada)
- **Marcar como realizada**
- Observações por consulta

### Transversal
- Auditoria automática (`CriadoEm` / `AtualizadoEm`) sem poluir os controllers
- *Soft delete* global — registros inativos somem das consultas automaticamente
- Documentação interativa via Swagger UI

---

## 🛠 Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core Razor Pages + Controllers (API REST) |
| ORM | Entity Framework Core 10 |
| Banco | PostgreSQL (Npgsql 10) |
| Mapeamento | AutoMapper 16 |
| Documentação | Swashbuckle / Swagger UI |
| Front-end | Bootstrap 5, jQuery Validation Unobtrusive |

---

## 🏗 Arquitetura

O projeto usa uma **separação por responsabilidade dentro de um único assembly**, com fronteiras bem definidas entre domínio, persistência, API e apresentação:

```
WebApplication1/
├── Domain/                  # Núcleo do negócio (entidades ricas + contratos)
│   ├── Models/              # Paciente, Profissional, Consulta, Convenio...
│   ├── IAuditavel.cs        # Contrato de auditoria
│   └── ISoftDelete.cs       # Contrato de exclusão lógica
│
├── Data/                    # Persistência
│   ├── AppDbContext.cs      # DbContext + query filters globais
│   └── Configurations/      # Fluent API (IEntityTypeConfiguration) + seeds
│
├── Infra/
│   └── Interceptors/        # AuditoriaInterceptor, SoftDeleteInterceptor
│
├── Api/                     # Camada REST
│   ├── Controllers/         # Endpoints HTTP
│   └── Dtos/                # Contratos de entrada e saída
│
├── Mappings/                # Profiles do AutoMapper
│
├── Services/                # Clientes HTTP que as páginas usam p/ falar com a API
│
├── Pages/                   # Razor Pages (interface do usuário)
│
└── Migrations/              # Histórico de evolução do schema
```

### Fluxo de uma requisição

```
Razor Page → *ApiService (HttpClient) → Controller → DbContext → PostgreSQL
```

As Razor Pages **não acessam o `DbContext` diretamente**. Elas consomem a própria API via `HttpClient`, cujo `BaseAddress` é montado dinamicamente a partir da requisição atual — o que mantém o front desacoplado e permite extrair a API para outro processo no futuro sem reescrever a interface.

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
| **Paciente** | Entidade auditável e com soft delete. Vinculada a um convênio. |
| **Profissional** | Dentista ou atendente, com validação condicional de CRO. |
| **Consulta** | Agregado que controla o ciclo de vida do agendamento. |
| **Convenio** | Tabela de referência. |
| **TipoProfissional** | Tabela de referência (`Dentista` = 1, `Atendente` = 2). |
| **StatusConsulta** | Tabela de referência (`Agendada` = 1, `Realizada` = 2, `Cancelada` = 3). |

### Máquina de estados da consulta

```
                 ┌──────────────┐
   criação ────> │   Agendada   │
                 └──────┬───────┘
                        │
          ┌─────────────┼─────────────┐
          ∨             ∨             ∨
    ┌───────────┐  ┌──────────┐  ┌──────────┐
    │ Realizada │  │Cancelada │  │Reagendada│ (permanece Agendada)
    └───────────┘  └──────────┘  └──────────┘
          │
          └──> ❌ não pode ser cancelada
```

---

## 🔍 Decisões técnicas

**Entidades ricas com `private set`**
Nenhuma propriedade pode ser alterada de fora da entidade. Toda mudança de estado passa por um método com nome de negócio (`Reagendar`, `Cancelar`, `MarcarComoRealizada`, `Desativar`) que valida a invariante antes de aplicar.

**Tabelas de referência no lugar de enums persistidos**
`StatusConsulta` e `TipoProfissional` são tabelas reais com seed via `HasData`, e não enums em C#. Isso mantém integridade referencial no banco e permite adicionar novos valores sem recompilar a aplicação. As constantes (`StatusConsulta.AgendadaId`) preservam a legibilidade no código.

**`Guid.CreateVersion7()` para chaves primárias**
GUIDs v7 são ordenáveis por tempo, o que evita a fragmentação de índice típica dos GUIDs aleatórios em bancos relacionais.

**Soft delete global por convenção**
O `AppDbContext` varre o modelo e aplica `HasQueryFilter` automaticamente em toda entidade que implemente `ISoftDelete`. Registros inativos simplesmente deixam de existir para as consultas — sem `Where(x => x.Ativo)` espalhado pelo código.

**Interceptors para preocupações transversais**
`AuditoriaInterceptor` preenche `CriadoEm`/`AtualizadoEm` e `SoftDeleteInterceptor` converte `DELETE` em `UPDATE`, ambos no `SaveChanges`. Os controllers ficam livres desse ruído.

**`ApplyConfigurationsFromAssembly`**
As configurações Fluent API são descobertas por reflexão. Criar uma nova entidade não exige tocar no `DbContext`.

---

## 🚀 Como executar

### Pré-requisitos

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ (local ou via Docker)
- `dotnet-ef` CLI

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clonar o repositório

```bash
git clone https://github.com/NICHOLAST0RRES/WebAppClinicaMedica.git
cd WebAppClinicaMedica
```

### 2. Subir o PostgreSQL

```bash
docker run --name clinica-postgres \
  -e POSTGRES_PASSWORD=1212 \
  -e POSTGRES_DB=ProjetoClinica \
  -p 5432:5432 \
  -d postgres:16
```

> ⚠️ O arquivo `WebApplication1/local/docker-compose.yml` sobe **MongoDB e RabbitMQ**, que não são usados por esta aplicação (resquício de outro projeto). O banco efetivamente utilizado é o PostgreSQL.

### 3. Configurar a connection string

Ajuste `WebApplication1/appsettings.json` se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ProjetoClinica;Username=postgres;Password=1212"
  }
}
```

### 4. Aplicar as migrations

```bash
cd WebApplication1
dotnet ef database update
```

Isso cria o schema e popula automaticamente as tabelas `StatusConsulta` e `TipoProfissionais`.

### 5. Popular os convênios

A tabela `Convenios` não possui seed. Insira ao menos um registro antes de cadastrar pacientes:

```sql
INSERT INTO "Convenios" ("Nome") VALUES ('Particular'), ('Unimed'), ('Amil');
```

### 6. Rodar a aplicação

```bash
dotnet run
```

| Recurso | URL |
|---|---|
| Aplicação | http://localhost:5146 |
| HTTPS | https://localhost:7219 |
| Swagger UI | http://localhost:5146/swagger |

---

## 📡 Endpoints da API

### Pacientes

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Paciente` | Lista todos os pacientes ativos |
| `GET` | `/Paciente/{id}` | Busca um paciente por ID |
| `POST` | `/Paciente` | Cadastra um paciente |
| `PUT` | `/Paciente/{id}` | Atualiza os dados do paciente |
| `DELETE` | `/Paciente/{id}` | Desativa o paciente (soft delete) |
| `POST` | `/Paciente/{id}/reativar` | Reativa um paciente desativado |

### Profissionais

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Profissional` | Lista todos os profissionais ativos |
| `GET` | `/Profissional/{id}` | Busca um profissional por ID |
| `POST` | `/Profissional` | Cadastra um profissional |
| `PUT` | `/Profissional/{id}` | Atualiza os dados do profissional |
| `DELETE` | `/Profissional/{id}` | Desativa o profissional |
| `POST` | `/Profissional/{id}/reativar` | Reativa um profissional |

### Consultas

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/Consulta` | Lista todas as consultas |
| `GET` | `/Consulta/{id}` | Busca uma consulta por ID |
| `POST` | `/Consulta` | Agenda uma nova consulta |
| `PUT` | `/Consulta/{id}/reagendar` | Reagenda a consulta |
| `POST` | `/Consulta/{id}/cancelar` | Cancela a consulta |
| `POST` | `/Consulta/{id}/realizar` | Marca a consulta como realizada |

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
  "dataHora": "2026-09-15T14:30:00Z",
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

- [ ] Autenticação e autorização (JWT / Identity)
- [ ] Testes automatizados (xUnit) para as regras de domínio
- [ ] Prontuário eletrônico e registro de evolução — entidades `Prontuario`, `RegistroEvolucao` e `Remedio` já esboçadas, mas ainda não mapeadas no `DbContext`
- [ ] Validação de conflito de horário na agenda do profissional
- [ ] Paginação e filtros nas listagens
- [ ] Containerização da aplicação (Dockerfile + compose com PostgreSQL)


---

## 👤 Autor

Desenvolvido por [@NICHOLAST0RRES](https://github.com/NICHOLAST0RRES)
