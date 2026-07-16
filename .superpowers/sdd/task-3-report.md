# Task 3 Report — Domain models e interfaces (Funcionário × Linha)

**Status:** DONE  
**Commit:** `d3deb3a` — feat(domain): add funcionario-linha persistence contracts  
**Branch:** `implementando`

## Summary

Added Domain persistence contracts for Funcionário × Linha vínculos: four model types (`SalvarParams`, `AtualizarParams`, `QueryResult`, `FiltroParams`) and `IFuncionarioLinhaRepository` with seven methods including unicidade (`ExisteParAsync`), bloqueio de encerramento de linha (`ExisteVinculoAbertoPorLinhaAsync`), and auto-encerramento ao desativar VT (`EncerrarAbertosPorFuncionarioAsync`). Matches brief signatures verbatim; follows Task 2 `LinhaOnibus*` conventions (`sealed` classes). No repository implementation in this task.

## Steps

### Step 1 — Create files (UTF-8 BOM)

| File | Purpose |
|------|---------|
| `src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs` | Insert vínculo |
| `src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs` | Update vínculo |
| `src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs` | Read projection (+ `FuncionarioNome`, `LinhaDescricao`) |
| `src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs` | List filter (`FuncionarioId`, `SomenteVigentes`, `Referencia`) |
| `src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs` | Full persistence contract |

All five files verified with BOM prefix `EF BB BF`.

### Step 2 — Build Domain

**Command:** `dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q`

**Result:** **PASS** — 0 warnings, 0 errors

### Step 3 — Commit

Committed exactly the five files specified in the brief with message:  
`feat(domain): add funcionario-linha persistence contracts`

## Self-Review

### Correctness

- Property names, types, and all seven repository method signatures match the brief verbatim.
- `FuncionarioLinhaAtualizarParams` omits `FuncionarioId` (immutable after create) per brief.
- `FuncionarioLinhaQueryResult` includes joined names for list/detail UI.
- Extra repo methods support downstream Tasks 5–7 (VT gate, linha encerramento block, VT desativar).

### Conventions

- Used `sealed class` and `string.Empty` defaults consistent with Task 2 `LinhaOnibus*`.
- Interface uses `using Beneficios.Domain.Models;` under `Beneficios.Domain.Interfaces`.

### Scope

- Domain contracts only; no Infrastructure, Application, API, DDL, or tests beyond build smoke.

### Minor observations (non-blocking)

1. `Referencia` default-to-today logic belongs in Application service — not in Domain params.
2. `EncerrarAbertosPorFuncionarioAsync` caller (FuncionarioService save VT) deferred to later task per plan.

### Downstream impact

- Task 4 (DDL) can create `funcionario_linhas` table aligned with params/results.
- Task 6 (Infrastructure) implements `FuncionarioLinhaRepository` against this interface.
- Task 5 `LinhaOnibusService` can mock `ExisteVinculoAbertoPorLinhaAsync` until repo exists.

## Files Changed

| File | Change |
|------|--------|
| `src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs` | New |
| `src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs` | New |
| `src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs` | New |
| `src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs` | New |
| `src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs` | New |

## Test Command (reference)

```powershell
dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q
```
