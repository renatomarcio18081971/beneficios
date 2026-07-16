# Task 9 Report — Frontend — Funcionário × Linhas + atalho

**Status:** DONE  
**Commit:** `ef6bac0` — feat(front): add funcionario-linhas UI and funcionario shortcuts  
**Branch:** `implementando`

## Summary

Added Angular standalone cadastro for Funcionário × Linhas (list + form), mirroring afastamentos Material patterns: reactive forms, filters, export Excel/PDF, Novo/Editar, **no Excluir**. Routes use `tenantGuard` + `permissaoGuard('funcionario_linhas')`. Query `?funcionarioId=` pré-filtra a listagem e pré-seleciona no `/novo`. Atalhos em Funcionários: listagem (`podeVerLinhas` → “Linhas”) e form edição (“Ver linhas de ônibus” na seção VT).

## Steps

### Step 1 — Implement

| File | Role |
|------|------|
| `funcionario-linha.models.ts` | DTO + filtro + salvar/atualizar |
| `funcionario-linha.service.ts` | GET/POST/PUT `api/funcionario-linhas` (sem DELETE) |
| `funcionario-linha-list/*` | Colunas Funcionário, Linha, Qtd, datas; filtros funcionário + vigência |
| `funcionario-linha-form/*` | Funcionário, linha (vigentes), quantidade ≥ 1, datas |
| `app.routes.ts` | `/funcionario-linhas`, `/novo`, `/:id/editar` |
| `funcionario-list` | Atalho `Linhas` com `funcionarioId` |
| `funcionario-form` | Link `Ver linhas de ônibus` (edição + permissão) |

UTF-8 BOM verified (`EF BB BF`) on new/changed front files.

### Step 2 — Build

`npx ng build` in `Beneficios.Front` → **PASS**

### Step 3 — Commit

`feat(front): add funcionario-linhas UI and funcionario shortcuts`

## Self-Review

- Menu: automatic via `MODULOS_SISTEMA` (`funcionario_linhas` → `/funcionario-linhas`) — no shell edit.
- No Excluir (design: encerrar via `data_fim`).
- Form edit keeps current linha in select even if no longer vigente.
- Concern: VT gate messages come from API on save (front does not pre-check VT ativo).

## Fix pass

**Finding:** Front did not pre-check active Vale Transporte before submit (Important from Task 9 review).

**Changes (`funcionario-linha-form`):**
- On `funcionarioId` change (select / query preselect / edit load via `valueChanges` + `patchValue`), call `FuncionarioService.obterPorId` and require `beneficios` with `codigoBeneficio === 'vale_transporte'` and `ativo === true`.
- If VT inactive (or load fails): show API-aligned message `Funcionário sem vale transporte ativo; não é possível vincular linhas.`, disable Salvar, and block `salvar()`.
- `quantidade`: `Validators.min(1)` + `Validators.pattern(/^\d+$/)` (integers only; HTML already had `min`/`step`).
- UTF-8 BOM (`EF BB BF`) on `.ts` / `.html` / `.scss`.

**Build evidence:** `npx ng build` in `Beneficios.Front` → **PASS** (Application bundle generation complete, ~10.3s).

**Commit:** `fix(front): block funcionario-linha submit without active VT`
