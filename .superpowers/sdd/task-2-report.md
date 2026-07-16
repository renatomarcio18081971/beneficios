# Task 2 Report — Domain models e interfaces (Linha de ônibus)

**Status:** DONE  
**Commit:** `16059e1` — feat(domain): add linha onibus persistence contracts  
**Branch:** `implementando`

## Summary

Added Domain persistence contracts for the Linhas de Ônibus catalog: four model types (`SalvarParams`, `AtualizarParams`, `QueryResult`, `FiltroParams`) and `ILinhaOnibusRepository`. Matches the brief signatures and follows existing `FuncionarioAfastamento*` patterns (`sealed` classes, same namespace layout). No repository implementation, Application service, or API in this task.

## Steps

### Step 1 — Create files (UTF-8 BOM)

| File | Purpose |
|------|---------|
| `src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs` | Insert contract |
| `src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs` | Update contract |
| `src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs` | Read/query projection |
| `src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs` | List filter (`Descricao`, `SomenteVigentes`, `Referencia`) |
| `src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs` | `SalvarAsync`, `AtualizarAsync`, `ObterPorIdAsync`, `FiltrarAsync` |

All five files verified with BOM prefix `EF BB BF`.

### Step 2 — Build Domain

**Command:** `dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q`

**Result:** **PASS** — 0 warnings, 0 errors

Smoke compile only (brief allows build instead of dedicated compile test; no `LinhaOnibusModelsCompileTests.cs` added).

### Step 3 — Commit

Committed exactly the five files specified in the brief with message:  
`feat(domain): add linha onibus persistence contracts`

## Self-Review

### Correctness

- Property names, types, and repository method signatures match the brief verbatim.
- `LinhaOnibusFiltroParams.Referencia` comment documents service-layer default (today when `SomenteVigentes` is set) — aligned with design spec vigência rules.
- `LinhaOnibusQueryResult` exposes audit fields (`DataInclusao`, `DataAlteracao`) consistent with `linhas_onibus` DDL in design spec.
- `ILinhaOnibusRepository` mirrors `IFuncionarioAfastamentoRepository` subset (no delete — design reserves delete for profile only, encerramento via `data_fim`).

### Conventions

- Used `sealed class` and `string.Empty` defaults per brief; consistent with newer Domain models.
- Interface uses `using Beneficios.Domain.Models;` and lives under `Beneficios.Domain.Interfaces` like sibling repositories.

### Scope

- Task scope respected: Domain contracts only; no Infrastructure, Application, API, DDL, or tests beyond build smoke.

### Minor observations (non-blocking)

1. Brief listed optional compile test file — skipped in favor of Domain build smoke (explicitly allowed).
2. `Referencia` default-to-today logic belongs in Application service (Task 4+) — not in Domain params, as intended.

### Downstream impact

- Task 3 (DDL) and Task 4 (Infrastructure repository) can implement against these contracts directly.
- Task 5+ Application service will map DTOs ↔ these params/results.

## Files Changed

| File | Change |
|------|--------|
| `src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs` | New |
| `src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs` | New |
| `src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs` | New |
| `src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs` | New |
| `src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs` | New |

## Test Command (reference)

```powershell
dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q
```
