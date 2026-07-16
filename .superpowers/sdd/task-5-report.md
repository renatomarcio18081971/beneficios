# Task 5 Report — Repository + Service + API (Linhas de ônibus)

**Status:** DONE  
**Commit:** `e4afaf3` — feat: add linhas-onibus service, repository, and API  
**Branch:** `implementando`

## Summary

Implemented Dapper `LinhaOnibusRepository` (tenant `search_path` via shared `IDbConnection`), `LinhaOnibusService` with date/tarifa validations and block-on-close via mocked `IFuncionarioLinhaRepository.ExisteVinculoAbertoPorLinhaAsync`, DTOs + AutoMapper profile, and `LinhasOnibusController` (`api/linhas-onibus`, `CodigoMenu = "linhas_onibus"`, no DELETE). DI wired in `Program.cs`. TDD: failing service tests first, then green (18 tests).

## Steps

### Step 1 — Failing service tests

Created `LinhaOnibusServiceTests` covering data fim inválida, tarifa negativa, encerrar com vínculo aberto, not found, happy paths, and `somenteVigentes` filter.

### Step 2 — RED

`dotnet test --filter FullyQualifiedName~LinhaOnibusService` → compile fail (`LinhaOnibusService` missing). Confirmed RED.

### Step 3 — Implement

| File | Role |
|------|------|
| `LinhaOnibusDtos.cs` | DTO records |
| `ILinhaOnibusService.cs` | Service contract |
| `LinhaOnibusService.cs` | Validations + vigência filter via `AfastamentoPeriodo.EstaAtivoEm` |
| `LinhaOnibusProfile.cs` | AutoMapper |
| `LinhaOnibusRepository.cs` | Dapper CRUD + ILIKE filter |
| `LinhasOnibusController.cs` | GET/POST/PUT, permissions |
| `Program.cs` | DI for repo + service |
| `LinhasOnibusControllerTests.cs` | Permission/status codes |

UTF-8 BOM verified on all new files.

### Step 4 — GREEN

`dotnet test --filter "LinhaOnibusService|LinhasOnibusController"` → **18/18 PASS**

### Step 5 — Commit

`feat: add linhas-onibus service, repository, and API` (brief file set)

## Self-Review

- No DELETE endpoint; mirrors Afastamentos permission pattern.
- Controller maps feminine `"não encontrada"` via prefix `"não encontrad"`.
- Service tests mock `IFuncionarioLinhaRepository` (Task 6 implements real repo).
- **Concern:** `IFuncionarioLinhaRepository` not registered in DI until Task 6 — resolving `LinhaOnibusService` at runtime fails until then. Unit tests unaffected.
- Stopped locked `Beneficios.Api` process to unblock build (DLL lock).
