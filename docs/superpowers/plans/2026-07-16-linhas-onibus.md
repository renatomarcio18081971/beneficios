# Linhas de Ônibus e Funcionário × Linhas — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar CRUD tenant de linhas de ônibus e vínculos funcionário×linha (N:N), com gate de Vale Transporte, atalho em Funcionários, dois módulos de permissão e sem DELETE físico.

**Architecture:** Tabelas `linhas_onibus` e `funcionario_linhas` por schema tenant. Domain reutiliza `AfastamentoPeriodo.EstaAtivoEm` para vigência. Repositories Dapper + Services com validação (VT, unicidade, bloqueio ao encerrar linha). APIs `api/linhas-onibus` e `api/funcionario-linhas` (GET/POST/PUT). Front list/form no layout existente; atalho `?funcionarioId=` em Funcionários. Ao desativar VT no save do funcionário, encerrar vínculos abertos.

**Tech Stack:** .NET 9, Dapper, PostgreSQL schemas, xUnit/Moq; Angular 22 standalone + Material + reactive forms; UTF-8 com BOM; Context7 + skill `angular-developer` no front.

**Spec:** `docs/superpowers/specs/2026-07-16-linhas-onibus-design.md`

## Global Constraints

- Encoding: **UTF-8 com BOM** em todo arquivo criado ou alterado
- Escopo: **somente tenant**; admin fora da v1
- Métodos/tipos de domínio novos em **português** (`SalvarAsync`, `ObterPorIdAsync`, `FiltrarAsync`, etc.)
- Sem endpoint DELETE e sem botão Excluir; encerrar = `data_fim`
- Vínculo só com VT ativo; desativar VT → `data_fim = hoje` nos vínculos abertos
- Encerrar linha bloqueado se houver vínculo com `data_fim` NULL
- UNIQUE `(funcionario_id, linha_onibus_id)`; quantidade inteiro ≥ 1
- Layout UI: **reutilizar** padrão das telas já existentes
- Cobertura unitária **≥ 80%** (`scripts/check-coverage.ps1` / Sonar)
- Backend: `*Params`, `*Dto`, AutoMapper `*Profile`, controllers enxutos
- Seguir padrões de Afastamentos / Funcionários / Usuários
- Motor de VT: **fora desta entrega**

---

## File Map

### Backend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs` | Insert linha |
| `src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs` | Update linha |
| `src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs` | Leitura linha |
| `src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs` | Filtro listagem |
| `src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs` | Contrato linha |
| `src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs` | Insert vínculo |
| `src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs` | Update vínculo |
| `src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs` | Leitura vínculo (+ nomes) |
| `src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs` | Filtro vínculos |
| `src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs` | Contrato vínculo |
| `src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs` | DTOs linha |
| `src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs` | DTOs vínculo |
| `src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs` | Serviço linha |
| `src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs` | Serviço vínculo |
| `src/Beneficios.Application/Services/LinhaOnibusService.cs` | Validação + bloqueio encerrar |
| `src/Beneficios.Application/Services/FuncionarioLinhaService.cs` | VT, unicidade, vigência |
| `src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs` | AutoMapper |
| `src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs` | AutoMapper |
| `src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs` | Dapper |
| `src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs` | Dapper |
| `src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql` | DDL referência |
| `src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql` | DDL referência |
| `src/Beneficios.Api/Controllers/LinhasOnibusController.cs` | `api/linhas-onibus` |
| `src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs` | `api/funcionario-linhas` |
| Testes Domain/Application/Api correspondentes | Cobertura ≥ 80% |

### Backend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `ModulosSistemaCatalog.cs` | + `linhas_onibus`, `funcionario_linhas` |
| `TenantSchemaSql.cs` | `CriarTabelaLinhasOnibus`, `CriarTabelaFuncionarioLinhas` |
| `TenantProvisioner.cs` | DDL em Provisionar/Garantir |
| `FuncionarioService.cs` | Ao VT inativo, chamar encerrar vínculos |
| `IFuncionarioLinhaRepository` (método) | `EncerrarAbertosPorFuncionarioAsync` |
| `Program.cs` | DI |
| `PostgresFixture.cs` (se existir DDL de teste) | Incluir novas tabelas |
| `ModulosSistemaCatalogTests.cs` | Assert novos módulos |

### Frontend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/api/linha-onibus.models.ts` / `linha-onibus.service.ts` | HTTP linhas |
| `core/api/funcionario-linha.models.ts` / `funcionario-linha.service.ts` | HTTP vínculos |
| `features/linhas-onibus/linha-onibus-list/*` | Listagem |
| `features/linhas-onibus/linha-onibus-form/*` | Form |
| `features/funcionario-linhas/funcionario-linha-list/*` | Listagem + `funcionarioId` |
| `features/funcionario-linhas/funcionario-linha-form/*` | Form |

### Frontend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/auth/modulos-sistema.ts` | + 2 módulos |
| `app.routes.ts` | Rotas + guards |
| `funcionario-list` / `funcionario-form` | Atalho Linhas |
| `e2e/fixtures/api-mocks.ts` + `e2e/tenant-flows.spec.ts` | Smoke se aplicável |

---

## Task breakdown

### Task 1: Módulos de permissão (back + front)

**Files:**
- Modify: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Modify: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Modify: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: códigos `linhas_onibus` (rota `/linhas-onibus`, ícone `directions_bus`) e `funcionario_linhas` (rota `/funcionario-linhas`, ícone `commute`), ambos com `AcaoPermissao.Todas`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Todos_DeveConterLinhasOnibus()
{
    Assert.Contains(ModulosSistemaCatalog.Todos,
        m => m.Codigo == "linhas_onibus" && m.Rota == "/linhas-onibus");
}

[Fact]
public void Todos_DeveConterFuncionarioLinhas()
{
    Assert.Contains(ModulosSistemaCatalog.Todos,
        m => m.Codigo == "funcionario_linhas" && m.Rota == "/funcionario-linhas");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q`  
Expected: FAIL (códigos ausentes)

- [ ] **Step 3: Add modules**

Em `ModulosSistemaCatalog.Todos`, após `afastamentos`:

```csharp
new("linhas_onibus", "Linhas de Ônibus", "/linhas-onibus", AcaoPermissao.Todas),
new("funcionario_linhas", "Funcionário × Linhas", "/funcionario-linhas", AcaoPermissao.Todas),
```

Em `MODULOS_SISTEMA` (front), entradas equivalentes com `icone: 'directions_bus'` e `icone: 'commute'`.

- [ ] **Step 4: Run tests — expect PASS**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q`

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/ModulosSistemaCatalog.cs Beneficios.Front/src/app/core/auth/modulos-sistema.ts tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
git commit -m "feat: add linhas_onibus and funcionario_linhas permission modules"
```

---

### Task 2: Domain models e interfaces — Linha de ônibus

**Files:**
- Create: `src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs`
- Create: `src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs`
- Test: `tests/Beneficios.Tests/Domain/LinhaOnibusModelsCompileTests.cs` (ou smoke via build)

**Interfaces:**
- Produces:

```csharp
public class LinhaOnibusSalvarParams
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public decimal ValorTarifa { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class LinhaOnibusAtualizarParams
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public decimal ValorTarifa { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class LinhaOnibusQueryResult
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal ValorTarifa { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}

public class LinhaOnibusFiltroParams
{
    public string? Descricao { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; } // default hoje no service se SomenteVigentes
}

public interface ILinhaOnibusRepository
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros);
    Task AtualizarAsync(LinhaOnibusAtualizarParams parametros);
    Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro);
}
```

- [ ] **Step 1: Create the files above (UTF-8 BOM)**

- [ ] **Step 2: Build Domain**

Run: `dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q`  
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add src/Beneficios.Domain/Models/LinhaOnibus*.cs src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs
git commit -m "feat(domain): add linha onibus persistence contracts"
```

---

### Task 3: Domain models e interfaces — Funcionário × Linha

**Files:**
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs`
- Create: `src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs`

**Interfaces:**
- Produces:

```csharp
public class FuncionarioLinhaSalvarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class FuncionarioLinhaAtualizarParams
{
    public Guid Id { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class FuncionarioLinhaQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public Guid LinhaOnibusId { get; set; }
    public string LinhaDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}

public class FuncionarioLinhaFiltroParams
{
    public Guid? FuncionarioId { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; }
}

public interface IFuncionarioLinhaRepository
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros);
    Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros);
    Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro);
    Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null);
    Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId);
    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId);
}
```

- [ ] **Step 1: Create files (UTF-8 BOM)**

- [ ] **Step 2: Build Domain — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add src/Beneficios.Domain/Models/FuncionarioLinha*.cs src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs
git commit -m "feat(domain): add funcionario-linha persistence contracts"
```

---

### Task 4: DDL e provisionamento tenant

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql`
- Create: `src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql`
- Modify: fixture de integração se aplicar DDL tenant (ex.: `PostgresFixture.cs`)
- Test: `tests/Beneficios.Tests/Infrastructure/DatabaseConfigurationTests.cs` ou `TenantProvisionerTests.cs` — assert métodos SQL não vazios / provisionamento inclui tabelas se o padrão do repo for esse

**Interfaces:**
- Produces: `TenantSchemaSql.CriarTabelaLinhasOnibus(string schemaName)` e `CriarTabelaFuncionarioLinhas(string schemaName)`
- Consumes: padrão de `CriarTabelaFuncionarioAfastamentos`

- [ ] **Step 1: Add SQL builders** (espelhar afastamentos; UNIQUE e FKs conforme spec)

```csharp
public static string CriarTabelaLinhasOnibus(string schemaName)
{
    var quotedSchema = CitarIdentificador(schemaName);
    var indexPrefix = schemaName.Replace('-', '_');
    return $"""
        CREATE TABLE IF NOT EXISTS {quotedSchema}.linhas_onibus (
            id UUID PRIMARY KEY,
            descricao VARCHAR NOT NULL,
            data_inicio DATE NOT NULL,
            data_fim DATE NULL,
            valor_tarifa NUMERIC(18,2) NOT NULL,
            data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
            data_alteracao TIMESTAMP NULL,
            usuario_alteracao_id UUID NULL
        );
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_descricao
            ON {quotedSchema}.linhas_onibus(descricao);
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_datas
            ON {quotedSchema}.linhas_onibus(data_inicio, data_fim);
        """;
}

public static string CriarTabelaFuncionarioLinhas(string schemaName)
{
    var quotedSchema = CitarIdentificador(schemaName);
    var indexPrefix = schemaName.Replace('-', '_');
    return $"""
        CREATE TABLE IF NOT EXISTS {quotedSchema}.funcionario_linhas (
            id UUID PRIMARY KEY,
            funcionario_id UUID NOT NULL,
            linha_onibus_id UUID NOT NULL,
            quantidade INT NOT NULL,
            data_inicio DATE NOT NULL,
            data_fim DATE NULL,
            data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
            data_alteracao TIMESTAMP NULL,
            usuario_alteracao_id UUID NULL,
            CONSTRAINT fk_{indexPrefix}_func_linha_funcionario
                FOREIGN KEY (funcionario_id) REFERENCES {quotedSchema}.funcionarios(id),
            CONSTRAINT fk_{indexPrefix}_func_linha_linha
                FOREIGN KEY (linha_onibus_id) REFERENCES {quotedSchema}.linhas_onibus(id),
            CONSTRAINT uq_{indexPrefix}_func_linha UNIQUE (funcionario_id, linha_onibus_id)
        );
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_funcionario
            ON {quotedSchema}.funcionario_linhas(funcionario_id);
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_linha
            ON {quotedSchema}.funcionario_linhas(linha_onibus_id);
        """;
}
```

Scripts `14_` / `15_`: comentário de referência apontando para esses métodos (padrão scripts 11–13).

- [ ] **Step 2: Call both in `ProvisionarAsync` and `GarantirPerfisNoSchemaAsync`** (após afastamentos; **linhas antes** de funcionario_linhas por FK)

- [ ] **Step 3: Build Infrastructure — expect PASS**

- [ ] **Step 4: Commit**

```bash
git add src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql
git commit -m "feat(infra): add linhas_onibus and funcionario_linhas DDL provisioning"
```

---

### Task 5: Repository + Service + API — Linhas de ônibus

**Files:**
- Create: `LinhaOnibusRepository.cs`, `LinhaOnibusService.cs`, `ILinhaOnibusService.cs`, `LinhaOnibusDtos.cs`, `LinhaOnibusProfile.cs`, `LinhasOnibusController.cs`
- Modify: `Program.cs` (DI)
- Test: `tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs`
- Test: `tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs` (padrão Afastamentos)

**Interfaces:**
- Consumes: `ILinhaOnibusRepository`, `IFuncionarioLinhaRepository.ExisteVinculoAbertoPorLinhaAsync`
- Produces:

```csharp
public record LinhaOnibusDto(Guid Id, string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusSalvarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusAtualizarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);

public interface ILinhaOnibusService
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(string? descricao, bool? somenteVigentes);
}
```

Mensagens constantes em `LinhaOnibusService`:

```csharp
public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
public const string MensagemNaoEncontrada = "Linha não encontrada.";
public const string MensagemVinculosAbertos = "Existem vínculos em aberto para esta linha; encerre-os antes de finalizar a vigência.";
public const string MensagemTarifa = "O valor da tarifa deve ser maior ou igual a zero.";
```

Regras em `AtualizarAsync`: se `dto.DataFim` não nulo e `ExisteVinculoAbertoPorLinhaAsync(id)` → throw `MensagemVinculosAbertos`.  
Validar datas com mesma lógica de afastamentos; tarifa ≥ 0.  
Vigência filtro: `AfastamentoPeriodo.EstaAtivoEm(dataInicio, dataFim, referencia)` (referencia = hoje).

Controller: `[Route("api/linhas-onibus")]`, `CodigoMenu = "linhas_onibus"`, espelhar `AfastamentosController` **sem** DELETE.

- [ ] **Step 1: Write failing service tests** (data fim inválida; tarifa negativa; encerrar com vínculo aberto; not found)

- [ ] **Step 2: Run — expect FAIL**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~LinhaOnibusService" -v q`

- [ ] **Step 3: Implement repository (Dapper, search_path tenant — copiar padrão `FuncionarioAfastamentoRepository`), service, profile, controller, DI**

Para Task 5 o método `ExisteVinculoAbertoPorLinhaAsync` pode retornar `false` temporariamente via stub/mock nos testes do service; implementação real do repo de vínculo na Task 6. Alternativa: implementar stub no `FuncionarioLinhaRepository` mínimo só com esse método na Task 5 — preferir **mock** de `IFuncionarioLinhaRepository` nos testes de `LinhaOnibusService`.

- [ ] **Step 4: Run service + controller tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs src/Beneficios.Application/Services/LinhaOnibusService.cs src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs src/Beneficios.Api/Controllers/LinhasOnibusController.cs src/Beneficios.Api/Program.cs tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs
git commit -m "feat: add linhas-onibus service, repository, and API"
```

---

### Task 6: Repository + Service + API — Funcionário × Linhas

**Files:**
- Create: `FuncionarioLinhaRepository.cs`, `FuncionarioLinhaService.cs`, `IFuncionarioLinhaService.cs`, `FuncionarioLinhaDtos.cs`, `FuncionarioLinhaProfile.cs`, `FuncionarioLinhasController.cs`
- Modify: `Program.cs`
- Test: `FuncionarioLinhaServiceTests.cs`, `FuncionarioLinhasControllerTests.cs`

**Interfaces:**
- Consumes: `IFuncionarioLinhaRepository`, `ILinhaOnibusRepository`, `IFuncionarioRepository.ObterPorIdAsync` + `ObterBeneficiosAsync`
- Produces:

```csharp
public record FuncionarioLinhaDto(
    Guid Id, Guid FuncionarioId, string FuncionarioNome,
    Guid LinhaOnibusId, string LinhaDescricao,
    int Quantidade, DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaSalvarDto(
    Guid FuncionarioId, Guid LinhaOnibusId, int Quantidade,
    DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaAtualizarDto(
    Guid LinhaOnibusId, int Quantidade, DateOnly DataInicio, DateOnly? DataFim);

public interface IFuncionarioLinhaService
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes);
}
```

Mensagens:

```csharp
public const string MensagemVtInativo = "Funcionário sem vale transporte ativo; não é possível vincular linhas.";
public const string MensagemLinhaNaoVigente = "A linha selecionada não está vigente na data de início do vínculo.";
public const string MensagemParDuplicado = "Esta linha já está vinculada a este funcionário.";
public const string MensagemQuantidade = "A quantidade de utilizações deve ser maior ou igual a 1.";
public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
public const string MensagemVinculoNaoEncontrado = "Vínculo não encontrado.";
public const string MensagemFuncionarioNaoEncontrado = "Funcionário não encontrado.";
public const string MensagemLinhaNaoEncontrada = "Linha não encontrada.";
public const string CodigoValeTransporte = "vale_transporte";
```

Fluxo `SalvarAsync` / `AtualizarAsync`:
1. Validar datas e quantidade ≥ 1  
2. Funcionário existe  
3. Benefícios: algum com `CodigoBeneficio == vale_transporte` e `Ativo == true` (senão `MensagemVtInativo`)  
4. Linha existe; `AfastamentoPeriodo.EstaAtivoEm(linha.DataInicio, linha.DataFim, dto.DataInicio)`  
5. `ExisteParAsync` → `MensagemParDuplicado`  
6. Persist  

SQL `EncerrarAbertosPorFuncionarioAsync`:

```sql
UPDATE funcionario_linhas
SET data_fim = @DataFim, data_alteracao = @DataAlteracao, usuario_alteracao_id = @UsuarioAlteracaoId
WHERE funcionario_id = @FuncionarioId AND data_fim IS NULL
```

Controller: `[Route("api/funcionario-linhas")]`, `CodigoMenu = "funcionario_linhas"`, sem DELETE.

- [ ] **Step 1: Write failing tests** (VT off, linha vencida, duplicado, quantidade 0, happy path)

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement repository + service + API + DI**

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs src/Beneficios.Application/Services/FuncionarioLinhaService.cs src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs src/Beneficios.Api/Program.cs tests/Beneficios.Tests/Application/FuncionarioLinhaServiceTests.cs tests/Beneficios.Tests/Api/FuncionarioLinhasControllerTests.cs
git commit -m "feat: add funcionario-linhas service, repository, and API"
```

---

### Task 7: Desativar VT encerra vínculos abertos

**Files:**
- Modify: `src/Beneficios.Application/Services/FuncionarioService.cs`
- Modify: `tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs` (ou criar se insuficiente)
- Consumes: `IFuncionarioLinhaRepository.EncerrarAbertosPorFuncionarioAsync`

**Interfaces:**
- Após `AtualizarAsync`/`SalvarAsync` persistir benefícios: se a lista normalizada **não** contém VT ativo (ausente ou `Ativo == false`), chamar:

```csharp
await _funcionarioLinhaRepository.EncerrarAbertosPorFuncionarioAsync(
    funcionarioId,
    DateOnly.FromDateTime(DateTime.UtcNow),
    DateTime.UtcNow,
    usuarioAlteracaoId);
```

Injetar `IFuncionarioLinhaRepository` no `FuncionarioService` (ou método no `IFuncionarioLinhaService` `EncerrarAbertosPorFuncionarioAsync` se preferir não acoplar Application→repo de outro agregado — **preferir** método no `IFuncionarioLinhaService` para manter o padrão de serviços).

Se usar service:

```csharp
Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId);
```

- [ ] **Step 1: Write failing test** — atualizar funcionário com VT `ativo: false` → verifica chamada a encerrar vínculos

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement hook no `FuncionarioService`**

- [ ] **Step 4: Run FuncionarioService + FuncionarioLinha tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Application/Services/FuncionarioService.cs src/Beneficios.Application/Services/FuncionarioLinhaService.cs src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs
git commit -m "feat: close open bus-line links when VT is deactivated"
```

---

### Task 8: Frontend — Linhas de Ônibus

**Files:**
- Create: `Beneficios.Front/src/app/core/api/linha-onibus.models.ts`
- Create: `Beneficios.Front/src/app/core/api/linha-onibus.service.ts`
- Create: `features/linhas-onibus/linha-onibus-list/*` (ts/html/scss)
- Create: `features/linhas-onibus/linha-onibus-form/*`
- Modify: `app.routes.ts`

**Layout:** copiar estrutura visual de `afastamento-list` / `afastamento-form` (ou `funcionario-list`/`funcionario-form`) — mesmos padrões Material, filtros, botões. **Não** inventar layout novo. Usar reactive forms como nos cadastros existentes. Skill `angular-developer` + Context7 para dúvidas de Angular 22.

Models:

```typescript
export interface LinhaOnibusDto {
  id: string;
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}

export interface LinhaOnibusSalvarDto {
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}
```

Service: `GET/POST api/linhas-onibus`, `GET/PUT api/linhas-onibus/:id`.

Rotas:

```typescript
{
  path: 'linhas-onibus',
  canActivate: [tenantGuard, permissaoGuard('linhas_onibus')],
  loadComponent: () => import('...linha-onibus-list...').then(m => m.LinhaOnibusListComponent),
},
// nova + :id/editar idem
```

Listagem: colunas Descrição, Data início, Data fim (“Em aberto”), Tarifa; Novo/Editar; **sem** Excluir.  
Form: descrição, datas, tarifa ≥ 0.

- [ ] **Step 1: Scaffold list + form + service + routes (BOM)**

- [ ] **Step 2: `npx ng build` no `Beneficios.Front` — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add Beneficios.Front/src/app/core/api/linha-onibus.* Beneficios.Front/src/app/features/linhas-onibus Beneficios.Front/src/app/app.routes.ts
git commit -m "feat(front): add linhas-onibus list and form"
```

---

### Task 9: Frontend — Funcionário × Linhas + atalho

**Files:**
- Create: `funcionario-linha.models.ts`, `funcionario-linha.service.ts`, list, form
- Modify: `app.routes.ts`
- Modify: `funcionario-list.component.ts/html`
- Modify: `funcionario-form.component.html` (link como afastamentos)

Atalho listagem (espelhar afastamentos):

```typescript
readonly podeVerLinhas = this.permissao.possuiPermissao('funcionario_linhas', 'visualizar');
```

```html
@if (podeVerLinhas) {
  <a mat-button [routerLink]="['/funcionario-linhas']" [queryParams]="{ funcionarioId: row.id }">Linhas</a>
}
```

Form edição: link `Ver linhas de ônibus` com `funcionarioId: id()`.

Listagem vínculos: lê `funcionarioId` da query; colunas Funcionário, Linha, Qtd, datas; Novo/Editar sem Excluir.  
Form: funcionário, linha (filtrar vigentes via `linhas-onibus?somenteVigentes=true` ou filtro client), quantidade ≥ 1, datas. Pré-selecionar funcionário se query na rota `/novo`.

- [ ] **Step 1: Implement list/form/service/routes + atalhos**

- [ ] **Step 2: `npx ng build` — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add Beneficios.Front/src/app/core/api/funcionario-linha.* Beneficios.Front/src/app/features/funcionario-linhas Beneficios.Front/src/app/app.routes.ts Beneficios.Front/src/app/features/funcionarios
git commit -m "feat(front): add funcionario-linhas UI and funcionario shortcuts"
```

---

### Task 10: Cobertura ≥ 80%, E2E smoke e verificação final

**Files:**
- Expandir testes Application/Api/Repository conforme gaps do `scripts/check-coverage.ps1`
- Opcional: `e2e/fixtures/api-mocks.ts` + `e2e/tenant-flows.spec.ts` — mocks das novas rotas e smoke mínimo

- [ ] **Step 1: Run backend tests + coverage**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj -v q`  
Run: `powershell -ExecutionPolicy Bypass -File scripts/check-coverage.ps1`  
Expected: PASS com cobertura ≥ 80%

- [ ] **Step 2: Fill gaps** — testes faltantes em services/controllers/repos até passar o script

- [ ] **Step 3: Front build + unit tests relevantes**

Run: `cd Beneficios.Front; npx ng build; npx ng test --watch=false --browsers=ChromeHeadless` (ou o comando que o repo usa)

- [ ] **Step 4: Commit**

```bash
git add tests/ Beneficios.Front/e2e/
git commit -m "test: expand linhas-onibus coverage to meet Sonar threshold"
```

---

## Self-review (plan vs spec)

| Spec requirement | Task |
|------------------|------|
| Tabelas `linhas_onibus` / `funcionario_linhas` | 2–4 |
| UNIQUE par funcionário+linha | 3, 6 |
| VT gate + auto-encerra | 6, 7 |
| Bloqueio encerrar linha | 5 |
| Dois módulos perfil + atalho | 1, 9 |
| Sem DELETE | 5, 6, 8, 9 |
| Layout existente | 8, 9 |
| BOM + coverage ≥ 80% | Global + 10 |
| Motor fora | — (não há task) |

Placeholders: nenhum intencional. Tipos alinhados entre tasks (`LinhaOnibusId`, mensagens PT do spec).

---

## Execution handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-16-linhas-onibus.md`. Two execution options:

**1. Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
**2. Inline Execution** — execute tasks in this session with executing-plans checkpoints  

Which approach?
