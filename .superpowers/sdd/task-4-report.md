# Task 4 Report — DDL e provisionamento tenant

**Status:** DONE  
**Commit:** `f9c8dcd` — feat(infra): add linhas_onibus and funcionario_linhas DDL provisioning  
**Branch:** `implementando`

## Summary

Added tenant DDL builders `CriarTabelaLinhasOnibus` and `CriarTabelaFuncionarioLinhas` (UNIQUE + FKs), wired both into `ProvisionarAsync` and `GarantirPerfisNoSchemaAsync` after afastamentos with linhas before funcionario_linhas, added reference scripts 14/15 (UTF-8 BOM), and updated `PostgresFixture` plus unit/integration assertions.

## Steps

### Step 1 — SQL builders + scripts

| File | Change |
|------|--------|
| `TenantSchemaSql.cs` | `CriarTabelaLinhasOnibus`, `CriarTabelaFuncionarioLinhas` |
| `14_Create_Linhas_Onibus_Tenant.sql` | Reference → `CriarTabelaLinhasOnibus` (BOM) |
| `15_Create_Funcionario_Linhas_Tenant.sql` | Reference → `CriarTabelaFuncionarioLinhas` (BOM) |

### Step 2 — Provisioner + fixture

Called both methods in `ProvisionarAsync` and `GarantirPerfisNoSchemaAsync` after afastamentos; `PostgresFixture.ProvisionTenantSchemaAsync` updated the same way.

### Step 3 — Build Infrastructure

`dotnet build src/Beneficios.Infrastructure/... --no-dependencies` — **PASS** (API DLL lock workaround)

### Step 4 — Tests

`dotnet test --filter TenantSchemaSqlTests|TenantProvisionerTests|DatabaseConfigurationTests --no-build` — **PASS** (10/10)

### Step 5 — Commit

`f9c8dcd` — 7 files (infra + scripts + fixture + tests)

## Self-Review

- Order: linhas_onibus before funcionario_linhas (FK).
- Pattern matches `CriarTabelaFuncionarioAfastamentos`.
- Scripts are reference-only, applied via TenantSchemaSql.
- Concern: full solution build needs API stopped (DLL lock); used `--no-dependencies`.