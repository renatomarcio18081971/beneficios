# Task 6 Report — Repository + Service + API (Funcionário × Linhas)

**Status:** DONE  
**Commit:** `3963f0f` — feat: add funcionario-linhas service, repository, and API  
**Branch:** `implementando`

## Summary

Implemented Dapper `FuncionarioLinhaRepository` (all interface methods: CRUD filter, `ExisteParAsync`, `ExisteVinculoAbertoPorLinhaAsync`, `EncerrarAbertosPorFuncionarioAsync`), `FuncionarioLinhaService` with VT gate (`ObterBeneficiosAsync` + `vale_transporte` ativo), linha vigente via `AfastamentoPeriodo.EstaAtivoEm`, unicidade, quantidade ≥ 1, DTOs + AutoMapper profile, and `FuncionarioLinhasController` (`api/funcionario-linhas`, `CodigoMenu = "funcionario_linhas"`, no DELETE). DI: `IFuncionarioLinhaRepository` → `FuncionarioLinhaRepository` and `IFuncionarioLinhaService` → `FuncionarioLinhaService` in `Program.cs` (unblocks `LinhaOnibusService`). TDD: failing tests first, then green (24 tests).

## Steps

### Step 1 — Failing tests

Created `FuncionarioLinhaServiceTests` (VT off/missing, linha vencida, duplicado, quantidade 0, data fim, not found, happy path, atualizar, filtrar vigentes) and `FuncionarioLinhasControllerTests` (403/404/400/Created/NoContent).

### Step 2 — RED

`dotnet test --filter FullyQualifiedName~FuncionarioLinha` → compile fail (types missing). Confirmed RED.

### Step 3 — Implement

| File | Role |
|------|------|
| `FuncionarioLinhaDtos.cs` | DTO records |
| `IFuncionarioLinhaService.cs` | Service contract |
| `FuncionarioLinhaService.cs` | VT / vigência / unicidade / quantidade |
| `FuncionarioLinhaProfile.cs` | AutoMapper |
| `FuncionarioLinhaRepository.cs` | Dapper + joins + EncerrarAbertos |
| `FuncionarioLinhasController.cs` | GET/POST/PUT, permissions |
| `Program.cs` | DI repo + service |

UTF-8 BOM verified on all new files.

### Step 4 — GREEN

`dotnet test --filter FullyQualifiedName~FuncionarioLinha` → **24/24 PASS**

### Step 5 — Commit

`feat: add funcionario-linhas service, repository, and API` (brief file set)

## Self-Review

- No DELETE endpoint; mirrors linhas-onibus / afastamentos permission pattern.
- `AtualizarAsync` loads vínculo for `FuncionarioId` (immutable); `ExisteParAsync(..., excetoId)`.
- `EncerrarAbertosPorFuncionarioAsync` ready for Task 7 (not wired into `FuncionarioService` yet).
- Critical handoff from Task 5: `IFuncionarioLinhaRepository` now registered in DI.
