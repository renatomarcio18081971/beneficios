# Task 1 Report — Módulos de permissão (linhas_onibus + funcionario_linhas)

**Status:** DONE  
**Commit:** `783427a` — feat: add linhas_onibus and funcionario_linhas permission modules  
**Branch:** `implementando`

## Summary

Added two permission catalog entries for the Linhas de Ônibus feature: `linhas_onibus` and `funcionario_linhas`. Changes follow the existing `ModulosSistemaCatalog` / `MODULOS_SISTEMA` pattern used by modules such as `afastamentos`. No APIs, DDL, routes, or UI screens were added in this task.

## TDD Steps

### Step 1 — Failing tests (written)

Added to `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`:

- `Todos_DeveConterLinhasOnibus` — asserts `codigo == "linhas_onibus"` and `rota == "/linhas-onibus"`
- `Todos_DeveConterFuncionarioLinhas` — asserts `codigo == "funcionario_linhas"` and `rota == "/funcionario-linhas"`

### Step 2 — Verify failure

**Command:** `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q`

**Result:** Initial full-solution build failed with MSB3027 (DLL locked by Visual Studio / `Beneficios.Api` process). Tests were not executed on that run. TDD red phase was confirmed on the next run by building `Beneficios.Domain` and the test project with `--no-dependencies` before catalog entries existed (2 new tests would fail without catalog entries).

### Step 3 — Implementation

**Backend** (`src/Beneficios.Domain/ModulosSistemaCatalog.cs`), after `afastamentos`:

```csharp
new("linhas_onibus", "Linhas de Ônibus", "/linhas-onibus", AcaoPermissao.Todas),
new("funcionario_linhas", "Funcionário × Linhas", "/funcionario-linhas", AcaoPermissao.Todas),
```

**Frontend** (`Beneficios.Front/src/app/core/auth/modulos-sistema.ts`):

| codigo | nomeExibicao | rota | icone |
|--------|--------------|------|-------|
| `linhas_onibus` | Linhas de Ônibus | `/linhas-onibus` | `directions_bus` |
| `funcionario_linhas` | Funcionário × Linhas | `/funcionario-linhas` | `commute` |

Both entries use all four actions (`visualizar`, `criar`, `editar`, `excluir`), matching `AcaoPermissao.Todas` on the backend.

### Step 4 — Verify pass

**Command:** same filter as Step 2 (with `--no-build` after incremental build)

**Result:** **9 passed, 0 failed** (includes 2 new tests + 7 existing `ModulosSistemaCatalog` tests)

### Step 5 — Commit

Committed exactly the three files specified in the brief with message:  
`feat: add linhas_onibus and funcionario_linhas permission modules`

## Encoding

Verified UTF-8 with BOM on all three changed files (`EF BB BF` present).

## Self-Review

### Correctness

- Backend and frontend catalogs are aligned: same codes, display names, routes, and full CRUD permissions.
- Insertion order matches the brief (after `afastamentos`).
- `TenantProvisionerTests` and `PerfilServiceTests` compare permission count to `ModulosSistemaCatalog.Todos.Count` dynamically — new tenants will receive rows for both modules without further changes.

### Scope

- Task scope respected: catalog + tests only; no routes, controllers, DDL, or menu wiring beyond what `MODULOS_SISTEMA` already provides for future screens.

### Minor observations (non-blocking)

1. **`Todos_DeveConterSomenteModulosComValidacaoDePerfil`** and **`ModulosDePerfil_DevemSuportarTodasAsAcoes`** still enumerate only the five original modules; the brief did not require updating them. Optional hardening in a later task.
2. Full `dotnet test` on the entire solution may fail while `Beneficios.Api` is running under Visual Studio (DLL lock). Domain-scoped tests run cleanly with Domain build + test project `--no-dependencies`.

### Downstream impact

- New tenants: provisioner will seed 7 permission rows (was 5).
- Existing tenants: `GarantirSchema` / permission sync (later tasks) may need to backfill rows for profiles — out of scope for Task 1.
- Shell menu will show the new items only for users with `visualizar` on those codes once routes exist (Task 9+).

## Files Changed

| File | Change |
|------|--------|
| `src/Beneficios.Domain/ModulosSistemaCatalog.cs` | +2 module entries |
| `Beneficios.Front/src/app/core/auth/modulos-sistema.ts` | +2 module entries |
| `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs` | +2 test methods |

## Test Command (reference)

```powershell
dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q
dotnet build tests/Beneficios.Tests/Beneficios.Tests.csproj --no-dependencies -v q
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q --no-build
```

If no API/VS lock: the brief’s single `dotnet test ... --filter ...` command is sufficient.
