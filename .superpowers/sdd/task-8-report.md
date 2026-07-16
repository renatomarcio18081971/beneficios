# Task 8 Report — Frontend — Linhas de Ônibus

**Status:** DONE  
**Commit:** `87ef916` — feat(front): add linhas-onibus list and form  
**Branch:** `implementando`

## Summary

Added Angular 22 standalone cadastro for Linhas de Ônibus mirroring afastamento list/form Material patterns (reactive forms, filters, export, Novo/Editar, no Excluir). Routes use `tenantGuard` + `permissaoGuard('linhas_onibus')`. Shell menu needs no change — `MODULOS_SISTEMA` already exposes `linhas_onibus` → `/linhas-onibus`.

## Steps

### Step 1 — Scaffold

| File | Role |
|------|------|
| `linha-onibus.models.ts` | DTO + filtro |
| `linha-onibus.service.ts` | GET/POST/PUT `api/linhas-onibus` |
| `linha-onibus-list/*` | Listagem: Descrição, Data início/fim (“Em aberto”), Tarifa; filtros descrição + vigência |
| `linha-onibus-form/*` | Form: descrição, datas, tarifa ≥ 0 |
| `app.routes.ts` | `/linhas-onibus`, `/nova`, `/:id/editar` |

UTF-8 BOM verified (`EF BB BF`) on all new/changed front files.

### Step 2 — Build

`npx ng build` in `Beneficios.Front` → **PASS**

### Step 3 — Commit

`feat(front): add linhas-onibus list and form`

## Self-Review

- Menu: automatic via `MODULOS_SISTEMA` + shell `navItems` — no shell edit.
- No Excluir button (design: encerrar via `data_fim`).
- Concern: list shows tarifa with `number:'1.2-2'` (not `currency` pipe) to match simple table style; export uses BRL currency format.
