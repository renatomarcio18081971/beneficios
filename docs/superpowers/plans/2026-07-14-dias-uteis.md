# Dias Úteis (calendário tenant) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar calendário anual dia a dia por empresa (tenant), com geração seg–sex/sáb–dom + feriados nacionais, tela mensal simples, botão Gerar ano, job de virada de ano e módulo `dias_uteis` no perfil de acesso.

**Architecture:** Tabela `calendario_dias` por schema tenant. Catálogo de feriados nacionais no Domain (fixos + móveis via Páscoa). `CalendarioDiaService.GerarAnoAsync` idempotente. Provisionamento gera o ano corrente; `BackgroundService` + startup garante ano corrente ausente. Front: grade mensal + diálogo de edição. Perfil: entrada `dias_uteis` no catálogo com enforcement front + API.

**Tech Stack:** .NET 9, Dapper, PostgreSQL schemas, xUnit/Moq; Angular 22 standalone + Material; UTF-8 com BOM em todo arquivo novo/alterado.

**Spec:** `docs/superpowers/specs/2026-07-14-dias-uteis-design.md`

## Global Constraints

- Encoding: **UTF-8 com BOM** em todo arquivo criado ou alterado
- Escopo: **somente tenant**; admin fora da v1
- Métodos e tipos de domínio novos em **português** claro (`GerarAnoAsync`, `ObterPorMesAsync`, etc.)
- `GerarAno`: se o ano já tem dias → **não sobrescreve**; mensagem `Este ano já está cadastrado.`
- Edição de dia → `origem = manual`
- Sem DELETE de dia; Excluir no perfil reservado sem UI/endpoint
- Dashboard continua fora do catálogo de permissões
- Backend: padrões `*Params`, `*Dto`, AutoMapper `*Profile`, controllers enxutos
- Cobertura backend ≥ 80% (`scripts/check-coverage.ps1`)
- Seguir padrões de Usuários/Perfis (Repository Dapper, list/form patterns no front adaptados à grade)

---

## File Map

### Backend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Domain/Enums/TipoExcecaoCalendario.cs` | nacional, estadual, municipal, ferias_coletivas, ponto_facultativo |
| `src/Beneficios.Domain/Enums/OrigemCalendarioDia.cs` | geracao, nacional, manual |
| `src/Beneficios.Domain/FeriadosNacionaisCatalog.cs` | Lista fixos + móveis por ano |
| `src/Beneficios.Domain/Entities/CalendarioDia.cs` | Entidade |
| `src/Beneficios.Domain/Models/CalendarioDia*Params.cs` / `CalendarioDiaQueryResult.cs` | Persistência |
| `src/Beneficios.Domain/Interfaces/ICalendarioDiaRepository.cs` | Contrato |
| `src/Beneficios.Application/DTOs/CalendarioDia*.cs` | DTOs API |
| `src/Beneficios.Application/Interfaces/ICalendarioDiaService.cs` | Serviço |
| `src/Beneficios.Application/Services/CalendarioDiaService.cs` | GerarAno + CRUD consulta/update |
| `src/Beneficios.Application/Mappings/CalendarioDiaProfile.cs` | AutoMapper |
| `src/Beneficios.Domain/Interfaces/ICalendarioAnoGarantia.cs` | Garantir ano corrente em todos tenants |
| `src/Beneficios.Infrastructure/Repositories/CalendarioDiaRepository.cs` | Dapper |
| `src/Beneficios.Infrastructure/Tenancy/CalendarioAnoGarantia.cs` | Loop schemas + GerarAno (connection catalog) |
| `src/Beneficios.Api/Background/CalendarioAnoHostedService.cs` | Timer diário |
| `src/Beneficios.Api/Controllers/DiasUteisController.cs` | `api/dias-uteis` |
| `src/Beneficios.Infrastructure/Scripts/10_Create_Calendario_Dias_Tenant.sql` | DDL referência |

### Backend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `ModulosSistemaCatalog.cs` | + `dias_uteis` |
| `TenantSchemaSql.cs` | `CriarTabelaCalendarioDias` |
| `TenantProvisioner.cs` | DDL + GerarAno ano corrente |
| `ITenantProvisioner.cs` | Se necessário expor garantia calendário |
| `Program.cs` | DI + hosted service + startup garantia |
| `PostgresFixture.cs` | Criar tabela calendário no provision de teste |
| Testes Domain/Application/Infrastructure/Api | Novos + catálogo módulos |

### Frontend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/api/dias-uteis.models.ts` / `dias-uteis.service.ts` | HTTP |
| `features/dias-uteis/dias-uteis-page/*` | Grade mensal + legenda + Gerar ano |
| `features/dias-uteis/dia-editar-dialog/*` | Diálogo editar dia |

### Frontend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/auth/modulos-sistema.ts` | + `dias_uteis` |
| `app.routes.ts` | Rota `/dias-uteis` + guards |
| `layout/shell/shell.component.ts` | Menu via `MODULOS_SISTEMA` (já filtra Visualizar) |
| E2E tenant fixtures/flows | Smoke dias úteis |

---

### Task 1: Catálogo de feriados nacionais

**Files:**
- Create: `src/Beneficios.Domain/FeriadosNacionaisCatalog.cs`
- Test: `tests/Beneficios.Tests/Domain/FeriadosNacionaisCatalogTests.cs`

**Interfaces:**
- Produces: `FeriadosNacionaisCatalog.ObterParaAno(int ano) → IReadOnlyList<FeriadoNacional>` onde `FeriadoNacional` é `record(DateOnly Data, string Nome)`
- Produces: datas móveis derivadas da Páscoa (algoritmo de Meeus/Jones/Butcher ou equivalente documentado no teste)

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void ObterParaAno_2026_DeveConterFeriadosFixosEMoveisConhecidos()
{
    var feriados = FeriadosNacionaisCatalog.ObterParaAno(2026);
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 1, 1) && f.Nome.Contains("Confraterniza", StringComparison.OrdinalIgnoreCase));
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 12, 25));
    // Carnaval 2026-02-16/17 (segunda/terça) — usar a regra do catálogo (terça de carnaval oficial)
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 4, 3)); // Sexta-feira Santa (Páscoa 2026-04-05 - 2)
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 2, 17)); // Carnaval (terça)
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~FeriadosNacionaisCatalogTests" -v n`  
Expected: FAIL (tipo/catálogo inexistente)

- [ ] **Step 3: Write minimal implementation**

Implementar `FeriadosNacionaisCatalog` com:
- Fixos: 01/01, 21/04, 01/05, 07/09, 12/10, 02/11, 15/11, 25/12 (e 20/11 Dia da Consciência Negra se adotado como nacional no escopo — **incluir** 20/11)
- Móveis a partir da Páscoa: Carnaval (terça = Páscoa−47), Sexta-feira Santa (Páscoa−2), Corpus Christi (Páscoa+60)

- [ ] **Step 4: Run test to verify it passes**

Run: mesmo comando — Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/FeriadosNacionaisCatalog.cs tests/Beneficios.Tests/Domain/FeriadosNacionaisCatalogTests.cs
git commit -m "feat(domain): add Brazilian national holidays catalog"
```

---

### Task 2: Enums, modelos e módulo `dias_uteis` no catálogo

**Files:**
- Create: `src/Beneficios.Domain/Enums/TipoExcecaoCalendario.cs`
- Create: `src/Beneficios.Domain/Enums/OrigemCalendarioDia.cs`
- Create: `src/Beneficios.Domain/Entities/CalendarioDia.cs`
- Create: `src/Beneficios.Domain/Models/CalendarioDiaQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/CalendarioDiaSalvarParams.cs` (bulk insert na geração)
- Create: `src/Beneficios.Domain/Models/CalendarioDiaAtualizarParams.cs`
- Modify: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Modify: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: enums string-backed ou nomes PascalCase mapeados para valores da spec (`nacional`, `geracao`, …) de forma estável no banco
- Produces: `ModulosSistemaCatalog` contendo `dias_uteis` → `/dias-uteis` com `AcaoPermissao.Todas`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Todos_DeveConterDiasUteis()
{
    Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "dias_uteis" && m.Rota == "/dias-uteis");
}
```

- [ ] **Step 2: Run test — Expected: FAIL**

- [ ] **Step 3: Implement enums/entity/params + atualizar catálogo**

- [ ] **Step 4: Run tests Domain — Expected: PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(domain): add calendario dia models and dias_uteis module"
```

---

### Task 3: DDL tenant + script referência

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/10_Create_Calendario_Dias_Tenant.sql`
- Modify: `tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs` (`ProvisionTenantSchemaAsync`)

**Interfaces:**
- Produces: `TenantSchemaSql.CriarTabelaCalendarioDias(string schemaName) → string` (IF NOT EXISTS, UNIQUE data, índices)

- [ ] **Step 1: Write failing test** (provision fixture / TenantProvisioner smoke que a tabela existe)

```csharp
[SkippableFact]
public async Task Provisionar_DeveCriarTabelaCalendarioDias()
{
    // após ProvisionarAsync ou ProvisionTenantSchemaAsync
    var existe = await connection.ExecuteScalarAsync<bool>(
        $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = @s AND table_name = 'calendario_dias')",
        new { s = schemaName });
    Assert.True(existe);
}
```

- [ ] **Step 2: Run — Expected: FAIL**

- [ ] **Step 3: Implement DDL + chamar no `PostgresFixture.ProvisionTenantSchemaAsync` e no fluxo de provision (Task 7 completa o GerarAno)**

Nesta task: só garantir `CriarTabelaCalendarioDias` chamado em `TenantProvisioner.ProvisionarAsync` **antes** do usuário padrão (junto às outras tabelas). Geração de dias fica na Task 7 se o serviço ainda não existir — ou criar tabela aqui e geração depois.

Ordem sugerida no provisioner após usuários/perfis:
1. `CriarTabelaCalendarioDias`
2. (Task 7) `GerarAno`

- [ ] **Step 4: Run infrastructure smoke — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(infra): add calendario_dias tenant DDL"
```

---

### Task 4: `ICalendarioDiaRepository` + implementação

**Files:**
- Create: `src/Beneficios.Domain/Interfaces/ICalendarioDiaRepository.cs`
- Create: `src/Beneficios.Infrastructure/Repositories/CalendarioDiaRepository.cs`
- Test: `tests/Beneficios.Tests/Infrastructure/CalendarioDiaRepositoryTests.cs`

**Interfaces:**
- Produces:
  - `Task<bool> AnoExisteAsync(int ano)`
  - `Task InserirLoteAsync(IReadOnlyList<CalendarioDiaSalvarParams> dias)`
  - `Task<CalendarioDiaQueryResult[]> ObterPorMesAsync(int ano, int mes)`
  - `Task<CalendarioDiaQueryResult?> ObterPorIdAsync(Guid id)`
  - `Task<bool> AtualizarAsync(CalendarioDiaAtualizarParams params)`

- [ ] **Step 1: Write failing repository tests** (inserir lote de 3 dias, obter mês, atualizar)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement repository** (Dapper, `IDbConnection` scoped do tenant — igual `PerfilRepository`)

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(infra): add CalendarioDiaRepository"
```

---

### Task 5: `CalendarioDiaService` (GerarAno + atualizar)

**Files:**
- Create: Application DTOs + `ICalendarioDiaService` + `CalendarioDiaService` + AutoMapper profile
- Test: `tests/Beneficios.Tests/Application/CalendarioDiaServiceTests.cs`

**Interfaces:**
- Consumes: `ICalendarioDiaRepository`, `FeriadosNacionaisCatalog`
- Produces:
  - `Task GerarAnoAsync(int ano)` — lança `InvalidOperationException("Este ano já está cadastrado.")` se `AnoExisteAsync`
  - `Task<CalendarioDiaDto[]> ObterPorMesAsync(int ano, int mes)`
  - `Task<CalendarioDiaDto?> ObterPorIdAsync(Guid id)`
  - `Task AtualizarAsync(Guid id, CalendarioDiaAtualizarDto dto, Guid? usuarioAlteracaoId)` — seta `Origem = Manual`

- [ ] **Step 1: Write failing service tests**

```csharp
[Fact]
public async Task GerarAnoAsync_QuandoAnoNovo_DeveInserirTodosOsDias()
{
    // mock repo AnoExiste=false; captura InserirLote; Assert count 365 ou 366
}

[Fact]
public async Task GerarAnoAsync_QuandoAnoExiste_DeveLancar()
{
    // AnoExiste=true → InvalidOperationException mensagem exata
}

[Fact]
public async Task AtualizarAsync_DeveMarcarOrigemManual()
{
    // verifica params.Origem == Manual
}
```

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement service**  
  - Geração: construir lista em memória (DateOnly loop) + aplicar feriados do catálogo  
  - Timezone: usar **data de calendário** sem conversão de fuso na data do dia (DateOnly)

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(app): add CalendarioDiaService with idempotent GerarAno"
```

---

### Task 6: API `DiasUteisController` + DI + permissões

**Files:**
- Create: `src/Beneficios.Api/Controllers/DiasUteisController.cs`
- Modify: `src/Beneficios.Api/Program.cs` (DI repository/service/mapper)
- Test: `tests/Beneficios.Tests/Api/DiasUteisControllerTests.cs`

**Interfaces:**
- `GET /api/dias-uteis?ano=&mes=` → Visualizar  
- `GET /api/dias-uteis/{id}` → Visualizar  
- `PUT /api/dias-uteis/{id}` → Editar  
- `POST /api/dias-uteis/gerar-ano` body `{ "ano": 2027 }` → Criar; 400 com mensagem se já existe  
- `CodigoMenu = "dias_uteis"`; admin tenant bypass igual UsuariosController

- [ ] **Step 1: Write failing controller tests** (403 / happy path com mocks)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement controller + DI**

- [ ] **Step 4: Run API tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(api): add dias-uteis endpoints with permission checks"
```

---

### Task 7: Provisionamento + garantia de ano (startup + hosted service)

**Files:**
- Create: `src/Beneficios.Domain/Interfaces/ICalendarioAnoGarantia.cs`
- Create: `src/Beneficios.Infrastructure/Tenancy/CalendarioAnoGarantia.cs`
- Create: `src/Beneficios.Api/Background/CalendarioAnoHostedService.cs`
- Modify: `TenantProvisioner.cs` — após DDL calendário, gerar ano corrente no schema (via SQL/lote no mesmo connection **ou** helper estático compartilhado com a garantia)
- Modify: `Program.cs` — `AddHostedService` + try/catch startup `GarantirAnoCorrenteEmTenantsExistentesAsync`
- Test: `tests/Beneficios.Tests/Infrastructure/CalendarioAnoGarantiaTests.cs` e/ou extensão `TenantProvisionerTests`

**Interfaces:**
- Produces: `Task GarantirAnoCorrenteEmTenantsExistentesAsync(CancellationToken ct = default)`  
  - Lista `tenant_%`  
  - Para cada schema: `search_path` / SQL qualified: se não há dias no ano UTC corrente → gerar  
- HostedService: delay inicial curto + loop a cada 24h; falhas logadas

**Nota de design:** Gerar ano no provisioner pode duplicar lógica do service. Preferir extrair `CalendarioAnoGerador` interno (Infrastructure) usado por provisioner (connection explícita + schema) **e** por `CalendarioAnoGarantia`, enquanto `CalendarioDiaService` usa o gerador via repository do request scope. Alternativa aceitável: no provisioner abrir connection com SearchPath do tenant e resolver `ICalendarioDiaService` não funciona facilmente — então **gerador estático/infra** compartilhado é a opção recomendada nesta task.

- [ ] **Step 1: Write failing tests** (tenant sem tabela dias do ano → garantia cria; segundo run não duplica)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement gerador compartilhado + provisioner + garantia + hosted service + Program.cs**

- [ ] **Step 4: Run tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat: provision and yearly job for tenant calendars"
```

---

### Task 8: Front — módulo, API client e rota

**Files:**
- Modify: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Create: `Beneficios.Front/src/app/core/api/dias-uteis.models.ts`
- Create: `Beneficios.Front/src/app/core/api/dias-uteis.service.ts`
- Modify: `Beneficios.Front/src/app/app.routes.ts`

**Interfaces:**
- Models alinhados ao DTO (`id`, `data`, `ehDiaUtil`, `tipoExcecao`, `origem`, `observacao`)
- Service: `listarPorMes(ano, mes)`, `obterPorId(id)`, `atualizar(id, body)`, `gerarAno(ano)`
- Rota: `path: 'dias-uteis'`, `canActivate: [tenantGuard, permissaoGuard('dias_uteis')]`

- [ ] **Step 1: Add module entry + service + route** (UTF-8 BOM)

- [ ] **Step 2: `npx ng build` — Expected: SUCCESS**

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add dias-uteis module route and API client"
```

---

### Task 9: Front — página calendário mensal + diálogo

**Files:**
- Create: `features/dias-uteis/dias-uteis-page/dias-uteis-page.component.{ts,html,scss}`
- Create: `features/dias-uteis/dia-editar-dialog/dia-editar-dialog.component.{ts,html,scss}`

**UX (spec §6):**
- Ano + mês (setas)
- Botão Gerar ano se `possuiPermissao('dias_uteis','criar')`
- Legenda + grade; clique abre dialog
- Dialog: readonly data; toggle útil; select tipo; observação; Salvar se `editar`
- Ano sem dados: mensagem + CTA Gerar ano
- Sem filtros avançados / sem export

- [ ] **Step 1: Implement page + dialog** (Material: toolbar, buttons, dialog, select, slide-toggle ou radio)

- [ ] **Step 2: `npx ng build` — SUCCESS; smoke manual se API no ar**

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add monthly business-days calendar UI"
```

---

### Task 10: E2E smoke + verificação final

**Files:**
- Modify: `Beneficios.Front/e2e/fixtures/api-mocks.ts` (permissões `dias_uteis` full no Dono mock)
- Create/modify: `Beneficios.Front/e2e/tenant-flows.spec.ts` (abrir `/dias-uteis` após login)

- [ ] **Step 1: E2E** — login tenant → navegar Dias úteis → grade visível (mock GET mês)

- [ ] **Step 2: Backend**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj
# opcional: scripts/check-coverage.ps1
```

Expected: PASS; cobertura ≥ 80%

- [ ] **Step 3: Front**

```bash
cd Beneficios.Front && npx ng build && npx ng test --watch=false --browsers=ChromeHeadless
```

- [ ] **Step 4: Commit**

```bash
git commit -m "test: cover dias-uteis flows and finalize feature verification"
```

---

## Spec coverage checklist

| Spec | Task |
|------|------|
| Catálogo feriados nacionais | 1 |
| Modelos + módulo perfil `dias_uteis` | 2 |
| DDL `calendario_dias` | 3 |
| Repository | 4 |
| GerarAno idempotente + update manual | 5 |
| API + permissões | 6 |
| Provision + job/startup | 7 |
| Front client/rota/catálogo | 8 |
| UI mensal | 9 |
| E2E + verificação | 10 |
| Fora da v1 (jornada, export…) | não implementar |

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-14-dias-uteis.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — execute tasks in this session with `executing-plans` checkpoints  

Which approach?
