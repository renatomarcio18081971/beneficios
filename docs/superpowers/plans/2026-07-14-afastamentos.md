# Afastamentos / Férias — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar CRUD de afastamentos/férias no tenant (menu próprio + atalho na listagem de funcionários), com derivação automática de `situacao`, remoção de `motivo_afastamento` e provisionamento da tabela `funcionario_afastamentos`.

**Architecture:** Tabela `funcionario_afastamentos` por schema tenant. Domain com enum/tipos, overlap e “ativo hoje”. Repository Dapper + Service com validação e recalc de `funcionarios.situacao`. API top-level `api/afastamentos` (GET/POST/PUT/DELETE). Front list/form no padrão Funcionários; atalho `?funcionarioId=` na listagem. Ajustes no módulo Funcionários (situação só Ativo/Desligado; coluna Motivo = tipo ativo).

**Tech Stack:** .NET 9, Dapper, PostgreSQL schemas, xUnit/Moq; Angular 22 standalone + Material; UTF-8 com BOM em todo arquivo novo/alterado.

**Spec:** `docs/superpowers/specs/2026-07-14-afastamentos-design.md`

## Global Constraints

- Encoding: **UTF-8 com BOM** em todo arquivo criado ou alterado
- Escopo: **somente tenant**; admin fora da v1
- Métodos/tipos de domínio novos em **português** (`SalvarAsync`, `ObterPorIdAsync`, `FiltrarAsync`, etc.)
- Data fim **opcional**; sobreposição no mesmo funcionário **bloqueada** (fim NULL = infinito)
- `situacao` do funcionário: após CRUD de afastamento, se não `desligado` → `afastado` se ativo hoje, senão `ativo`
- Save de funcionário: situação apenas `ativo` | `desligado`; rejeitar `afastado`; impedir `ativo` se houver afastamento ativo
- Remover coluna/campo `motivo_afastamento`; listagem expõe `motivoAfastamentoAtivo` (calculado)
- Motor / API de exclusão de dias: **fora desta entrega**
- DELETE de afastamento: **sim**; DELETE de funcionário: continua inexistente
- Dashboard fora do catálogo de permissões
- Backend: `*Params`, `*Dto`, AutoMapper `*Profile`, controllers enxutos
- Cobertura backend ≥ 80% (`scripts/check-coverage.ps1`)
- Seguir padrões de Funcionários / Dias úteis / Usuários

---

## File Map

### Backend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Domain/Enums/TipoAfastamento.cs` | Enum dos 6 tipos |
| `src/Beneficios.Domain/TipoAfastamentoCatalog.cs` | Labels + conversão string ↔ enum |
| `src/Beneficios.Domain/AfastamentoPeriodo.cs` | Helpers `Sobrepoe`, `EstaAtivoEm` |
| `src/Beneficios.Domain/Models/FuncionarioAfastamento*Params.cs` / Query / Filtro | Persistência |
| `src/Beneficios.Domain/Interfaces/IFuncionarioAfastamentoRepository.cs` | Contrato |
| `src/Beneficios.Application/DTOs/AfastamentoDtos.cs` | DTOs API |
| `src/Beneficios.Application/Interfaces/IFuncionarioAfastamentoService.cs` | Serviço |
| `src/Beneficios.Application/Services/FuncionarioAfastamentoService.cs` | Validação + recalc |
| `src/Beneficios.Application/Mappings/AfastamentoProfile.cs` | AutoMapper |
| `src/Beneficios.Infrastructure/Repositories/FuncionarioAfastamentoRepository.cs` | Dapper |
| `src/Beneficios.Infrastructure/Scripts/12_Create_Funcionario_Afastamentos_Tenant.sql` | DDL referência |
| `src/Beneficios.Infrastructure/Scripts/13_Drop_Motivo_Afastamento_Funcionarios.sql` | DROP COLUMN |
| `src/Beneficios.Api/Controllers/AfastamentosController.cs` | `api/afastamentos` |

### Backend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `ModulosSistemaCatalog.cs` | + `afastamentos` |
| `TenantSchemaSql.cs` | `CriarTabelaFuncionarioAfastamentos` + `DropMotivoAfastamentoFuncionarios` |
| `TenantProvisioner.cs` | DDL + drop em Provisionar/Garantir; recalc situações |
| `IFuncionarioRepository` / `FuncionarioRepository` | Remover motivo; `AtualizarSituacaoAsync`; join/subquery tipo ativo; listar ids para recalc |
| `Funcionario*Params` / `FuncionarioQueryResult` / DTOs / Profile / Service | Remover motivo; regras situação; `MotivoAfastamentoAtivo` |
| `Program.cs` | DI |
| `PostgresFixture.cs` | DDL afastamentos + drop motivo |
| Testes Domain/Application/Api + `ModulosSistemaCatalogTests` | Novos/ajustes |

### Frontend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/api/afastamento.models.ts` / `afastamento.service.ts` | HTTP |
| `features/afastamentos/afastamento-list/*` | Listagem + filtros + query `funcionarioId` |
| `features/afastamentos/afastamento-form/*` | Formulário CRUD |

### Frontend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/auth/modulos-sistema.ts` | + `afastamentos` |
| `app.routes.ts` | Rotas + guards |
| `funcionario.models.ts` / form / list | Remover motivo livre; Motivo coluna; atalho; situação Ativo/Desligado |
| `e2e/fixtures/api-mocks.ts` + `e2e/tenant-flows.spec.ts` | Smoke |

---

## Task breakdown

### Task 1: Enum, catálogo e helpers de período

**Files:**
- Create: `src/Beneficios.Domain/Enums/TipoAfastamento.cs`
- Create: `src/Beneficios.Domain/TipoAfastamentoCatalog.cs`
- Create: `src/Beneficios.Domain/AfastamentoPeriodo.cs`
- Test: `tests/Beneficios.Tests/Domain/AfastamentoPeriodoTests.cs`
- Test: `tests/Beneficios.Tests/Domain/TipoAfastamentoCatalogTests.cs`

**Interfaces:**
- Produces:

```csharp
public enum TipoAfastamento
{
    Ferias = 1,
    LicencaMedica = 2,
    MaternidadePaternidade = 3,
    Acidente = 4,
    Suspensao = 5,
    Outros = 6
}

public static class TipoAfastamentoCatalog
{
    public static string ParaBanco(TipoAfastamento tipo); // ferias | licenca_medica | ...
    public static TipoAfastamento DeBanco(string codigo);
    public static string Label(TipoAfastamento tipo);
    public static IReadOnlyList<(string codigo, string label)> Todos { get; }
}

public static class AfastamentoPeriodo
{
    // fim null = +∞. Conflito: iniA <= fimB && iniB <= fimA
    public static bool Sobrepoe(DateOnly iniA, DateOnly? fimA, DateOnly iniB, DateOnly? fimB);

    // dataInicio <= ref && (dataFim is null || dataFim >= ref)
    public static bool EstaAtivoEm(DateOnly dataInicio, DateOnly? dataFim, DateOnly referencia);
}
```

Códigos banco: `ferias`, `licenca_medica`, `maternidade_paternidade`, `acidente`, `suspensao`, `outros`.

- [ ] **Step 1: Write failing tests**

```csharp
[Fact]
public void Sobrepoe_IntervalosAbertos_DeveDetectarConflito()
{
    Assert.True(AfastamentoPeriodo.Sobrepoe(
        new DateOnly(2026, 1, 1), null,
        new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)));
}

[Fact]
public void Sobrepoe_IntervalosDisjuntos_NaoConflita()
{
    Assert.False(AfastamentoPeriodo.Sobrepoe(
        new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
        new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)));
}

[Fact]
public void EstaAtivoEm_FimNulo_TrueSeJaComecou()
{
    Assert.True(AfastamentoPeriodo.EstaAtivoEm(
        new DateOnly(2026, 1, 1), null, new DateOnly(2026, 7, 14)));
}

[Fact]
public void Catalog_Ferias_CodigoELabel()
{
    Assert.Equal("ferias", TipoAfastamentoCatalog.ParaBanco(TipoAfastamento.Ferias));
    Assert.Equal("Férias", TipoAfastamentoCatalog.Label(TipoAfastamento.Ferias));
}
```

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~AfastamentoPeriodo|FullyQualifiedName~TipoAfastamentoCatalog" -v q
```

Expected: FAIL (types missing)

- [ ] **Step 3: Implement enum, catalog, `AfastamentoPeriodo` (UTF-8 BOM)**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/Enums/TipoAfastamento.cs src/Beneficios.Domain/TipoAfastamentoCatalog.cs src/Beneficios.Domain/AfastamentoPeriodo.cs tests/Beneficios.Tests/Domain/AfastamentoPeriodoTests.cs tests/Beneficios.Tests/Domain/TipoAfastamentoCatalogTests.cs
git commit -m "feat(domain): add tipo afastamento and period overlap helpers"
```

---

### Task 2: Módulo `afastamentos` no catálogo de perfil

**Files:**
- Modify: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Modify: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Test: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: `new("afastamentos", "Afastamentos/Férias", "/afastamentos", AcaoPermissao.Todas)`
- Front: `codigo: 'afastamentos'`, `nomeExibicao: 'Afastamentos/Férias'`, `rota: '/afastamentos'`, `icone: 'event_busy'`, ações todas

- [ ] **Step 1: Extend failing assertions** — `Assert.Contains("afastamentos", codigos)` e rota `/afastamentos`

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q
```

- [ ] **Step 3: Add module back + front (UTF-8 BOM)**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/ModulosSistemaCatalog.cs Beneficios.Front/src/app/core/auth/modulos-sistema.ts tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
git commit -m "feat: add afastamentos module to permission catalog"
```

---

### Task 3: Models + `IFuncionarioAfastamentoRepository` + extensão em `IFuncionarioRepository`

**Files:**
- Create: `src/Beneficios.Domain/Models/FuncionarioAfastamentoSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioAfastamentoAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioAfastamentoQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioAfastamentoFiltroParams.cs`
- Create: `src/Beneficios.Domain/Interfaces/IFuncionarioAfastamentoRepository.cs`
- Modify: `src/Beneficios.Domain/Interfaces/IFuncionarioRepository.cs`
- Modify: `src/Beneficios.Domain/Models/FuncionarioQueryResult.cs` — remove `MotivoAfastamento`; add `MotivoAfastamentoAtivo` (`string?` código tipo)
- Modify: `src/Beneficios.Domain/Models/FuncionarioSalvarParams.cs` — remove `MotivoAfastamento`
- Modify: `src/Beneficios.Domain/Models/FuncionarioAtualizarParams.cs` — remove `MotivoAfastamento`

**Interfaces:**
- Produces:

```csharp
public sealed class FuncionarioAfastamentoSalvarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public string Tipo { get; init; } = ""; // código banco
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public string? Observacao { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public sealed class FuncionarioAfastamentoAtualizarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public string Tipo { get; init; } = "";
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public string? Observacao { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public sealed class FuncionarioAfastamentoQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = "";
    public string Tipo { get; set; } = "";
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Observacao { get; set; }
}

public sealed class FuncionarioAfastamentoFiltroParams
{
    public Guid? FuncionarioId { get; init; }
    public string? Tipo { get; init; }
    public DateOnly? DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
}

public interface IFuncionarioAfastamentoRepository
{
    Task<Guid> SalvarAsync(FuncionarioAfastamentoSalvarParams parametros);
    Task AtualizarAsync(FuncionarioAfastamentoAtualizarParams parametros);
    Task ExcluirAsync(Guid id);
    Task<FuncionarioAfastamentoQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> FiltrarAsync(FuncionarioAfastamentoFiltroParams filtro);
    Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> ListarPorFuncionarioAsync(Guid funcionarioId);
    Task<bool> ExisteSobreposicaoAsync(Guid funcionarioId, DateOnly dataInicio, DateOnly? dataFim, Guid? excetoId = null);
    Task<FuncionarioAfastamentoQueryResult?> ObterAtivoEmAsync(Guid funcionarioId, DateOnly referencia);
}

// Em IFuncionarioRepository, adicionar:
Task AtualizarSituacaoAsync(Guid funcionarioId, string situacaoBanco);
Task<IReadOnlyList<(Guid Id, string Situacao)>> ListarIdSituacaoAsync();
```

`FiltrarAsync` período: se filtro tem `DataInicio`/`DataFim`, retornar registros que **intersectam** (mesma regra `Sobrepoe`; filtro fim omitido = aberto).

- [ ] **Step 1: Create/modify models + interfaces (UTF-8 BOM)** — também atualizar SQL/params de Funcionario no step de repo se o build quebrar cedo; esta task só Domain

- [ ] **Step 2: Build Domain — PASS**

```bash
dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q
```

- [ ] **Step 3: Commit**

```bash
git add src/Beneficios.Domain/
git commit -m "feat(domain): add afastamento persistence contracts"
```

---

### Task 4: DDL tenant + provisionamento + drop motivo + fixture

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/12_Create_Funcionario_Afastamentos_Tenant.sql`
- Create: `src/Beneficios.Infrastructure/Scripts/13_Drop_Motivo_Afastamento_Funcionarios.sql`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs`
- Modify: `tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs` — remover `motivo_afastamento` de `CriarTabelaFuncionarios` (novos tenants já nascem sem a coluna)
- Test: extend provisioner/fixture assertions

**Interfaces:**
- Produces:

```csharp
public static string CriarTabelaFuncionarioAfastamentos(string schemaName)
{
    // CREATE TABLE IF NOT EXISTS {schema}.funcionario_afastamentos (
    //   id UUID PK, funcionario_id UUID NOT NULL FK → funcionarios(id),
    //   tipo VARCHAR(40) NOT NULL, data_inicio DATE NOT NULL, data_fim DATE NULL,
    //   observacao TEXT NULL, data_inclusao TIMESTAMP NOT NULL,
    //   data_alteracao TIMESTAMP NULL, usuario_alteracao_id UUID NULL
    // );
    // INDEX (funcionario_id); INDEX (data_inicio, data_fim);
}

public static string DropColunaMotivoAfastamento(string schemaName)
{
    // ALTER TABLE {schema}.funcionarios DROP COLUMN IF EXISTS motivo_afastamento;
}
```

Chamar em `ProvisionarAsync` e `GarantirPerfisNoSchemaAsync` (após tabelas de funcionários):
1. `CriarTabelaFuncionarioAfastamentos`
2. `DropColunaMotivoAfastamento`
3. Recalc situações: para cada funcionário com situacao ≠ `desligado`, se existe afastamento ativo hoje → `afastado`, senão → `ativo` (pode usar SQL set-based no provisioner ou serviço; preferir SQL no provisioner para não acoplar Application)

“Hoje” no recalc: `DateOnly.FromDateTime(DateTime.UtcNow)` **ou** clock já usado no provisioner (`agora`); documentar nos testes com data fixa se injetável — se `agora` já existe no método, usar `DateOnly.FromDateTime(agora)`.

- [ ] **Step 1: Write failing assertion** — após provision/garantia, `funcionario_afastamentos` existe; coluna `motivo_afastamento` **não** existe

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement DDL + scripts + wire provisioner + PostgresFixture**

- [ ] **Step 4: Run provisioner/infra tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(infra): add funcionario_afastamentos DDL and drop motivo_afastamento"
```

---

### Task 5: `FuncionarioAfastamentoRepository` + ajustes `FuncionarioRepository`

**Files:**
- Create: `src/Beneficios.Infrastructure/Repositories/FuncionarioAfastamentoRepository.cs`
- Modify: `src/Beneficios.Infrastructure/Repositories/FuncionarioRepository.cs` — remover motivo SQL; mapear `MotivoAfastamentoAtivo` via subquery do afastamento ativo (`data_inicio <= CURRENT_DATE AND (data_fim IS NULL OR data_fim >= CURRENT_DATE)`); implementar `AtualizarSituacaoAsync` e `ListarIdSituacaoAsync`
- Test: `tests/Beneficios.Tests/Infrastructure/FuncionarioAfastamentoRepositoryTests.cs` (se houver fixture Postgres; senão cobrir via service + integração mínima)

**Interfaces:**
- Consome: `IFuncionarioAfastamentoRepository`, params da Task 3
- `ExisteSobreposicaoAsync`: carregar períodos do funcionário (exceto `excetoId`) e aplicar `AfastamentoPeriodo.Sobrepoe` **ou** SQL equivalente; preferir SQL:

```sql
EXISTS (
  SELECT 1 FROM funcionario_afastamentos a
  WHERE a.funcionario_id = @FuncionarioId
    AND (@ExcetoId IS NULL OR a.id <> @ExcetoId)
    AND a.data_inicio <= COALESCE(@DataFim, '9999-12-31'::date)
    AND @DataInicio <= COALESCE(a.data_fim, '9999-12-31'::date)
)
```

- [ ] **Step 1: Implement repository (+ build fix em FuncionarioRepository sem motivo)**

- [ ] **Step 2: Register deferred until Task 7 DI — build Infrastructure**

```bash
dotnet build src/Beneficios.Infrastructure/Beneficios.Infrastructure.csproj -v q
```

- [ ] **Step 3: Test overlap + CRUD roundtrip se fixture disponível — PASS**

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(infra): implement funcionario afastamento repository"
```

---

### Task 6: DTOs, AutoMapper, `FuncionarioAfastamentoService` (TDD)

**Files:**
- Create: `src/Beneficios.Application/DTOs/AfastamentoDtos.cs`
- Create: `src/Beneficios.Application/Mappings/AfastamentoProfile.cs`
- Create: `src/Beneficios.Application/Interfaces/IFuncionarioAfastamentoService.cs`
- Create: `src/Beneficios.Application/Services/FuncionarioAfastamentoService.cs`
- Test: `tests/Beneficios.Tests/Application/FuncionarioAfastamentoServiceTests.cs`

**Interfaces:**
- Produces:

```csharp
public record AfastamentoDto(
    Guid Id, Guid FuncionarioId, string FuncionarioNome,
    TipoAfastamento Tipo, DateOnly DataInicio, DateOnly? DataFim, string? Observacao);

public record AfastamentoSalvarDto(
    Guid FuncionarioId, TipoAfastamento Tipo,
    DateOnly DataInicio, DateOnly? DataFim, string? Observacao);

public record AfastamentoAtualizarDto(
    TipoAfastamento Tipo, DateOnly DataInicio, DateOnly? DataFim, string? Observacao);

public interface IFuncionarioAfastamentoService
{
    Task<Guid> SalvarAsync(AfastamentoSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, AfastamentoAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task ExcluirAsync(Guid id);
    Task<AfastamentoDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<AfastamentoDto>> FiltrarAsync(Guid? funcionarioId, TipoAfastamento? tipo, DateOnly? dataInicio, DateOnly? dataFim);
}

public class FuncionarioAfastamentoService : IFuncionarioAfastamentoService
{
    public const string MensagemSobreposicao = "Já existe um afastamento neste período para o funcionário.";
    public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
    public const string MensagemFuncionarioNaoEncontrado = "Funcionário não encontrado.";
    public const string MensagemAfastamentoNaoEncontrado = "Afastamento não encontrado.";
    // deps: IFuncionarioAfastamentoRepository, IFuncionarioRepository, IMapper
    // "hoje": DateOnly.FromDateTime(DateTime.UtcNow) — same as provisioner; keep private method ObterHoje() for testability if needed
}
```

Fluxo `SalvarAsync`:
1. Se `DataFim < DataInicio` → throw `MensagemDataFim`
2. `ObterPorIdAsync` funcionário — se null → `MensagemFuncionarioNaoEncontrado`
3. Se `ExisteSobreposicaoAsync(...)` → `MensagemSobreposicao`
4. Persistir
5. `RecalcularSituacaoAsync(funcionarioId)`:
   - ler situação atual; se `desligado` return
   - se `ObterAtivoEmAsync(id, hoje)` não null → `AtualizarSituacaoAsync(..., "afastado")`
   - senão → `AtualizarSituacaoAsync(..., "ativo")`

`AtualizarAsync` / `ExcluirAsync`: mesmas validações (update exclui self no overlap); 404 se id inexistente.

- [ ] **Step 1: Write failing service tests**

```csharp
[Fact]
public async Task SalvarAsync_Sobreposicao_DeveFalhar()
{
    // setup funcionario existe; ExisteSobreposicaoAsync true
    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
    Assert.Equal(FuncionarioAfastamentoService.MensagemSobreposicao, ex.Message);
}

[Fact]
public async Task SalvarAsync_Valido_DevePersistirERecalcAfastado()
{
    // sem overlap; ObterAtivoEm retorna registro após save → verify AtualizarSituacaoAsync(..., "afastado")
}

[Fact]
public async Task ExcluirAsync_SemAtivo_DeveRecalcAtivo()
{
    // situacao atual afastado; após delete ObterAtivoEm null → "ativo"
}

[Fact]
public async Task Recalc_Desligado_NaoAltera()
{
    // situacao desligado → AtualizarSituacaoAsync nunca chamado
}
```

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~FuncionarioAfastamentoService" -v q
```

- [ ] **Step 3: Implement DTOs, Profile, Service**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(app): add funcionario afastamento service with situacao recalc"
```

---

### Task 7: Ajustar `FuncionarioService` (situação + remover motivo)

**Files:**
- Modify: `src/Beneficios.Application/DTOs/FuncionarioDtos.cs`
- Modify: `src/Beneficios.Application/Mappings/FuncionarioProfile.cs`
- Modify: `src/Beneficios.Application/Services/FuncionarioService.cs`
- Modify: `src/Beneficios.Application/Interfaces/IFuncionarioService.cs` se assinaturas mudarem
- Test: `tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs`

**Interfaces:**
- Remover `MotivoAfastamento` dos DTOs/params
- Em `FuncionarioDto` / list item: adicionar `string? MotivoAfastamentoAtivo` (código; front traduz label)
- Constantes novas:

```csharp
public const string MensagemSituacaoInvalida = "Situação inválida.";
public const string MensagemAtivoComAfastamento = "Existe afastamento ativo; encerre ou exclua o período antes de marcar como ativo.";
```

- `ValidarRegras`: se `situacao == Afastado` → `MensagemSituacaoInvalida`
- Em `SalvarAsync`/`AtualizarAsync`: se `situacao == Ativo`, chamar `IFuncionarioAfastamentoRepository.ObterAtivoEmAsync` (injetar repo de afastamento **ou** método em `IFuncionarioRepository`) — se houver ativo → `MensagemAtivoComAfastamento`
- Preferir injetar `IFuncionarioAfastamentoRepository` no `FuncionarioService` para não inchá-lo demais no FuncionarioRepository

- [ ] **Step 1: Write failing tests** — salvar com `Afastado` falha; salvar `Ativo` com afastamento ativo falha; CriarDto sem MotivoAfastamento

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement**

- [ ] **Step 4: Run FuncionarioServiceTests + AfastamentoServiceTests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(app): derive situacao rules and remove motivo_afastamento from funcionario"
```

---

### Task 8: `AfastamentosController` + DI

**Files:**
- Create: `src/Beneficios.Api/Controllers/AfastamentosController.cs`
- Modify: `src/Beneficios.Api/Program.cs`
- Test: `tests/Beneficios.Tests/Api/AfastamentosControllerTests.cs`

**Interfaces:**
- Mirror `FuncionariosController` / `DiasUteisController`:
  - `[Route("api/afastamentos")]`, `CodigoMenu = "afastamentos"`
  - GET ``?funcionarioId=&tipo=&dataInicio=&dataFim=`` → Visualizar
  - GET `{id}` → Visualizar
  - POST → Criar
  - PUT `{id}` → Editar
  - DELETE `{id}` → Excluir
  - `InvalidOperationException` → 400; message containing `"não encontrado"` → 404
- DI:

```csharp
builder.Services.AddScoped<IFuncionarioAfastamentoRepository, FuncionarioAfastamentoRepository>();
builder.Services.AddScoped<IFuncionarioAfastamentoService, FuncionarioAfastamentoService>();
```

Registrar `AfastamentoProfile` no AutoMapper (mesmo padrão dos demais profiles).

- [ ] **Step 1: Write controller tests** — 403 sem permissão; POST happy path Created; DELETE chama service

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement controller + DI**

- [ ] **Step 4: Run Api + Application tests — PASS**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~Afastamento|FullyQualifiedName~FuncionarioService" -v q
```

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(api): add afastamentos endpoints and DI"
```

---

### Task 9: Front — models, service, rotas, listagem, formulário

**Files:**
- Create: `Beneficios.Front/src/app/core/api/afastamento.models.ts`
- Create: `Beneficios.Front/src/app/core/api/afastamento.service.ts`
- Create: `Beneficios.Front/src/app/features/afastamentos/afastamento-list/*`
- Create: `Beneficios.Front/src/app/features/afastamentos/afastamento-form/*`
- Modify: `Beneficios.Front/src/app/app.routes.ts`

**Interfaces:**
- Models: tipos alinhados à API (`TipoAfastamento` union + `TIPOS_AFASTAMENTO` labels)
- Service: `filtrar`, `obterPorId`, `salvar`, `atualizar`, `excluir` → `/api/afastamentos`
- Routes (filhas do shell, como funcionários):

```typescript
{ path: 'afastamentos', canActivate: [tenantGuard, permissaoGuard('afastamentos')], loadComponent: ... list },
{ path: 'afastamentos/novo', canActivate: [...], loadComponent: ... form },
{ path: 'afastamentos/:id/editar', canActivate: [...], loadComponent: ... form },
```

- List: colunas Funcionário, Tipo, Início, Fim (“Em aberto”), Observação, ações; filtros; ler `ActivatedRoute` query `funcionarioId` e pré-filtrar; Novo → `/afastamentos/novo` com query se houver funcionarioId
- Form: campos do spec; permissões Criar/Editar/Excluir; confirmação excluir no padrão do projeto
- UX list/form: espelhar `funcionario-list` / `funcionario-form` (Material, signals, reactive forms)

- [ ] **Step 1: Create API client + components + routes (UTF-8 BOM)**

- [ ] **Step 2: `ng build` — PASS**

```bash
cd Beneficios.Front
npx ng build --configuration=development
```

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add afastamentos list, form, and routes"
```

---

### Task 10: Front — ajustes Funcionários + E2E smoke

**Files:**
- Modify: `Beneficios.Front/src/app/core/api/funcionario.models.ts`
- Modify: `Beneficios.Front/src/app/features/funcionarios/funcionario-list/*`
- Modify: `Beneficios.Front/src/app/features/funcionarios/funcionario-form/*`
- Modify: `Beneficios.Front/e2e/fixtures/api-mocks.ts`
- Modify: `Beneficios.Front/e2e/tenant-flows.spec.ts`

**Interfaces:**
- Remover `motivoAfastamento` dos models/requests
- Adicionar `motivoAfastamentoAtivo?: string | null`
- List: colunas incluir `cargo`, `motivo`, `jornada` conforme spec (Nome, CPF, Cargo, Situação, Motivo, Jornada, ações); Motivo = label do tipo ou `—`
- Ação ícone/botão → `routerLink="/afastamentos"` `[queryParams]="{ funcionarioId: row.id }"` se `possuiPermissao('afastamentos','visualizar')`
- Form: select situação só Ativo/Desligado; se carga vier `Afastado`, mostrar chip/leitura + link `/afastamentos?funcionarioId=`; sem campo motivo livre
- E2E: mock permissão `afastamentos` + lista vazia; smoke menu → `/afastamentos`

- [ ] **Step 1: Implement UI adjustments + mocks**

- [ ] **Step 2: `ng build` + E2E smoke relevante — PASS**

```bash
cd Beneficios.Front
npx ng build --configuration=development
npx playwright test e2e/tenant-flows.spec.ts --grep "afastamentos|Afastamentos" 
```

(Se o grep não achar, rode o spec inteiro e garanta assert do fluxo novo.)

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): wire afastamentos shortcut and funcionario situacao UI"
```

---

### Task 11: Verificação final de cobertura e regressão

**Files:** (nenhum novo obrigatório; só correções se falhar)

- [ ] **Step 1: Backend full test + coverage**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj -v q
pwsh scripts/check-coverage.ps1
```

Expected: todos PASS; cobertura ≥ 80%

- [ ] **Step 2: Front build**

```bash
cd Beneficios.Front
npx ng build --configuration=development
```

- [ ] **Step 3: Spot-check UTF-8 BOM** nos arquivos novos críticos (bytes `EF BB BF`) via PowerShell se o projeto exigir no checklist

- [ ] **Step 4: Commit** apenas se houver fixes

```bash
git commit -m "test: harden afastamentos coverage and regressions"
```

---

## Spec coverage checklist (self-review)

| Spec | Task |
|------|------|
| Tabela `funcionario_afastamentos` + índices | 4 |
| Tipos catálogo | 1, 9 |
| Data fim opcional + overlap bloqueado | 1, 5, 6 |
| Derivação situacao + desligado intocado | 4 (lote), 6 |
| Drop `motivo_afastamento` | 3, 4, 5, 7, 10 |
| `motivoAfastamentoAtivo` listagem | 5, 7, 10 |
| Save funcionário Ativo/Desligado only | 7, 10 |
| Impedir Ativo com afastamento ativo | 7 |
| Módulo perfil `afastamentos` | 2 |
| API CRUD + DELETE | 8 |
| Menu list/form | 9 |
| Atalho `?funcionarioId=` | 9, 10 |
| Provisionamento + garantia + recalc lote | 4 |
| Sem motor nesta v1 | (sem task — YAGNI) |
| Testes + cobertura | 1, 6, 7, 8, 11 |
| UTF-8 BOM | Global + Task 11 |

**Placeholder scan:** none intentionally left.  
**Type consistency:** `TipoAfastamento` enum Domain ↔ DTO ↔ front union with same bank codes via `TipoAfastamentoCatalog`.
