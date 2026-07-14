# Perfil de Acesso (tenant) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar cadastro de perfil de acesso por empresa (schema tenant), com matriz Visualizar/Criar/Editar/Excluir por módulo, vínculo no usuário, provisionamento do dono, e enforcement no front + API.

**Architecture:** Catálogo de módulos no Domain (espelhado no Front). Tabelas `perfis` + `perfil_permissoes` + `usuarios.perfil_id` por tenant. Login carrega permissões na sessão; API revalida via `IPermissaoService.EnsureAsync`. Menu e botões ocultam sem permissão (sem placeholders desabilitados). Enum `UsuarioPerfil` (Admin/Empresa) permanece inalterado.

**Tech Stack:** .NET 9, Dapper, PostgreSQL schemas, xUnit/Moq; Angular 22 standalone + Material; UTF-8 com BOM em todo arquivo novo/alterado.

**Spec:** `docs/superpowers/specs/2026-07-14-perfil-acesso-design.md`

## Global Constraints

- Encoding: **UTF-8 com BOM** em todo arquivo criado ou alterado (back, front, tests, scripts, docs)
- Escopo: **somente tenant**; subdomain `admin` fora da matriz na v1
- Não confundir enum `UsuarioPerfil` com entidade `Perfil` (acesso)
- Login sem `perfil_id`: mensagem exata `Usuário sem perfil configurado, procure o administrador do sistema !`
- Ocultar UI sem permissão (`@if`); layout sem gaps/tortos
- Export PDF/Excel só no front; conta como Visualizar
- Backend: padrões `SalvarAsync`, `*Params`, `*Dto`, AutoMapper `*Profile`, controllers enxutos
- Cobertura backend ≥ 80% (`scripts/check-coverage.ps1`)
- Extensibilidade: novo módulo = entrada no catálogo + feature + `EnsureAsync` — sem alterar shape de `perfil_permissoes`

---

## File Map

### Backend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Domain/Enums/AcaoPermissao.cs` | Visualizar, Criar, Editar, Excluir |
| `src/Beneficios.Domain/Models/ModuloSistema.cs` | Record do catálogo |
| `src/Beneficios.Domain/ModulosSistemaCatalog.cs` | Lista estática dashboard/usuarios/perfis |
| `src/Beneficios.Domain/Entities/Perfil.cs` | Entidade perfil de acesso |
| `src/Beneficios.Domain/Entities/PerfilPermissao.cs` | Linha da matriz |
| `src/Beneficios.Domain/Models/Perfil*Params.cs` / `PerfilQueryResult.cs` | Params repositório |
| `src/Beneficios.Domain/Interfaces/IPerfilRepository.cs` | Contrato CRUD + permissões |
| `src/Beneficios.Domain/Interfaces/IPermissaoService.cs` | `EnsureAsync` / `UsuarioPossuiAsync` (Domain ou Application — preferir Application Interfaces) |
| `src/Beneficios.Application/DTOs/Perfil*.cs` | DTOs CRUD + `PermissaoDto` |
| `src/Beneficios.Application/Interfaces/IPerfilService.cs` | Serviço perfil |
| `src/Beneficios.Application/Interfaces/IPermissaoService.cs` | Enforcement |
| `src/Beneficios.Application/Services/PerfilService.cs` | Regras de negócio perfil |
| `src/Beneficios.Application/Services/PermissaoService.cs` | Checagem por usuário/menu/ação |
| `src/Beneficios.Application/Mappings/PerfilProfile.cs` | AutoMapper |
| `src/Beneficios.Infrastructure/Repositories/PerfilRepository.cs` | Dapper |
| `src/Beneficios.Api/Controllers/PerfisController.cs` | `api/perfis` |
| `src/Beneficios.Infrastructure/Scripts/08_Create_Table_Perfis.sql` | Script referência / migração catalog-style |
| `src/Beneficios.Infrastructure/Scripts/09_Migrate_Tenant_Perfis.sql` | Migração idempotente schemas existentes (ou helper C#) |

### Backend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `TenantSchemaSql.cs` | DDL perfis/permissoes; ALTER usuarios.perfil_id; seed dono |
| `TenantProvisioner.cs` | Ordem: tables → dono → default user com perfil_id |
| `Usuario.cs`, `Usuario*Params`, `UsuarioAuthResult`, `UsuarioQueryResult` | `PerfilId` |
| `UsuarioSalvarDto` / `UsuarioAtualizarDto` / `UsuarioDto` | `PerfilId` (+ nome perfil opcional) |
| `LoginResponseDto` | `PerfilId`, `PerfilNome`, `Permissoes` |
| `UsuarioRepository.cs` | Coluna `perfil_id`; join opcional |
| `UsuarioService.cs` | Login bloqueia sem perfil; valida perfilId no save |
| `UsuariosController.cs` | EnsureAsync por ação |
| `Program.cs` | DI Perfil + Permissao |
| Testes Api/Application/Infrastructure Usuario + TenantProvisioner | Novos cenários |

### Frontend — criar

| Arquivo | Responsabilidade |
|---------|------------------|
| `core/auth/modulos-sistema.ts` | Catálogo espelho |
| `core/auth/permission.service.ts` | `can(codigo, acao)` |
| `core/auth/permission.guard.ts` | Rota exige Visualizar |
| `core/api/perfil.models.ts` / `perfil.service.ts` | HTTP perfis |
| `features/perfis/perfil-list/*` | Listagem padrão Usuários |
| `features/perfis/perfil-form/*` | Nome + matriz |

### Frontend — modificar

| Arquivo | Responsabilidade |
|---------|------------------|
| `auth.models.ts` / `auth.service.ts` | Sessão com permissões; mensagem login |
| `usuario.models.ts` / form | `perfilId` obrigatório |
| `shell.component.ts` | Menu filtrado por visualizar |
| `usuario-list` | Ocultar Novo/Editar/Excluir via `can` |
| `app.routes.ts` | Rotas `/perfis` + guards |
| E2E tenant | Fluxos perfil |

---

## Task 1: Catálogo de módulos + `AcaoPermissao`

**Files:**
- Create: `src/Beneficios.Domain/Enums/AcaoPermissao.cs`
- Create: `src/Beneficios.Domain/Models/ModuloSistema.cs`
- Create: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Test: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: `AcaoPermissao` { Visualizar, Criar, Editar, Excluir }; `ModuloSistema(string Codigo, string NomeExibicao, string Rota, AcaoPermissao AcoesSuportadas)`; `ModulosSistemaCatalog.Todos` com códigos `dashboard`, `usuarios`, `perfis`

- [ ] **Step 1: Write the failing test**

```csharp
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class ModulosSistemaCatalogTests
{
    [Fact]
    public void Todos_DeveConterCodigosDaV1()
    {
        var codigos = ModulosSistemaCatalog.Todos.Select(m => m.Codigo).ToArray();
        Assert.Contains("dashboard", codigos);
        Assert.Contains("usuarios", codigos);
        Assert.Contains("perfis", codigos);
    }

    [Fact]
    public void Dashboard_DeveSuportarSomenteVisualizar()
    {
        var dashboard = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == "dashboard");
        Assert.Equal(AcaoPermissao.Visualizar, dashboard.AcoesSuportadas);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "ModulosSistemaCatalogTests" -v n`  
Expected: FAIL (tipos não existem)

- [ ] **Step 3: Implement**

`AcaoPermissao.cs`:

```csharp
namespace Beneficios.Domain.Enums;

[Flags]
public enum AcaoPermissao
{
    Nenhuma = 0,
    Visualizar = 1,
    Criar = 2,
    Editar = 4,
    Excluir = 8,
    Todas = Visualizar | Criar | Editar | Excluir
}
```

`ModuloSistema.cs`:

```csharp
using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public sealed record ModuloSistema(
    string Codigo,
    string NomeExibicao,
    string Rota,
    AcaoPermissao AcoesSuportadas);
```

`ModulosSistemaCatalog.cs`:

```csharp
using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;

namespace Beneficios.Domain;

public static class ModulosSistemaCatalog
{
    public static IReadOnlyList<ModuloSistema> Todos { get; } =
    [
        new("dashboard", "Dashboard", "/dashboard", AcaoPermissao.Visualizar),
        new("usuarios", "Usuários", "/usuarios", AcaoPermissao.Todas),
        new("perfis", "Perfis", "/perfis", AcaoPermissao.Todas),
    ];

    public static ModuloSistema? ObterPorCodigo(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
```

Salvar com **UTF-8 BOM**.

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/Enums/AcaoPermissao.cs src/Beneficios.Domain/Models/ModuloSistema.cs src/Beneficios.Domain/ModulosSistemaCatalog.cs tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
git commit -m "feat: add module catalog and permission actions for tenant RBAC"
```

---

## Task 2: DDL tenant + provisionamento do Dono

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/08_Create_Perfis_Tenant.sql` (documentação / replay manual)
- Modify: `tests/Beneficios.Tests/Infrastructure/TenantProvisionerTests.cs`

**Interfaces:**
- Consumes: `ModulosSistemaCatalog.Todos`
- Produces: tabelas `perfis`, `perfil_permissoes`; coluna `usuarios.perfil_id`; perfil Dono (`eh_sistema=true`, nome `"Dono"`) com todas flags true por código do catálogo; default user com `perfil_id` setado

- [ ] **Step 1: Extend failing test in TenantProvisionerTests**

Após provisionar, assert:

```csharp
var perfisTable = await fixture.Connection!.ExecuteScalarAsync<bool>(
    """
    SELECT EXISTS(
      SELECT 1 FROM information_schema.tables
      WHERE table_schema = @SchemaName AND table_name = 'perfis')
    """, new { SchemaName = schemaName });
Assert.True(perfisTable);

var dono = await fixture.Connection.QueryFirstOrDefaultAsync<(Guid Id, string Nome, bool EhSistema)>(
    $"""
    SELECT id AS Id, nome AS Nome, eh_sistema AS EhSistema
    FROM {quotedSchema}.perfis WHERE eh_sistema = TRUE LIMIT 1
    """);
Assert.NotNull(dono);
Assert.Equal("Dono", dono.Nome);

var permCount = await fixture.Connection.ExecuteScalarAsync<int>(
    $"""
    SELECT COUNT(*) FROM {quotedSchema}.perfil_permissoes
    WHERE perfil_id = @Id AND visualizar AND criar AND editar AND excluir
    """, new { dono.Id });
Assert.Equal(ModulosSistemaCatalog.Todos.Count, permCount);

var perfilIdUsuario = await fixture.Connection.ExecuteScalarAsync<Guid?>(
    $"""
    SELECT perfil_id FROM {quotedSchema}.usuarios WHERE email = @Email
    """, new { Email = TenantDefaultUser.Email });
Assert.Equal(dono.Id, perfilIdUsuario);
```

- [ ] **Step 2: Run test — expect FAIL**

- [ ] **Step 3: Implement SQL helpers in `TenantSchemaSql`**

Adicionar métodos (strings SQL) aproximadamente:

- `CreatePerfisTable(schemaName)` — colunas conforme spec §3.1  
- `CreatePerfilPermissoesTable(schemaName)` — UNIQUE (perfil_id, codigo_menu)  
- `AlterUsuariosAddPerfilId(schemaName)` — `ALTER TABLE ... ADD COLUMN IF NOT EXISTS perfil_id UUID NULL` + FK  
- `InsertPerfilDono(schemaName)` — INSERT perfil Dono retornando id via parâmetro `@PerfilId`  
- `InsertPermissoesDono(schemaName)` — INSERT uma linha por módulo (loop no C# com Execute, ou multi-values)  
- Ajustar `InsertDefaultUsuario` para incluir `perfil_id`

Ordem no `TenantProvisioner.ProvisionAsync`:

1. CreateSchema  
2. CreateUsuariosTable  
3. CreatePerfisTable  
4. CreatePerfilPermissoesTable  
5. AlterUsuariosAddPerfilId  
6. Insert Perfil Dono (`Guid perfilId = Guid.NewGuid()`)  
7. Insert permissões full para cada `ModulosSistemaCatalog.Todos`  
8. InsertDefaultUsuario com `PerfilId = perfilId`

- [ ] **Step 4: Run TenantProvisionerTests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat: provision tenant owner profile and permission tables"
```

---

## Task 3: Domain + Repository Perfil

**Files:**
- Create: entities `Perfil`, `PerfilPermissao`
- Create: `PerfilSalvarParams`, `PerfilAtualizarParams`, `PerfilFiltroParams`, `PerfilQueryResult`, `PerfilPermissaoParams`
- Create: `IPerfilRepository`
- Create: `PerfilRepository`
- Test: `tests/Beneficios.Tests/Infrastructure/PerfilRepositoryTests.cs`

**Interfaces:**
- Produces:
  - `Task SalvarAsync(PerfilSalvarParams)`  
  - `Task<bool> AtualizarAsync(PerfilAtualizarParams)`  
  - `Task<bool> DeleteAsync(Guid id)`  
  - `Task<PerfilQueryResult?> ObterUmAsync(Guid id)` (inclui lista permissões)  
  - `Task<PerfilQueryResult[]> ObterTodosAsync()` / `FiltrarAsync`  
  - `Task<bool> EstaEmUsoAsync(Guid id)`  
  - `Task<PerfilPermissaoParams[]?> ObterPermissoesPorUsuarioAsync(Guid usuarioId)`

- [ ] **Step 1: Write failing infrastructure test** (Postgres collection) — salvar perfil + 3 permissões, obter por id, filtrar por nome

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement entities, params, repository (Dapper, tabela sem schema prefix — SearchPath)**

Checklist SQL: snake_case + aliases PascalCase como em `UsuarioRepository`.

- [ ] **Step 4: Run — PASS**

- [ ] **Step 5: Commit** `feat: add perfil repository for tenant schemas`

---

## Task 4: Application + API Perfis CRUD

**Files:**
- Create: DTOs `PerfilDto`, `PerfilSalvarDto`, `PerfilAtualizarDto`, `PerfilFiltroDto`, `PermissaoMenuDto`
- Create: `IPerfilService`, `PerfilService`, `PerfilProfile` (AutoMapper)
- Create: `PerfisController`
- Modify: `Program.cs` — `AddScoped<IPerfilRepository,...>` + `IPerfilService` + AutoMapper assembly já cobre se mesmo assembly
- Test: `PerfilServiceTests`, `PerfisControllerTests`

**Interfaces:**
- `PerfilSalvarDto(string Nome, IReadOnlyList<PermissaoMenuDto> Permissoes)`
- `PermissaoMenuDto(string CodigoMenu, bool Visualizar, bool Criar, bool Editar, bool Excluir)`
- Regras: normalizar permissões contra catálogo (desconhecido ignora; faltante = false); não excluir `EhSistema`; não excluir se `EstaEmUsoAsync`

- [ ] **Step 1: Failing service tests** — salvar completa faltantes; DeleteAsync em perfil sistema retorna false/throws conforme padrão do projeto (preferir `false` ou exception de domínio — usar **InvalidOperationException** com mensagem PT e controller 400)

Padronizar: Service lança `InvalidOperationException`; Controller captura → `BadRequest(new { message })`.

- [ ] **Step 2–4:** Implement TDD service + controller espelhando `UsuariosController` (GET, filtrar, id, POST 201, PUT 204, DELETE 204/404/400)

- [ ] **Step 5: Commit** `feat: add perfis API and application service`

---

## Task 5: `IPermissaoService` + enforcement em controllers tenant

**Files:**
- Create: `IPermissaoService`, `PermissaoService`
- Modify: `UsuariosController`, `PerfisController`
- Test: `PermissaoServiceTests`, `UsuariosControllerTests` (403)

**Interfaces:**
- `Task EnsureAsync(Guid usuarioId, string codigoMenu, AcaoPermissao acao, CancellationToken ct = default)`  
  - Sem permissão → `UnauthorizedAccessException` (controller → 403)  
- `Task<bool> PossuiAsync(...)` para uso interno  

Mapeamento UsuariosController:

| Action | Ensure |
|--------|--------|
| Filtrar, GetAll, GetById | `usuarios`, Visualizar |
| Create | Criar |
| Update | Editar |
| Delete | Excluir |

PerfisController idem com código `perfis`.  
Obter `usuarioId` do JWT (`Sub` / `NameIdentifier`) como em Update atual.

Login / solicitar senha: **sem** Ensure (AllowAnonymous).

Admin EmpresasController: **não** alterar.

- [ ] **Step 1: Failing test** — `PossuiAsync` false → `EnsureAsync` throws

- [ ] **Step 2–4:** Implement + wire DI + controllers

- [ ] **Step 5: Commit** `feat: enforce menu permissions on tenant API controllers`

---

## Task 6: Login + vínculo `perfilId` em Usuário

**Files:**
- Modify: entity/params/DTOs usuário + `LoginResponseDto`
- Modify: `UsuarioRepository` (SELECT/INSERT/UPDATE `perfil_id`)
- Modify: `UsuarioService.LoginAsync`, `SalvarAsync`, `AtualizarAsync`
- Modify: AutoMapper `UsuarioProfile`
- Test: `UsuarioServiceTests`

**Login flow (tenant):**

```csharp
if (usuario.PerfilId is null)
    throw new InvalidOperationException(
        "Usuário sem perfil configurado, procure o administrador do sistema !");
// ou retornar null + controller mapear mensagem — preferir exception tipada LoginSemPerfilException
```

Preferência do plano: método retorna `(LoginResponseDto? dto, string? errorMessage)`. Se `errorMessage` preenchida, controller `Unauthorized(new { message = errorMessage })` com texto **exato** do spec.

Estender `LoginResponseDto`:

```csharp
public Guid? PerfilAcessoId { get; set; }  // evitar colisão com enum Perfil
public string PerfilAcessoNome { get; set; } = string.Empty;
public List<PermissaoMenuDto> Permissoes { get; set; } = [];
```

JSON: usar nomes `perfilAcessoId` / `perfilAcessoNome` / `permissoes` (documentar no front).  
Manter propriedade `Perfil` = enum Admin/Empresa.

`UsuarioSalvarDto` / `UsuarioAtualizarDto`: adicionar `Guid PerfilId` (obrigatório no service se tenant).

- [ ] **Step 1: Tests** — login sem perfilId falha com mensagem; login com dono retorna permissões count = catálogo; save exige perfil existente

- [ ] **Step 2–4:** Implement

- [ ] **Step 5: Commit** `feat: require access profile on login and user CRUD`

---

## Task 7: Migração de tenants já existentes

**Files:**
- Create: helper `TenantPerfisMigrator` **ou** script SQL aplicado via ferramenta existente
- Test: se houver fixture multi-schema, cobrir “schema com usuarios sem perfis → após migrate tem Dono + perfil_id no default user”

Comportamento idempotente:

1. Para cada schema `tenant_%` (exceto se já tem tabela perfis completa)  
2. CREATE TABLE IF NOT EXISTS perfis / perfil_permissoes  
3. ALTER usuarios ADD perfil_id  
4. Se não existe perfil eh_sistema: criar Dono + permissões  
5. UPDATE usuarios SET perfil_id = dono WHERE email = default e perfil_id IS NULL  

Expor como método `ITenantProvisioner.EnsurePerfisAsync(schema)` chamado opcionalmente no startup **ou** documentar execução manual do script `09_*.sql` nesta task. Preferir método C# testável + script SQL espelho.

- [ ] **Steps:** test → implement → PASS → commit `fix: migrate existing tenant schemas to perfis model`

---

## Task 8: Front — catálogo, sessão, permission helper

**Files:**
- Create: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Create: `Beneficios.Front/src/app/core/auth/permission.service.ts`
- Modify: `auth.models.ts`, `auth.service.ts` (persistir permissoes na session)
- Modify: login component — exibir `error.message` da API quando 401 com texto de sem perfil
- Test unitário (se houver padrão): `permission.service.spec.ts`

**Interfaces front:**

```typescript
export type AcaoPermissao = 'visualizar' | 'criar' | 'editar' | 'excluir';

export interface PermissaoMenu {
  codigoMenu: string;
  visualizar: boolean;
  criar: boolean;
  editar: boolean;
  excluir: boolean;
}

export interface UserSession {
  // ...campos existentes
  perfilAcessoId?: string | null;
  perfilAcessoNome?: string;
  permissoes: PermissaoMenu[];
}
```

`PermissionService.can(codigoMenu: string, acao: AcaoPermissao): boolean` — admin mode → true para Empresas apenas (não usa matriz); tenant usa `permissoes`.

Admin: `can` não se aplica aos itens admin (menu fixo). Tenant sem permissão → false.

- [ ] Commit: `feat(front): add permission catalog and session permissions`

---

## Task 9: Front — CRUD Perfis (list + form)

**Files:**
- Create: `core/api/perfil.models.ts`, `perfil.service.ts`
- Create: `features/perfis/perfil-list/*`, `perfil-form/*`
- Modify: `app.routes.ts` — `/perfis`, `/perfis/novo`, `/perfis/:id/editar` com `tenantGuard` + `permissionGuard('perfis')`

Espelho fiel de `usuario-list` / `usuario-form`:

- List: filtro nome, limpar, export `perfis-{subdomain}-{date}`, mat-table + mobile cards, delete com confirm (bloquear se `ehSistema`)
- Form: nome + matriz de checkboxes a partir de `MODULOS_SISTEMA`; ocultar ações não suportadas; salvar payload completo

UTF-8 BOM em todos os arquivos com acentuação.

- [ ] Commit: `feat(front): add perfis list and form screens`

---

## Task 10: Front — Usuário select + menu + ocultação de ações

**Files:**
- Modify: `usuario.models.ts`, `usuario-form` (select Perfil obrigatório; carregar `perfilService.list()`)
- Modify: `shell.component.ts` — `navItems` tenant = catálogo filtrado por `permission.can(codigo, 'visualizar')`; incluir Perfis
- Modify: `usuario-list` — `@if (can('usuarios','criar'))` no Novo; coluna ações só com botões permitidos; se zero ações, omitir coluna
- Modify: `perfil-list` — mesma regra com código `perfis`
- Create: `permission.guard.ts` e aplicar nas rotas

Regra de layout: usar `@if` (não `hidden`/`disabled`); flex do header com `gap` só entre filhos existentes.

- [ ] Commit: `feat(front): wire profile select, dynamic menu, and action visibility`

---

## Task 11: E2E + verificação final

**Files:**
- Modify: `Beneficios.Front/e2e/tenant-flows.spec.ts` (+ mocks em `api-mocks.ts` se necessário)
- Run backend tests + coverage script
- Run front build / e2e relevantes

Checklist:

- [ ] Empresa nova: default user loga com Dashboard, Usuários, Perfis e ações full  
- [ ] Usuário sem perfil: API/front mostram mensagem fixa  
- [ ] 403 ao chamar POST /usuarios sem permissão Criar  
- [ ] Menu não mostra item sem Visualizar  
- [ ] Arquivos novos com BOM (spot-check PowerShell: bytes EF BB BF)  
- [ ] `scripts/check-coverage.ps1` ≥ 80%  

- [ ] Commit: `test: cover tenant profile access flows`

---

## Self-review (plan vs spec)

| Spec § | Task |
|--------|------|
| Catálogo extensível | Task 1, 8 |
| Tabelas + Dono no provision | Task 2 |
| CRUD Perfil API | Tasks 3–4 |
| Ensure API + front | Tasks 5, 8–10 |
| Login mensagem + permissões payload | Task 6, 8 |
| Select usuário + tela perfis + menu | Tasks 9–10 |
| Migração schemas antigos | Task 7 |
| UTF-8 BOM | Global + checklist Task 11 |
| Admin fora | Tasks 5, 10 |
| Fora v1 (JWT matrix, menu table) | Não incluído |

Type names: `PerfilAcessoId` no login DTO evita colisão com `Perfil` enum; front usa `perfilAcessoId` / `permissoes`.

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-14-perfil-acesso.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — execute tasks in this session with `executing-plans`, checkpoints for review  

Which approach?
