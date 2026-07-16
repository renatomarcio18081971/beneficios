# Final fix report — linhas-onibus Important findings

**Date:** 2026-07-16  
**Branch:** implementando  
**Scope:** Whole-branch Important findings from final review

## Important 1 — Linha dropdown vs dataInicio

**Problem:** Form loaded lines with `somenteVigentes=true` using “today” only; vínculos with past/future `dataInicio` could miss/show wrong lines.

**Fix:**
- API: `ILinhaOnibusService.FiltrarAsync` / `LinhasOnibusController.Filtrar` accept optional `DateOnly? referencia`; when `somenteVigentes=true`, filter uses `referencia ?? today`.
- Front: `LinhaOnibusFiltroRequest.referencia`; form reloads lines on `dataInicio` changes (and init via `startWith`).
- Clear selected line if no longer in filtered list; still inject current line when editing.

**Tests:** `LinhaOnibusServiceTests.FiltrarAsync_SomenteVigentesComReferencia_DeveUsarDataInformada`; controller setups updated for 3-arg `Filtrar`.

## Important 2 — Encerrar vínculo without active VT

**Problem:** `AtualizarAsync` always called `GarantirFuncionarioComVtAsync`, blocking close when VT already inactive.

**Fix:** Require VT only if vínculo remains open after save (`dataFim` null or `dataFim >= today`). Front mirrors rule in `submitDesabilitado`.

**Tests:** `AtualizarAsync_EncerrarSemVt_DevePermitir`, `AtualizarAsync_ManterAbertoSemVt_DeveFalhar`.

## Minors

- `ModulosSistemaCatalogTests` enumerations include `linhas_onibus` + `funcionario_linhas`.
- VT check uses `switchMap` on `funcionarioId` valueChanges.

## Verification

```
dotnet test --filter "LinhaOnibusServiceTests|FuncionarioLinhaServiceTests|LinhasOnibusControllerTests|ModulosSistemaCatalogTests"
→ Passed

npx ng build (Beneficios.Front)
→ Success
```

UTF-8 BOM (`EF BB BF`) applied on all touched source/test/front files.

## Commit

`ca1bf80` — fix: align linha vigencia and VT gate on close
