# Cadastro de Funcionários — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar CRUD de funcionários no tenant (identificação, contrato, localização, situação, VT vinculado, jornada fixa), com DDL no provisionamento e módulo `funcionarios` no perfil de acesso.

**Architecture:** Tabelas `funcionarios` + `funcionario_beneficios` por schema tenant. Enums e catálogo de jornada no Domain. Repository Dapper + Service com validação (CPF, matrícula, desligamento, jornada especial) e sync de VT. API `api/funcionarios` sem DELETE. Front list/form seccionado no padrão Usuários. Garantia DDL em tenants existentes no startup.

**Tech Stack:** .NET 9, Dapper, PostgreSQL schemas, xUnit/Moq; Angular 22 standalone + Material; UTF-8 com BOM em todo arquivo novo/alterado.

**Spec:** `docs/superpowers/specs/2026-07-14-funcionarios-design.md`

## Global Constraints

- Encoding: **UTF-8 com BOM** em todo arquivo criado ou alterado
- Escopo: **somente tenant**; admin fora da v1
- Métodos e tipos de domínio novos em **português** claro (`SalvarAsync`, `ObterPorIdAsync`, `FiltrarAsync`, etc.)
- **Sem DELETE** de funcionário; permissão Excluir no perfil **reservada** sem UI/endpoint
- CPF: somente dígitos, validação de dígitos verificadores, **único** no tenant
- Matrícula: **opcional**; se preenchida, **única** no tenant
- Situação `desligado` → `data_desligamento` **obrigatória**
- Jornada `especial_categoria` → `jornada_detalhe` **obrigatório**; demais → detalhe nulo
- Benefício v1: somente `vale_transporte` via tabela filha (upsert no save)
- Endereços estruturados; CEP máscara UI; **sem** API externa de CEP
- Dashboard continua fora do catálogo de permissões
- Backend: padrões `*Params`, `*Dto`, AutoMapper `*Profile`, controllers enxutos
- Cobertura backend ≥ 80% (`scripts/check-coverage.ps1`)
- Seguir padrões de Usuários/Perfis (Repository Dapper, list/form no front)

---

## File Map

### Backend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Domain/Enums/TipoContrato.cs` | clt, estagio, terceirizado, pj |
| `src/Beneficios.Domain/Enums/SituacaoFuncionario.cs` | ativo, afastado, desligado |
| `src/Beneficios.Domain/Enums/JornadaTrabalho.cs` | códigos da lista fixa |
| `src/Beneficios.Domain/JornadaTrabalhoCatalog.cs` | Labels + helper `ExigeDetalhe` |
| `src/Beneficios.Domain/CpfUtil.cs` | Normalizar + validar dígitos |
| `src/Beneficios.Domain/Entities/Funcionario.cs` | Entidade (opcional se só params) |
| `src/Beneficios.Domain/Models/Funcionario*Params.cs` / `FuncionarioQueryResult.cs` / `FuncionarioBeneficio*` | Persistência |
| `src/Beneficios.Domain/Models/FuncionarioFiltroParams.cs` | Filtro listagem |
| `src/Beneficios.Domain/Interfaces/IFuncionarioRepository.cs` | Contrato |
| `src/Beneficios.Application/DTOs/Funcionario*.cs` | DTOs API |
| `src/Beneficios.Application/Interfaces/IFuncionarioService.cs` | Serviço |
| `src/Beneficios.Application/Services/FuncionarioService.cs` | Validação + orchestrção |
| `src/Beneficios.Application/Mappings/FuncionarioProfile.cs` | AutoMapper |
| `src/Beneficios.Infrastructure/Repositories/FuncionarioRepository.cs` | Dapper |
| `src/Beneficios.Infrastructure/Scripts/11_Create_Funcionarios_Tenant.sql` | DDL referência |
| `src/Beneficios.Api/Controllers/FuncionariosController.cs` | `api/funcionarios` |

### Backend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `ModulosSistemaCatalog.cs` | + `funcionarios` |
| `TenantSchemaSql.cs` | `CriarTabelaFuncionarios` + `CriarTabelaFuncionarioBeneficios` |
| `TenantProvisioner.cs` | DDL no provision + em `GarantirPerfisNoSchemaAsync` (ou método dedicado chamado no loop/startup) |
| `Program.cs` | DI repository/service |
| `PostgresFixture.cs` | DDL funcionários no setup de testes |
| Testes Domain/Application/Infrastructure/Api | Novos + catálogo módulos |

### Frontend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/api/funcionario.models.ts` / `funcionario.service.ts` | HTTP |
| `features/funcionarios/funcionario-list/*` | Listagem + filtros |
| `features/funcionarios/funcionario-form/*` | Formulário seccionado |

### Frontend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/auth/modulos-sistema.ts` | + `funcionarios` |
| `app.routes.ts` | Rotas + guards |
| `e2e/fixtures/api-mocks.ts` | Permissão + mock list |
| `e2e/tenant-flows.spec.ts` | Smoke Funcionários |

---

## Task breakdown

### Task 1: Enums, CPF util e catálogo de jornada

**Files:**
- Create: `src/Beneficios.Domain/Enums/TipoContrato.cs`
- Create: `src/Beneficios.Domain/Enums/SituacaoFuncionario.cs`
- Create: `src/Beneficios.Domain/Enums/JornadaTrabalho.cs`
- Create: `src/Beneficios.Domain/JornadaTrabalhoCatalog.cs`
- Create: `src/Beneficios.Domain/CpfUtil.cs`
- Test: `tests/Beneficios.Tests/Domain/CpfUtilTests.cs`
- Test: `tests/Beneficios.Tests/Domain/JornadaTrabalhoCatalogTests.cs`

**Interfaces:**
- Produces: `CpfUtil.Normalizar(string?) → string?`, `CpfUtil.EhValido(string cpfSomenteDigitos) → bool`
- Produces: `JornadaTrabalho` enum com valores alinhados aos códigos do banco (usar `[EnumMember]` / nomes Pascal + conversão string no repo, **ou** enum com nomes `QuarentaQuatroHorasClt` mapeados para `"44h_clt"` via helper — preferir helper `JornadaTrabalhoConversao` com strings literais do spec)
- Produces: `JornadaTrabalhoCatalog.ExigeDetalhe(string codigo) → bool` true só para `especial_categoria`
- Códigos exatos: `44h_clt`, `40h_seg_sex`, `36h`, `12x36`, `6x1`, `5x1`, `5x2`, `tempo_parcial`, `especial_categoria`

- [ ] **Step 1: Write failing CPF tests**

```csharp
[Fact]
public void EhValido_CpfValido_DeveRetornarTrue()
    => Assert.True(CpfUtil.EhValido("52998224725"));

[Fact]
public void EhValido_CpfInvalido_DeveRetornarFalse()
    => Assert.False(CpfUtil.EhValido("11111111111"));

[Fact]
public void Normalizar_RemoveMascara()
    => Assert.Equal("52998224725", CpfUtil.Normalizar("529.982.247-25"));
```

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~CpfUtil" -v q
```

Expected: FAIL (type missing)

- [ ] **Step 3: Implement `CpfUtil`, enums, `JornadaTrabalhoCatalog` (+ conversão string ↔ enum se usar enum)**

- [ ] **Step 4: Run catalog tests — PASS** (especial exige detalhe; labels não vazias)

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(domain): add funcionario enums, CPF util and jornada catalog"
```

---

### Task 2: Módulo `funcionarios` no catálogo de perfil

**Files:**
- Modify: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Modify: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Test: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: entrada `new("funcionarios", "Funcionários", "/funcionarios", AcaoPermissao.Todas)`
- Front: `codigo: 'funcionarios'`, `nomeExibicao: 'Funcionários'`, `rota: '/funcionarios'`, `icone: 'badge'`, acões todas

- [ ] **Step 1: Extend failing assertions** — `Assert.Contains("funcionarios", codigos)`

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Add module back + front (UTF-8 BOM)**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat: add funcionarios module to permission catalog"
```

---

### Task 3: DDL tenant + provisionamento + fixture

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/11_Create_Funcionarios_Tenant.sql`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs`
- Modify: `tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs` (e/ou helper de provision de schema de teste)
- Test: extend `TenantProvisionerTests` — após provisionar, tabelas `funcionarios` e `funcionario_beneficios` existem

**Interfaces:**
- Produces: `TenantSchemaSql.CriarTabelaFuncionarios(schemaName)` e `CriarTabelaFuncionarioBeneficios(schemaName)` com colunas exatamente como o spec §3.1–3.2
- Índice único parcial matrícula: `CREATE UNIQUE INDEX ... ON ... (matricula) WHERE matricula IS NOT NULL`
- Chamar DDL em `ProvisionarAsync` e dentro do fluxo que já migra tenants (`GarantirPerfisNoSchemaAsync` ou método irmão chamado no mesmo loop)

- [ ] **Step 1: Write failing assertion** em `TenantProvisionerTests`

```csharp
Assert.True(await TableExists(schemaName, "funcionarios"));
Assert.True(await TableExists(schemaName, "funcionario_beneficios"));
```

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement DDL + wire provisioner + fixture**

- [ ] **Step 4: Run provisioner tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(infra): add funcionarios tenant DDL and provision"
```

---

### Task 4: Models + `IFuncionarioRepository`

**Files:**
- Create: `src/Beneficios.Domain/Models/FuncionarioSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioFiltroParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioBeneficioSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioBeneficioQueryResult.cs`
- Create: `src/Beneficios.Domain/Interfaces/IFuncionarioRepository.cs`

**Interfaces:**
- Produces:

```csharp
public interface IFuncionarioRepository
{
    Task<Guid> SalvarAsync(FuncionarioSalvarParams parametros, IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios);
    Task AtualizarAsync(FuncionarioAtualizarParams parametros, IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios);
    Task<FuncionarioQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioBeneficioQueryResult>> ObterBeneficiosAsync(Guid funcionarioId);
    Task<IReadOnlyList<FuncionarioQueryResult>> FiltrarAsync(FuncionarioFiltroParams filtro);
    Task<bool> CpfExisteAsync(string cpf, Guid? excetoId = null);
    Task<bool> MatriculaExisteAsync(string matricula, Guid? excetoId = null);
}
```

- Benefícios no save: dentro da mesma conexão/transação quando possível — delete-by-codigo ausentes + upsert `vale_transporte` (v1: lista 0..1)

- [ ] **Step 1: Create models + interface** (UTF-8 BOM)

- [ ] **Step 2: Build Domain — PASS**

```bash
dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q
```

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(domain): add funcionario persistence contracts"
```

---

### Task 5: `FuncionarioRepository` + testes de infra

**Files:**
- Create: `src/Beneficios.Infrastructure/Repositories/FuncionarioRepository.cs`
- Create: helpers de conversão enum↔banco se necessário (ex. `FuncionarioConversao.cs` no Domain ou Infrastructure)
- Test: `tests/Beneficios.Tests/Infrastructure/FuncionarioRepositoryTests.cs`

**Interfaces:**
- Consome: `IFuncionarioRepository`, schemas via `ITenantSchemaAccessor` / `search_path` como `UsuarioRepository`
- DateOnly ↔ `DateTime`/`Npgsql` igual padrão `CalendarioDiaRepository`

- [ ] **Step 1: Write failing repo tests** (salvar, obter, filtrar, CPF duplicado no banco)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement repository** (transação no Salvar/Atualizar incluindo benefícios)

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(infra): add FuncionarioRepository"
```

---

### Task 6: DTOs, AutoMapper, `FuncionarioService`

**Files:**
- Create: `src/Beneficios.Application/DTOs/FuncionarioDto.cs` (list/detail)
- Create: `src/Beneficios.Application/DTOs/FuncionarioSalvarDto.cs` / `FuncionarioAtualizarDto.cs`
- Create: `src/Beneficios.Application/DTOs/FuncionarioBeneficioDto.cs`
- Create: `src/Beneficios.Application/DTOs/FuncionarioFiltroDto.cs`
- Create: `src/Beneficios.Application/Interfaces/IFuncionarioService.cs`
- Create: `src/Beneficios.Application/Services/FuncionarioService.cs`
- Create: `src/Beneficios.Application/Mappings/FuncionarioProfile.cs`
- Test: `tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs`

**Interfaces:**
- Produces:

```csharp
public interface IFuncionarioService
{
    Task<Guid> SalvarAsync(FuncionarioSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, FuncionarioAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<FuncionarioDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioDto>> FiltrarAsync(FuncionarioFiltroDto filtro);
}
```

- Constantes de mensagem exatamente como o spec:
  - `CPF inválido.`
  - `CPF já cadastrado.`
  - `Matrícula já cadastrada.`
  - `Funcionário não encontrado.`
  - `Informe a data de desligamento.`
  - `Informe o detalhe da jornada especial.`
- Filtrar benefícios no save: rejeitar `codigo_beneficio` ≠ `vale_transporte` na v1 com `InvalidOperationException` (`Benefício não suportado na v1.`) **ou** ignorar silenciosamente — **preferir rejeitar** para falhar alto
- Normalizar CPF via `CpfUtil` antes de persistir; CEP: somente dígitos

- [ ] **Step 1: Write failing service tests** (CPF inválido, desligado sem data, especial sem detalhe, VT sync)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement service + DTOs + profile**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(app): add FuncionarioService with validation and VT sync"
```

---

### Task 7: `FuncionariosController` + DI

**Files:**
- Create: `src/Beneficios.Api/Controllers/FuncionariosController.cs`
- Modify: `src/Beneficios.Api/Program.cs` — registrar `IFuncionarioRepository` / `IFuncionarioService`
- Test: `tests/Beneficios.Tests/Api/FuncionariosControllerTests.cs`

**Interfaces:**
- Route: `api/funcionarios`
- `CodigoMenu = "funcionarios"`
- GET `/` filtrar query; GET `/{id}`; POST `/`; PUT `/{id}`
- Sem DELETE
- Bypass admin tenant igual outros controllers; `GarantirPermissaoAsync` no tenant
- 403 / 400 / 404 mapeados das exceptions do service

- [ ] **Step 1: Write failing controller tests** (403, create ok, no delete action)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement controller + DI**

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(api): add funcionarios endpoints with permission checks"
```

---

### Task 8: Front — módulo já feito na Task 2; API client + rotas

**Files:**
- Create: `Beneficios.Front/src/app/core/api/funcionario.models.ts`
- Create: `Beneficios.Front/src/app/core/api/funcionario.service.ts`
- Modify: `Beneficios.Front/src/app/app.routes.ts`

**Interfaces:**
- Service: `filtrar`, `obterPorId`, `criar`, `atualizar` → `${environment.apiUrl}/funcionarios`
- Models alinhados aos DTOs (camelCase JSON)
- Rotas lazy: `funcionarios`, `funcionarios/novo`, `funcionarios/:id/editar` com `tenantGuard` + `permissaoGuard('funcionarios')`
- Expor const de jornadas e tipos no models (labels PT)

- [ ] **Step 1: Add models + service + routes** (UTF-8 BOM). Criar shells mínimos dos componentes list/form (selector + template `<!-- Task 9 -->`) para o `ng build` passar.

- [ ] **Step 2: `npx ng build` — Expected: SUCCESS**

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add funcionario API client and routes"
```

---

### Task 9: Front — listagem + formulário

**Files:**
- Create: `features/funcionarios/funcionario-list/funcionario-list.component.{ts,html,scss}`
- Create: `features/funcionarios/funcionario-form/funcionario-form.component.{ts,html,scss}`

**UX (spec §6):**
- List: filtros nome/CPF/matrícula/situação; Novo se `criar`; Editar se `editar`; **sem** Excluir
- Form sections: Identificação, Contrato, Localização (2 blocos + CEP mask), Situação, Benefícios (VT card), Jornada (+ detalhe condicional)
- Máscaras: CPF, CEP `00000-000`, salário BRL (usar padrão já existente no projeto se houver; senão mask simples / `currency`)
- Validadores: desligado→data; especial→detalhe
- Dialog confirmar salvar se padrão Usuários/Perfis

- [ ] **Step 1: Implement list + form** espelhando `usuario-list` / `usuario-form` / `perfil-form` para estrutura Material

- [ ] **Step 2: `npx ng build` — SUCCESS**

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add funcionarios list and form UI"
```

---

### Task 10: E2E smoke + verificação final

**Files:**
- Modify: `Beneficios.Front/e2e/fixtures/api-mocks.ts` — incluir `funcionarios` em `fullPermissoes` + `mockFuncionarioList`
- Modify: `Beneficios.Front/e2e/tenant-flows.spec.ts` — login → menu Funcionários → heading

- [ ] **Step 1: E2E**

```bash
cd Beneficios.Front && npx playwright test e2e/tenant-flows.spec.ts -g "Funcionários"
```

Expected: PASS

- [ ] **Step 2: Backend**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj
```

Expected: PASS

- [ ] **Step 3: Front**

```bash
cd Beneficios.Front && npx ng build
```

Expected: SUCCESS

- [ ] **Step 4: Commit**

```bash
git commit -m "test: cover funcionarios flows and finalize feature verification"
```

---

## Spec coverage checklist

| Spec | Task |
|------|------|
| Enums / CPF / jornada catálogo | 1 |
| Módulo perfil `funcionarios` | 2 |
| DDL + provision + tenants existentes | 3 |
| Contratos persistência | 4 |
| Repository | 5 |
| Service + validação + VT | 6 |
| API + DI | 7 |
| Front client/rotas | 8 |
| UI list/form | 9 |
| E2E + verificação | 10 |
| Fora da v1 (motor, ViaCEP, delete…) | não implementar |

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-14-funcionarios.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — execute tasks in this session with `executing-plans` checkpoints  

Which approach?
