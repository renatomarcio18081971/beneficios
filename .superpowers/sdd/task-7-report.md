# Task 7 Report — Desativar VT encerra vínculos abertos

**Status:** DONE  
**Commit:** `184fe3b` — feat: close open bus-line links when VT is deactivated  
**Branch:** `implementando`

## Summary

Wired auto-encerramento of open Funcionário×Linha vínculos when VT is inactive or absent after `FuncionarioService.SalvarAsync` / `AtualizarAsync`. Preferential path: `IFuncionarioLinhaService.EncerrarAbertosPorFuncionarioAsync` (delegates to repository with `DateOnly`/`DateTime.UtcNow`), injected into `FuncionarioService` to avoid Application→repo coupling across aggregates. TDD: failing tests first (compile RED), then green.

## Steps

### Step 1 — Failing tests

Added `AtualizarAsync_VtInativo_DeveEncerrarVinculosAbertos` and `AtualizarAsync_VtAtivo_NaoDeveEncerrarVinculos`; injected `Mock<IFuncionarioLinhaService>` into `FuncionarioServiceTests` constructor.

### Step 2 — RED

`dotnet test --filter FullyQualifiedName~FuncionarioServiceTests` → compile fail (`CS1729` 4-arg ctor missing; `CS1061` `EncerrarAbertosPorFuncionarioAsync` missing on interface). Confirmed RED.

### Step 3 — Implement

| File | Change |
|------|--------|
| `IFuncionarioLinhaService.cs` | `EncerrarAbertosPorFuncionarioAsync(Guid, Guid?)` |
| `FuncionarioLinhaService.cs` | Delegates to repo with UTC date/time |
| `FuncionarioService.cs` | Inject service; after persist, if normalized benefícios lack active VT → encerrar |
| `FuncionarioServiceTests.cs` | VT inactive/active verify call / never |

UTF-8 BOM verified on all four files.

### Step 4 — GREEN

`dotnet test --filter "FullyQualifiedName~FuncionarioServiceTests|FullyQualifiedName~FuncionarioLinha"` → **39/39 PASS**

### Step 5 — Commit

`feat: close open bus-line links when VT is deactivated` (brief file set)

## Self-Review

- Empty/`null` benefícios after normalize ⇒ VT absent ⇒ encerrar (matches brief).
- Active VT ⇒ no call (negative test).
- DI already had both services scoped; constructor order: repo, afastamento, linha service, mapper.
- Concern: `AtualizarAsync` with `Beneficios: null` now closes open links (idempotent SQL); callers that omit benefícios unintentionally will encerrar.
