# Final whole-branch review package — linhas-onibus
Base: 2f6194f025a9e8459fd34b7c68a763f8f6ddfe17 (plan commit; Task 1+)
Head: ccf2349c295c1d0bb2c925742b34b560b351d074

## Commits
ccf2349 test: expand linhas-onibus coverage to meet Sonar threshold c6d7fdd fix(front): block funcionario-linha submit without active VT ef6bac0 feat(front): add funcionario-linhas UI and funcionario shortcuts 87ef916 feat(front): add linhas-onibus list and form 184fe3b feat: close open bus-line links when VT is deactivated 3963f0f feat: add funcionario-linhas service, repository, and API e4afaf3 feat: add linhas-onibus service, repository, and API f9c8dcd feat(infra): add linhas_onibus and funcionario_linhas DDL provisioning d3deb3a feat(domain): add funcionario-linha persistence contracts 16059e1 feat(domain): add linha onibus persistence contracts 783427a feat: add linhas_onibus and funcionario_linhas permission modules

## Stat
 .superpowers/sdd/task-9-report.md                  |  54 ++++  Beneficios.Front/e2e/fixtures/api-mocks.ts         |  30 ++  Beneficios.Front/e2e/tenant-flows.spec.ts          |  28 ++  Beneficios.Front/src/app/app.routes.ts             |  48 +++  .../src/app/core/api/funcionario-linha.models.ts   |  30 ++  .../src/app/core/api/funcionario-linha.service.ts  |  36 +++  .../src/app/core/api/linha-onibus.models.ts        |  19 ++  .../src/app/core/api/linha-onibus.service.ts       |  35 ++  .../src/app/core/auth/modulos-sistema.ts           |  14 +  .../funcionario-linha-form.component.html          |  61 ++++  .../funcionario-linha-form.component.scss          |  35 ++  .../funcionario-linha-form.component.ts            | 224 +++++++++++++  .../funcionario-linha-list.component.html          | 131 ++++++++  .../funcionario-linha-list.component.scss          |  78 +++++  .../funcionario-linha-list.component.ts            | 152 +++++++++  .../funcionario-form.component.html                |   5 +  .../funcionario-form/funcionario-form.component.ts |   1 +  .../funcionario-list.component.html                |   3 +  .../funcionario-list/funcionario-list.component.ts |   1 +  .../linha-onibus-form.component.html               |  41 +++  .../linha-onibus-form.component.scss               |  29 ++  .../linha-onibus-form.component.ts                 | 120 +++++++  .../linha-onibus-list.component.html               | 122 +++++++  .../linha-onibus-list.component.scss               |  78 +++++  .../linha-onibus-list.component.ts                 | 128 ++++++++  .../Controllers/FuncionarioLinhasController.cs     | 174 ++++++++++  .../Controllers/LinhasOnibusController.cs          | 174 ++++++++++  src/Beneficios.Api/Program.cs                      |   4 +  .../DTOs/FuncionarioLinhaDtos.cs                   |  13 +  src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs |   5 +  .../Interfaces/IFuncionarioLinhaService.cs         |  12 +  .../Interfaces/ILinhaOnibusService.cs              |  11 +  .../Mappings/FuncionarioLinhaProfile.cs            |  13 +  .../Mappings/LinhaOnibusProfile.cs                 |  13 +  .../Services/FuncionarioLinhaService.cs            | 150 +++++++++  .../Services/FuncionarioService.cs                 |  18 ++  .../Services/LinhaOnibusService.cs                 | 103 ++++++  .../Interfaces/IFuncionarioLinhaRepository.cs      |  14 +  .../Interfaces/ILinhaOnibusRepository.cs           |  11 +  .../Models/FuncionarioLinhaAtualizarParams.cs      |  12 +  .../Models/FuncionarioLinhaFiltroParams.cs         |   8 +  .../Models/FuncionarioLinhaQueryResult.cs          |  15 +  .../Models/FuncionarioLinhaSalvarParams.cs         |  13 +  .../Models/LinhaOnibusAtualizarParams.cs           |  12 +  .../Models/LinhaOnibusFiltroParams.cs              |   8 +  .../Models/LinhaOnibusQueryResult.cs               |  12 +  .../Models/LinhaOnibusSalvarParams.cs              |  12 +  src/Beneficios.Domain/ModulosSistemaCatalog.cs     |   2 +  .../Repositories/FuncionarioLinhaRepository.cs     | 223 +++++++++++++  .../Repositories/LinhaOnibusRepository.cs          | 166 ++++++++++  .../Scripts/14_Create_Linhas_Onibus_Tenant.sql     |   2 +  .../15_Create_Funcionario_Linhas_Tenant.sql        |   2 +  .../Tenancy/TenantProvisioner.cs                   |  16 +  .../Tenancy/TenantSchemaSql.cs                     |  56 ++++  .../Api/FuncionarioLinhasControllerTests.cs        | 238 ++++++++++++++  .../Api/LinhasOnibusControllerTests.cs             | 252 +++++++++++++++  .../Application/FuncionarioLinhaServiceTests.cs    | 357 +++++++++++++++++++++  .../Application/FuncionarioServiceTests.cs         |  77 ++++-  .../Application/LinhaOnibusServiceTests.cs         | 238 ++++++++++++++  .../Domain/ModulosSistemaCatalogTests.cs           |  14 +  .../Infrastructure/PostgresFixture.cs              |   2 +  .../Infrastructure/TenantProvisionerTests.cs       |  32 ++  .../Infrastructure/TenantSchemaSqlTests.cs         |  21 ++  63 files changed, 4007 insertions(+), 1 deletion(-)

## Diffdiff --git a/.superpowers/sdd/task-9-report.md b/.superpowers/sdd/task-9-report.md
new file mode 100644
index 0000000..7695a1c
--- /dev/null
+++ b/.superpowers/sdd/task-9-report.md
@@ -0,0 +1,54 @@
+# Task 9 Report ÔÇö Frontend ÔÇö Funcion├írio ├ù Linhas + atalho
+
+**Status:** DONE  
+**Commit:** `ef6bac0` ÔÇö feat(front): add funcionario-linhas UI and funcionario shortcuts  
+**Branch:** `implementando`
+
+## Summary
+
+Added Angular standalone cadastro for Funcion├írio ├ù Linhas (list + form), mirroring afastamentos Material patterns: reactive forms, filters, export Excel/PDF, Novo/Editar, **no Excluir**. Routes use `tenantGuard` + `permissaoGuard('funcionario_linhas')`. Query `?funcionarioId=` pr├®-filtra a listagem e pr├®-seleciona no `/novo`. Atalhos em Funcion├írios: listagem (`podeVerLinhas` ÔåÆ ÔÇ£LinhasÔÇØ) e form edi├º├úo (ÔÇ£Ver linhas de ├┤nibusÔÇØ na se├º├úo VT).
+
+## Steps
+
+### Step 1 ÔÇö Implement
+
+| File | Role |
+|------|------|
+| `funcionario-linha.models.ts` | DTO + filtro + salvar/atualizar |
+| `funcionario-linha.service.ts` | GET/POST/PUT `api/funcionario-linhas` (sem DELETE) |
+| `funcionario-linha-list/*` | Colunas Funcion├írio, Linha, Qtd, datas; filtros funcion├írio + vig├¬ncia |
+| `funcionario-linha-form/*` | Funcion├írio, linha (vigentes), quantidade ÔëÑ 1, datas |
+| `app.routes.ts` | `/funcionario-linhas`, `/novo`, `/:id/editar` |
+| `funcionario-list` | Atalho `Linhas` com `funcionarioId` |
+| `funcionario-form` | Link `Ver linhas de ├┤nibus` (edi├º├úo + permiss├úo) |
+
+UTF-8 BOM verified (`EF BB BF`) on new/changed front files.
+
+### Step 2 ÔÇö Build
+
+`npx ng build` in `Beneficios.Front` ÔåÆ **PASS**
+
+### Step 3 ÔÇö Commit
+
+`feat(front): add funcionario-linhas UI and funcionario shortcuts`
+
+## Self-Review
+
+- Menu: automatic via `MODULOS_SISTEMA` (`funcionario_linhas` ÔåÆ `/funcionario-linhas`) ÔÇö no shell edit.
+- No Excluir (design: encerrar via `data_fim`).
+- Form edit keeps current linha in select even if no longer vigente.
+- Concern: VT gate messages come from API on save (front does not pre-check VT ativo).
+
+## Fix pass
+
+**Finding:** Front did not pre-check active Vale Transporte before submit (Important from Task 9 review).
+
+**Changes (`funcionario-linha-form`):**
+- On `funcionarioId` change (select / query preselect / edit load via `valueChanges` + `patchValue`), call `FuncionarioService.obterPorId` and require `beneficios` with `codigoBeneficio === 'vale_transporte'` and `ativo === true`.
+- If VT inactive (or load fails): show API-aligned message `Funcion├írio sem vale transporte ativo; n├úo ├® poss├¡vel vincular linhas.`, disable Salvar, and block `salvar()`.
+- `quantidade`: `Validators.min(1)` + `Validators.pattern(/^\d+$/)` (integers only; HTML already had `min`/`step`).
+- UTF-8 BOM (`EF BB BF`) on `.ts` / `.html` / `.scss`.
+
+**Build evidence:** `npx ng build` in `Beneficios.Front` ÔåÆ **PASS** (Application bundle generation complete, ~10.3s).
+
+**Commit:** `fix(front): block funcionario-linha submit without active VT`
diff --git a/Beneficios.Front/e2e/fixtures/api-mocks.ts b/Beneficios.Front/e2e/fixtures/api-mocks.ts
index 454aacf..802eb43 100644
--- a/Beneficios.Front/e2e/fixtures/api-mocks.ts
+++ b/Beneficios.Front/e2e/fixtures/api-mocks.ts
@@ -6,10 +6,12 @@ const fullPermissoes = [
   { codigoMenu: 'usuarios', visualizar: true, criar: true, editar: true, excluir: true },
   { codigoMenu: 'perfis', visualizar: true, criar: true, editar: true, excluir: true },
   { codigoMenu: 'calendario', visualizar: true, criar: true, editar: true, excluir: true },
   { codigoMenu: 'funcionarios', visualizar: true, criar: true, editar: true, excluir: true },
   { codigoMenu: 'afastamentos', visualizar: true, criar: true, editar: true, excluir: true },
+  { codigoMenu: 'linhas_onibus', visualizar: true, criar: true, editar: true, excluir: true },
+  { codigoMenu: 'funcionario_linhas', visualizar: true, criar: true, editar: true, excluir: true },
 ];
 
 export const perfilDonoId = '44444444-4444-4444-4444-444444444444';
 
 export const tenantSession = {
@@ -234,10 +236,38 @@ export async function mockAfastamentoList(page: Page, afastamentos: unknown[] =
     }
     await route.continue();
   });
 }
 
+export async function mockLinhaOnibusList(page: Page, linhas: unknown[] = []): Promise<void> {
+  await page.route(`${API_BASE}/linhas-onibus**`, async (route: Route) => {
+    if (route.request().method() === 'GET') {
+      await route.fulfill({
+        status: 200,
+        contentType: 'application/json',
+        body: JSON.stringify(linhas),
+      });
+      return;
+    }
+    await route.continue();
+  });
+}
+
+export async function mockFuncionarioLinhaList(page: Page, vinculos: unknown[] = []): Promise<void> {
+  await page.route(`${API_BASE}/funcionario-linhas**`, async (route: Route) => {
+    if (route.request().method() === 'GET') {
+      await route.fulfill({
+        status: 200,
+        contentType: 'application/json',
+        body: JSON.stringify(vinculos),
+      });
+      return;
+    }
+    await route.continue();
+  });
+}
+
 export async function loginTenant(page: Page): Promise<void> {
   await mockTenantLogin(page);
   await mockUsuarioList(page);
   await mockPerfilList(page);
   await page.goto('/login');
diff --git a/Beneficios.Front/e2e/tenant-flows.spec.ts b/Beneficios.Front/e2e/tenant-flows.spec.ts
index 694f953..1b35668 100644
--- a/Beneficios.Front/e2e/tenant-flows.spec.ts
+++ b/Beneficios.Front/e2e/tenant-flows.spec.ts
@@ -2,10 +2,12 @@
 import {
   loginTenant,
   mockCalendarioMes,
   mockFuncionarioList,
   mockAfastamentoList,
+  mockLinhaOnibusList,
+  mockFuncionarioLinhaList,
   mockPerfilList,
   mockTenantLogin,
   mockTenantLoginSemPerfil,
   mockUnauthorized,
   mockUsuarioList,
@@ -74,10 +76,36 @@ test.describe('Fluxos cr├¡ticos tenant', () => {
     await page.getByRole('link', { name: 'Afastamentos/F├®rias' }).click();
     await page.waitForURL('**/afastamentos');
     await expect(page.getByRole('heading', { name: 'Afastamentos/F├®rias' })).toBeVisible();
   });
 
+  test('abrir Linhas de ├önibus exibe listagem', async ({ page }) => {
+    await mockLinhaOnibusList(page, [
+      {
+        id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
+        descricao: 'Linha 100',
+        dataInicio: '2026-01-01',
+        dataFim: null,
+        valorTarifa: 4.5,
+      },
+    ]);
+    await loginTenant(page);
+    await page.getByRole('link', { name: 'Linhas de ├önibus' }).click();
+    await page.waitForURL('**/linhas-onibus');
+    await expect(page.getByRole('heading', { name: 'Linhas de ├önibus' })).toBeVisible();
+    await expect(page.getByText('Linha 100')).toBeVisible();
+  });
+
+  test('abrir Funcion├írio ├ù Linhas exibe listagem', async ({ page }) => {
+    await mockFuncionarioLinhaList(page, []);
+    await mockFuncionarioList(page, []);
+    await loginTenant(page);
+    await page.getByRole('link', { name: 'Funcion├írio ├ù Linhas' }).click();
+    await page.waitForURL('**/funcionario-linhas');
+    await expect(page.getByRole('heading', { name: 'Funcion├írio ├ù Linhas' })).toBeVisible();
+  });
+
   test('criar usu├írio na tenant', async ({ page }) => {
     await loginTenant(page);
     await page.getByRole('link', { name: 'Usu├írios' }).click();
     await page.waitForURL('**/usuarios');
     await page.getByRole('link', { name: /Novo usu├írio/i }).click();
diff --git a/Beneficios.Front/src/app/app.routes.ts b/Beneficios.Front/src/app/app.routes.ts
index 201c3df..0ee2358 100644
--- a/Beneficios.Front/src/app/app.routes.ts
+++ b/Beneficios.Front/src/app/app.routes.ts
@@ -143,10 +143,58 @@ export const routes: Routes = [
         loadComponent: () =>
           import('./features/afastamentos/afastamento-form/afastamento-form.component').then(
             (m) => m.AfastamentoFormComponent,
           ),
       },
+      {
+        path: 'linhas-onibus',
+        canActivate: [tenantGuard, permissaoGuard('linhas_onibus')],
+        loadComponent: () =>
+          import('./features/linhas-onibus/linha-onibus-list/linha-onibus-list.component').then(
+            (m) => m.LinhaOnibusListComponent,
+          ),
+      },
+      {
+        path: 'linhas-onibus/nova',
+        canActivate: [tenantGuard, permissaoGuard('linhas_onibus')],
+        loadComponent: () =>
+          import('./features/linhas-onibus/linha-onibus-form/linha-onibus-form.component').then(
+            (m) => m.LinhaOnibusFormComponent,
+          ),
+      },
+      {
+        path: 'linhas-onibus/:id/editar',
+        canActivate: [tenantGuard, permissaoGuard('linhas_onibus')],
+        loadComponent: () =>
+          import('./features/linhas-onibus/linha-onibus-form/linha-onibus-form.component').then(
+            (m) => m.LinhaOnibusFormComponent,
+          ),
+      },
+      {
+        path: 'funcionario-linhas',
+        canActivate: [tenantGuard, permissaoGuard('funcionario_linhas')],
+        loadComponent: () =>
+          import('./features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component').then(
+            (m) => m.FuncionarioLinhaListComponent,
+          ),
+      },
+      {
+        path: 'funcionario-linhas/novo',
+        canActivate: [tenantGuard, permissaoGuard('funcionario_linhas')],
+        loadComponent: () =>
+          import('./features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component').then(
+            (m) => m.FuncionarioLinhaFormComponent,
+          ),
+      },
+      {
+        path: 'funcionario-linhas/:id/editar',
+        canActivate: [tenantGuard, permissaoGuard('funcionario_linhas')],
+        loadComponent: () =>
+          import('./features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component').then(
+            (m) => m.FuncionarioLinhaFormComponent,
+          ),
+      },
       { path: '', pathMatch: 'full', redirectTo: 'login' },
     ],
   },
   { path: '**', redirectTo: 'login' },
 ];
diff --git a/Beneficios.Front/src/app/core/api/funcionario-linha.models.ts b/Beneficios.Front/src/app/core/api/funcionario-linha.models.ts
new file mode 100644
index 0000000..277ddbd
--- /dev/null
+++ b/Beneficios.Front/src/app/core/api/funcionario-linha.models.ts
@@ -0,0 +1,30 @@
+´╗┐export interface FuncionarioLinhaDto {
+  id: string;
+  funcionarioId: string;
+  funcionarioNome: string;
+  linhaOnibusId: string;
+  linhaDescricao: string;
+  quantidade: number;
+  dataInicio: string;
+  dataFim: string | null;
+}
+
+export interface FuncionarioLinhaFiltroRequest {
+  funcionarioId?: string;
+  somenteVigentes?: boolean;
+}
+
+export interface FuncionarioLinhaSalvarRequest {
+  funcionarioId: string;
+  linhaOnibusId: string;
+  quantidade: number;
+  dataInicio: string;
+  dataFim: string | null;
+}
+
+export interface FuncionarioLinhaAtualizarRequest {
+  linhaOnibusId: string;
+  quantidade: number;
+  dataInicio: string;
+  dataFim: string | null;
+}
diff --git a/Beneficios.Front/src/app/core/api/funcionario-linha.service.ts b/Beneficios.Front/src/app/core/api/funcionario-linha.service.ts
new file mode 100644
index 0000000..1ae0d5e
--- /dev/null
+++ b/Beneficios.Front/src/app/core/api/funcionario-linha.service.ts
@@ -0,0 +1,36 @@
+´╗┐import { Injectable, inject } from '@angular/core';
+import { HttpClient, HttpParams } from '@angular/common/http';
+import { Observable } from 'rxjs';
+import { environment } from '../../../environments/environment';
+import {
+  FuncionarioLinhaAtualizarRequest,
+  FuncionarioLinhaDto,
+  FuncionarioLinhaFiltroRequest,
+  FuncionarioLinhaSalvarRequest,
+} from './funcionario-linha.models';
+
+@Injectable({ providedIn: 'root' })
+export class FuncionarioLinhaService {
+  private readonly http = inject(HttpClient);
+  private readonly baseUrl = `${environment.apiUrl}/funcionario-linhas`;
+
+  filtrar(filtro: FuncionarioLinhaFiltroRequest = {}): Observable<FuncionarioLinhaDto[]> {
+    let params = new HttpParams();
+    if (filtro.funcionarioId) params = params.set('funcionarioId', filtro.funcionarioId);
+    if (filtro.somenteVigentes === true) params = params.set('somenteVigentes', 'true');
+    if (filtro.somenteVigentes === false) params = params.set('somenteVigentes', 'false');
+    return this.http.get<FuncionarioLinhaDto[]>(this.baseUrl, { params });
+  }
+
+  obterPorId(id: string): Observable<FuncionarioLinhaDto> {
+    return this.http.get<FuncionarioLinhaDto>(`${this.baseUrl}/${id}`);
+  }
+
+  criar(payload: FuncionarioLinhaSalvarRequest): Observable<{ id: string }> {
+    return this.http.post<{ id: string }>(this.baseUrl, payload);
+  }
+
+  atualizar(id: string, payload: FuncionarioLinhaAtualizarRequest): Observable<void> {
+    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
+  }
+}
diff --git a/Beneficios.Front/src/app/core/api/linha-onibus.models.ts b/Beneficios.Front/src/app/core/api/linha-onibus.models.ts
new file mode 100644
index 0000000..a2d68fd
--- /dev/null
+++ b/Beneficios.Front/src/app/core/api/linha-onibus.models.ts
@@ -0,0 +1,19 @@
+´╗┐export interface LinhaOnibusDto {
+  id: string;
+  descricao: string;
+  dataInicio: string;
+  dataFim: string | null;
+  valorTarifa: number;
+}
+
+export interface LinhaOnibusSalvarDto {
+  descricao: string;
+  dataInicio: string;
+  dataFim: string | null;
+  valorTarifa: number;
+}
+
+export interface LinhaOnibusFiltroRequest {
+  descricao?: string;
+  somenteVigentes?: boolean;
+}
diff --git a/Beneficios.Front/src/app/core/api/linha-onibus.service.ts b/Beneficios.Front/src/app/core/api/linha-onibus.service.ts
new file mode 100644
index 0000000..f2b62b1
--- /dev/null
+++ b/Beneficios.Front/src/app/core/api/linha-onibus.service.ts
@@ -0,0 +1,35 @@
+´╗┐import { Injectable, inject } from '@angular/core';
+import { HttpClient, HttpParams } from '@angular/common/http';
+import { Observable } from 'rxjs';
+import { environment } from '../../../environments/environment';
+import {
+  LinhaOnibusDto,
+  LinhaOnibusFiltroRequest,
+  LinhaOnibusSalvarDto,
+} from './linha-onibus.models';
+
+@Injectable({ providedIn: 'root' })
+export class LinhaOnibusService {
+  private readonly http = inject(HttpClient);
+  private readonly baseUrl = `${environment.apiUrl}/linhas-onibus`;
+
+  filtrar(filtro: LinhaOnibusFiltroRequest = {}): Observable<LinhaOnibusDto[]> {
+    let params = new HttpParams();
+    if (filtro.descricao) params = params.set('descricao', filtro.descricao);
+    if (filtro.somenteVigentes === true) params = params.set('somenteVigentes', 'true');
+    if (filtro.somenteVigentes === false) params = params.set('somenteVigentes', 'false');
+    return this.http.get<LinhaOnibusDto[]>(this.baseUrl, { params });
+  }
+
+  obterPorId(id: string): Observable<LinhaOnibusDto> {
+    return this.http.get<LinhaOnibusDto>(`${this.baseUrl}/${id}`);
+  }
+
+  criar(payload: LinhaOnibusSalvarDto): Observable<{ id: string }> {
+    return this.http.post<{ id: string }>(this.baseUrl, payload);
+  }
+
+  atualizar(id: string, payload: LinhaOnibusSalvarDto): Observable<void> {
+    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
+  }
+}
diff --git a/Beneficios.Front/src/app/core/auth/modulos-sistema.ts b/Beneficios.Front/src/app/core/auth/modulos-sistema.ts
index d64ee7c..f938869 100644
--- a/Beneficios.Front/src/app/core/auth/modulos-sistema.ts
+++ b/Beneficios.Front/src/app/core/auth/modulos-sistema.ts
@@ -43,6 +43,20 @@ export const MODULOS_SISTEMA: readonly ModuloSistema[] = [
     nomeExibicao: 'Afastamentos/F├®rias',
     rota: '/afastamentos',
     icone: 'event_busy',
     acoesSuportadas: ['visualizar', 'criar', 'editar', 'excluir'],
   },
+  {
+    codigo: 'linhas_onibus',
+    nomeExibicao: 'Linhas de ├önibus',
+    rota: '/linhas-onibus',
+    icone: 'directions_bus',
+    acoesSuportadas: ['visualizar', 'criar', 'editar', 'excluir'],
+  },
+  {
+    codigo: 'funcionario_linhas',
+    nomeExibicao: 'Funcion├írio ├ù Linhas',
+    rota: '/funcionario-linhas',
+    icone: 'commute',
+    acoesSuportadas: ['visualizar', 'criar', 'editar', 'excluir'],
+  },
 ] as const;
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.html b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.html
new file mode 100644
index 0000000..a603d5d
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.html
@@ -0,0 +1,61 @@
+´╗┐<div class="funcionario-linha-form-page">
+  <div class="header">
+    <h1>{{ titulo }}</h1>
+    <a mat-button routerLink="/funcionario-linhas">Voltar</a>
+  </div>
+
+  @if (carregando()) {
+    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
+  } @else {
+    <form [formGroup]="form" (ngSubmit)="salvar()" class="form">
+      <mat-form-field appearance="outline">
+        <mat-label>Funcion├írio</mat-label>
+        <mat-select formControlName="funcionarioId" [disabled]="!!id()">
+          @for (f of funcionarios(); track f.id) {
+            <mat-option [value]="f.id">{{ f.nome }}</mat-option>
+          }
+        </mat-select>
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Linha</mat-label>
+        <mat-select formControlName="linhaOnibusId">
+          @for (l of linhas(); track l.id) {
+            <mat-option [value]="l.id">{{ l.descricao }}</mat-option>
+          }
+        </mat-select>
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Quantidade</mat-label>
+        <input matInput type="number" min="1" step="1" formControlName="quantidade" />
+        @if (form.controls.quantidade.touched && form.controls.quantidade.hasError('pattern')) {
+          <mat-error>Informe um n├║mero inteiro.</mat-error>
+        }
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Data in├¡cio</mat-label>
+        <input matInput type="date" formControlName="dataInicio" />
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Data fim</mat-label>
+        <input matInput type="date" formControlName="dataFim" />
+      </mat-form-field>
+
+      @if (mensagemVtInativo(); as msg) {
+        <p class="vt-warning" role="alert">{{ msg }}</p>
+      }
+
+      <div class="actions">
+        @if (podeSalvar) {
+          <button mat-flat-button color="primary" type="submit" [disabled]="submitDesabilitado">
+            {{ salvando() ? 'SalvandoÔÇª' : 'Salvar' }}
+          </button>
+        }
+        <a mat-button routerLink="/funcionario-linhas">Cancelar</a>
+      </div>
+    </form>
+  }
+</div>
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.scss b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.scss
new file mode 100644
index 0000000..5014267
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.scss
@@ -0,0 +1,35 @@
+´╗┐.funcionario-linha-form-page {
+  padding: 1.5rem;
+  max-width: 720px;
+}
+.header {
+  display: flex;
+  justify-content: space-between;
+  align-items: center;
+  margin-bottom: 1rem;
+}
+.form {
+  display: grid;
+  grid-template-columns: 1fr 1fr;
+  gap: 0.75rem;
+}
+.actions {
+  grid-column: 1 / -1;
+  display: flex;
+  gap: 0.75rem;
+  margin-top: 0.5rem;
+}
+.loading {
+  display: flex;
+  justify-content: center;
+  padding: 2rem;
+}
+.vt-warning {
+  grid-column: 1 / -1;
+  margin: 0;
+  padding: 0.75rem 1rem;
+  border-radius: 4px;
+  background: #fdecea;
+  color: #b71c1c;
+  font-size: 0.875rem;
+}
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.ts b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.ts
new file mode 100644
index 0000000..a65751a
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-form/funcionario-linha-form.component.ts
@@ -0,0 +1,224 @@
+´╗┐import { Component, OnInit, inject, signal } from '@angular/core';
+import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
+import { ActivatedRoute, Router, RouterLink } from '@angular/router';
+import { MatButtonModule } from '@angular/material/button';
+import { MatFormFieldModule } from '@angular/material/form-field';
+import { MatInputModule } from '@angular/material/input';
+import { MatSelectModule } from '@angular/material/select';
+import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
+import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
+import { finalize } from 'rxjs';
+import { FuncionarioLinhaService } from '../../../core/api/funcionario-linha.service';
+import { FuncionarioService } from '../../../core/api/funcionario.service';
+import { Funcionario } from '../../../core/api/funcionario.models';
+import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
+import { LinhaOnibusDto } from '../../../core/api/linha-onibus.models';
+import { PermissaoService } from '../../../core/auth/permissao.service';
+
+const MSG_VT_INATIVO =
+  'Funcion├írio sem vale transporte ativo; n├úo ├® poss├¡vel vincular linhas.';
+
+@Component({
+  selector: 'app-funcionario-linha-form',
+  standalone: true,
+  imports: [
+    ReactiveFormsModule,
+    RouterLink,
+    MatButtonModule,
+    MatFormFieldModule,
+    MatInputModule,
+    MatSelectModule,
+    MatSnackBarModule,
+    MatProgressSpinnerModule,
+  ],
+  templateUrl: './funcionario-linha-form.component.html',
+  styleUrl: './funcionario-linha-form.component.scss',
+})
+export class FuncionarioLinhaFormComponent implements OnInit {
+  private readonly fb = inject(FormBuilder);
+  private readonly service = inject(FuncionarioLinhaService);
+  private readonly funcionarioService = inject(FuncionarioService);
+  private readonly linhaService = inject(LinhaOnibusService);
+  private readonly route = inject(ActivatedRoute);
+  private readonly router = inject(Router);
+  private readonly snack = inject(MatSnackBar);
+  private readonly permissao = inject(PermissaoService);
+
+  readonly carregando = signal(false);
+  readonly salvando = signal(false);
+  readonly verificandoVt = signal(false);
+  readonly mensagemVtInativo = signal<string | null>(null);
+  readonly id = signal<string | null>(null);
+  readonly funcionarios = signal<Funcionario[]>([]);
+  readonly linhas = signal<LinhaOnibusDto[]>([]);
+  readonly podeEditar = this.permissao.possuiPermissao('funcionario_linhas', 'editar');
+  readonly podeCriar = this.permissao.possuiPermissao('funcionario_linhas', 'criar');
+
+  readonly form = this.fb.nonNullable.group({
+    funcionarioId: ['', Validators.required],
+    linhaOnibusId: ['', Validators.required],
+    quantidade: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
+    dataInicio: ['', Validators.required],
+    dataFim: [''],
+  });
+
+  get titulo(): string {
+    return this.id() ? 'Editar v├¡nculo' : 'Novo v├¡nculo';
+  }
+
+  get podeSalvar(): boolean {
+    return this.id() ? this.podeEditar : this.podeCriar;
+  }
+
+  get submitDesabilitado(): boolean {
+    return this.salvando() || this.verificandoVt() || !!this.mensagemVtInativo();
+  }
+
+  ngOnInit(): void {
+    this.funcionarioService.filtrar().subscribe({
+      next: (lista) => this.funcionarios.set(lista),
+      error: () => this.funcionarios.set([]),
+    });
+
+    this.form.controls.funcionarioId.valueChanges.subscribe((funcionarioId) => {
+      this.verificarVtAtivo(funcionarioId);
+    });
+
+    const id = this.route.snapshot.paramMap.get('id');
+    const funcionarioId = this.route.snapshot.queryParamMap.get('funcionarioId');
+    if (funcionarioId && !id) {
+      this.form.patchValue({ funcionarioId });
+    }
+
+    if (!id) {
+      this.carregarLinhasVigentes();
+      if (!this.podeCriar && !this.podeEditar) this.form.disable();
+      return;
+    }
+
+    this.id.set(id);
+    this.carregando.set(true);
+    this.service
+      .obterPorId(id)
+      .pipe(finalize(() => this.carregando.set(false)))
+      .subscribe({
+        next: (v) => {
+          this.carregarLinhasVigentes({ id: v.linhaOnibusId, descricao: v.linhaDescricao });
+          this.form.patchValue({
+            funcionarioId: v.funcionarioId,
+            linhaOnibusId: v.linhaOnibusId,
+            quantidade: v.quantidade,
+            dataInicio: v.dataInicio?.substring(0, 10) ?? '',
+            dataFim: v.dataFim?.substring(0, 10) ?? '',
+          });
+          if (!this.podeEditar) this.form.disable();
+        },
+        error: () => {
+          this.snack.open('V├¡nculo n├úo encontrado.', 'Fechar', { duration: 4000 });
+          void this.router.navigate(['/funcionario-linhas']);
+        },
+      });
+  }
+
+  salvar(): void {
+    if (this.form.invalid || !this.podeSalvar || this.submitDesabilitado) {
+      this.form.markAllAsTouched();
+      if (this.mensagemVtInativo()) {
+        this.snack.open(this.mensagemVtInativo()!, 'Fechar', { duration: 5000 });
+      }
+      return;
+    }
+    const v = this.form.getRawValue();
+    this.salvando.set(true);
+    const done = () => this.salvando.set(false);
+    const onOk = () => {
+      this.snack.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
+      void this.router.navigate(['/funcionario-linhas'], {
+        queryParams: v.funcionarioId ? { funcionarioId: v.funcionarioId } : undefined,
+      });
+    };
+    const onErr = (err: { error?: { message?: string } }) =>
+      this.snack.open(err?.error?.message ?? 'Erro ao salvar.', 'Fechar', { duration: 5000 });
+
+    if (this.id()) {
+      this.service
+        .atualizar(this.id()!, {
+          linhaOnibusId: v.linhaOnibusId,
+          quantidade: Number(v.quantidade),
+          dataInicio: v.dataInicio,
+          dataFim: v.dataFim || null,
+        })
+        .pipe(finalize(done))
+        .subscribe({ next: onOk, error: onErr });
+    } else {
+      this.service
+        .criar({
+          funcionarioId: v.funcionarioId,
+          linhaOnibusId: v.linhaOnibusId,
+          quantidade: Number(v.quantidade),
+          dataInicio: v.dataInicio,
+          dataFim: v.dataFim || null,
+        })
+        .pipe(finalize(done))
+        .subscribe({ next: onOk, error: onErr });
+    }
+  }
+
+  private verificarVtAtivo(funcionarioId: string | null | undefined): void {
+    if (!funcionarioId) {
+      this.mensagemVtInativo.set(null);
+      this.verificandoVt.set(false);
+      return;
+    }
+
+    this.verificandoVt.set(true);
+    this.funcionarioService
+      .obterPorId(funcionarioId)
+      .pipe(finalize(() => this.verificandoVt.set(false)))
+      .subscribe({
+        next: (f) => {
+          const vtAtivo = f.beneficios?.some(
+            (b) => b.codigoBeneficio === 'vale_transporte' && b.ativo,
+          );
+          this.mensagemVtInativo.set(vtAtivo ? null : MSG_VT_INATIVO);
+        },
+        error: () => {
+          this.mensagemVtInativo.set(MSG_VT_INATIVO);
+        },
+      });
+  }
+
+  private carregarLinhasVigentes(incluir?: { id: string; descricao: string }): void {
+    this.linhaService.filtrar({ somenteVigentes: true }).subscribe({
+      next: (lista) => {
+        if (incluir && !lista.some((l) => l.id === incluir.id)) {
+          lista = [
+            ...lista,
+            {
+              id: incluir.id,
+              descricao: incluir.descricao,
+              dataInicio: '',
+              dataFim: null,
+              valorTarifa: 0,
+            },
+          ];
+        }
+        this.linhas.set(lista);
+      },
+      error: () =>
+        this.linhas.set(
+          incluir
+            ? [
+                {
+                  id: incluir.id,
+                  descricao: incluir.descricao,
+                  dataInicio: '',
+                  dataFim: null,
+                  valorTarifa: 0,
+                },
+              ]
+            : [],
+        ),
+    });
+  }
+}
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.html b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.html
new file mode 100644
index 0000000..69bca98
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.html
@@ -0,0 +1,131 @@
+´╗┐<div class="funcionario-linha-list-page">
+  <div class="funcionario-linha-list-header">
+    <h1>Funcion├írio ├ù Linhas</h1>
+
+    <div class="header-actions">
+      @if (isMobile) {
+        <button
+          type="button"
+          mat-icon-button
+          matTooltip="Exportar Excel"
+          aria-label="Exportar Excel"
+          (click)="exportExcel()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">table_view</mat-icon>
+        </button>
+
+        <button
+          type="button"
+          mat-icon-button
+          matTooltip="Exportar PDF"
+          aria-label="Exportar PDF"
+          (click)="exportPdf()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">picture_as_pdf</mat-icon>
+        </button>
+      } @else {
+        <button
+          type="button"
+          mat-stroked-button
+          (click)="exportExcel()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">table_view</mat-icon>
+          Excel
+        </button>
+
+        <button
+          type="button"
+          mat-stroked-button
+          (click)="exportPdf()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">picture_as_pdf</mat-icon>
+          PDF
+        </button>
+      }
+
+      @if (podeCriar) {
+        <a mat-flat-button color="primary" [routerLink]="novoLink()" [queryParams]="novoQueryParams()">
+          <mat-icon aria-hidden="true">add</mat-icon>
+          Novo v├¡nculo
+        </a>
+      }
+    </div>
+  </div>
+
+  <form class="filter-bar" [formGroup]="filtro" (ngSubmit)="buscar()">
+    <mat-form-field appearance="outline" class="filter-field">
+      <mat-label>Funcion├írio</mat-label>
+      <mat-select formControlName="funcionarioId">
+        <mat-option value="">Todos</mat-option>
+        @for (f of funcionarios(); track f.id) {
+          <mat-option [value]="f.id">{{ f.nome }}</mat-option>
+        }
+      </mat-select>
+    </mat-form-field>
+    <mat-form-field appearance="outline" class="filter-field">
+      <mat-label>Vig├¬ncia</mat-label>
+      <mat-select formControlName="vigencia">
+        <mat-option value="">Todas</mat-option>
+        <mat-option value="vigentes">Somente vigentes</mat-option>
+      </mat-select>
+    </mat-form-field>
+
+    <div class="filter-actions">
+      <button mat-flat-button color="primary" type="submit" class="filter-button" [disabled]="carregando()">
+        <mat-icon aria-hidden="true">filter_alt</mat-icon>
+        Filtrar
+      </button>
+      <button
+        mat-stroked-button
+        type="button"
+        class="filter-button"
+        [disabled]="carregando()"
+        (click)="limpar()"
+      >
+        <mat-icon aria-hidden="true">cleaning_services</mat-icon>
+        Limpar
+      </button>
+    </div>
+  </form>
+
+  @if (carregando()) {
+    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
+  } @else {
+    <table mat-table [dataSource]="dataSource()" class="mat-elevation-z1">
+      <ng-container matColumnDef="funcionarioNome">
+        <th mat-header-cell *matHeaderCellDef>Funcion├írio</th>
+        <td mat-cell *matCellDef="let row">{{ row.funcionarioNome }}</td>
+      </ng-container>
+      <ng-container matColumnDef="linhaDescricao">
+        <th mat-header-cell *matHeaderCellDef>Linha</th>
+        <td mat-cell *matCellDef="let row">{{ row.linhaDescricao }}</td>
+      </ng-container>
+      <ng-container matColumnDef="quantidade">
+        <th mat-header-cell *matHeaderCellDef>Qtd</th>
+        <td mat-cell *matCellDef="let row">{{ row.quantidade }}</td>
+      </ng-container>
+      <ng-container matColumnDef="dataInicio">
+        <th mat-header-cell *matHeaderCellDef>In├¡cio</th>
+        <td mat-cell *matCellDef="let row">{{ row.dataInicio }}</td>
+      </ng-container>
+      <ng-container matColumnDef="dataFim">
+        <th mat-header-cell *matHeaderCellDef>Fim</th>
+        <td mat-cell *matCellDef="let row">{{ row.dataFim || 'Em aberto' }}</td>
+      </ng-container>
+      <ng-container matColumnDef="acoes">
+        <th mat-header-cell *matHeaderCellDef></th>
+        <td mat-cell *matCellDef="let row" class="actions-cell">
+          @if (podeEditar) {
+            <a mat-button [routerLink]="['/funcionario-linhas', row.id, 'editar']">Editar</a>
+          }
+        </td>
+      </ng-container>
+      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
+      <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
+    </table>
+  }
+</div>
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.scss b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.scss
new file mode 100644
index 0000000..5f6c8c5
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.scss
@@ -0,0 +1,78 @@
+´╗┐.funcionario-linha-list-page {
+  padding: 16px;
+}
+
+.funcionario-linha-list-header {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  justify-content: space-between;
+  gap: 16px;
+  margin-bottom: 24px;
+}
+
+.funcionario-linha-list-header h1 {
+  margin: 0;
+  font-size: 1.5rem;
+  font-weight: 500;
+}
+
+.header-actions {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  gap: 8px;
+}
+
+.filter-bar {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: flex-end;
+  gap: 12px;
+  margin-bottom: 24px;
+}
+
+.filter-field {
+  flex: 1 1 160px;
+  min-width: 0;
+}
+
+.filter-actions {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  justify-content: center;
+  gap: 8px;
+  margin-bottom: 22px;
+  flex: 1 1 auto;
+  min-width: fit-content;
+}
+
+.filter-button {
+  height: 56px;
+  min-width: 120px;
+}
+
+.loading {
+  display: flex;
+  justify-content: center;
+  padding: 48px 0;
+}
+
+table {
+  width: 100%;
+}
+
+.header-actions,
+.actions-cell,
+.filter-actions {
+  .mat-icon {
+    font-family: 'Material Icons';
+    font-feature-settings: 'liga';
+    width: 24px;
+    height: 24px;
+    font-size: 24px;
+    line-height: 1;
+    overflow: hidden;
+  }
+}
diff --git a/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.ts b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.ts
new file mode 100644
index 0000000..9073cae
--- /dev/null
+++ b/Beneficios.Front/src/app/features/funcionario-linhas/funcionario-linha-list/funcionario-linha-list.component.ts
@@ -0,0 +1,152 @@
+´╗┐import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
+import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
+import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
+import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
+import { ActivatedRoute, RouterLink } from '@angular/router';
+import { MatButtonModule } from '@angular/material/button';
+import { MatFormFieldModule } from '@angular/material/form-field';
+import { MatIconModule } from '@angular/material/icon';
+import { MatInputModule } from '@angular/material/input';
+import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
+import { MatSelectModule } from '@angular/material/select';
+import { MatTableModule } from '@angular/material/table';
+import { MatTooltipModule } from '@angular/material/tooltip';
+import { finalize } from 'rxjs';
+import { FuncionarioLinhaService } from '../../../core/api/funcionario-linha.service';
+import { FuncionarioLinhaDto } from '../../../core/api/funcionario-linha.models';
+import { FuncionarioService } from '../../../core/api/funcionario.service';
+import { Funcionario } from '../../../core/api/funcionario.models';
+import { PermissaoService } from '../../../core/auth/permissao.service';
+import { TenantService } from '../../../core/tenant/tenant.service';
+import { ExportColumn } from '../../../shared/utils/export.models';
+import { ExportService } from '../../../shared/utils/export.service';
+
+@Component({
+  selector: 'app-funcionario-linha-list',
+  standalone: true,
+  imports: [
+    ReactiveFormsModule,
+    RouterLink,
+    MatButtonModule,
+    MatFormFieldModule,
+    MatIconModule,
+    MatInputModule,
+    MatProgressSpinnerModule,
+    MatSelectModule,
+    MatTableModule,
+    MatTooltipModule,
+  ],
+  templateUrl: './funcionario-linha-list.component.html',
+  styleUrl: './funcionario-linha-list.component.scss',
+})
+export class FuncionarioLinhaListComponent implements OnInit {
+  private readonly fb = inject(FormBuilder);
+  private readonly service = inject(FuncionarioLinhaService);
+  private readonly funcionarioService = inject(FuncionarioService);
+  private readonly route = inject(ActivatedRoute);
+  private readonly permissao = inject(PermissaoService);
+  private readonly exportService = inject(ExportService);
+  private readonly tenantService = inject(TenantService);
+  private readonly breakpointObserver = inject(BreakpointObserver);
+  private readonly destroyRef = inject(DestroyRef);
+
+  readonly carregando = signal(false);
+  readonly dataSource = signal<FuncionarioLinhaDto[]>([]);
+  readonly funcionarios = signal<Funcionario[]>([]);
+  readonly displayedColumns = [
+    'funcionarioNome',
+    'linhaDescricao',
+    'quantidade',
+    'dataInicio',
+    'dataFim',
+    'acoes',
+  ];
+  readonly podeCriar = this.permissao.possuiPermissao('funcionario_linhas', 'criar');
+  readonly podeEditar = this.permissao.possuiPermissao('funcionario_linhas', 'editar');
+  isMobile = false;
+
+  readonly filtro = this.fb.nonNullable.group({
+    funcionarioId: [''],
+    vigencia: ['' as '' | 'vigentes'],
+  });
+
+  private readonly exportColumns: ExportColumn[] = [
+    { key: 'funcionarioNome', label: 'Funcion├írio' },
+    { key: 'linhaDescricao', label: 'Linha' },
+    { key: 'quantidade', label: 'Quantidade' },
+    { key: 'dataInicio', label: 'In├¡cio' },
+    {
+      key: 'dataFim',
+      label: 'Fim',
+      format: (value) => (value ? String(value) : 'Em aberto'),
+    },
+  ];
+
+  constructor() {
+    this.breakpointObserver
+      .observe([Breakpoints.XSmall])
+      .pipe(takeUntilDestroyed(this.destroyRef))
+      .subscribe((result) => {
+        this.isMobile = result.matches;
+      });
+  }
+
+  ngOnInit(): void {
+    this.funcionarioService.filtrar().subscribe({
+      next: (lista) => this.funcionarios.set(lista),
+      error: () => this.funcionarios.set([]),
+    });
+
+    const funcionarioId = this.route.snapshot.queryParamMap.get('funcionarioId');
+    if (funcionarioId) {
+      this.filtro.patchValue({ funcionarioId });
+    }
+    this.buscar();
+  }
+
+  buscar(): void {
+    this.carregando.set(true);
+    const v = this.filtro.getRawValue();
+    this.service
+      .filtrar({
+        funcionarioId: v.funcionarioId || undefined,
+        somenteVigentes: v.vigencia === 'vigentes' ? true : undefined,
+      })
+      .pipe(finalize(() => this.carregando.set(false)))
+      .subscribe({
+        next: (lista) => this.dataSource.set(lista),
+        error: () => this.dataSource.set([]),
+      });
+  }
+
+  limpar(): void {
+    this.filtro.reset({ funcionarioId: '', vigencia: '' });
+    this.buscar();
+  }
+
+  novoLink(): string[] {
+    return ['/funcionario-linhas/novo'];
+  }
+
+  novoQueryParams(): Record<string, string> | null {
+    const id = this.filtro.controls.funcionarioId.value;
+    return id ? { funcionarioId: id } : null;
+  }
+
+  exportExcel(): void {
+    const subdomain = this.tenantService.getSubdomain();
+    const filename = this.exportService.buildFilename('funcionario-linhas', subdomain, 'xlsx');
+    void this.exportService.exportToExcel(this.dataSource(), this.exportColumns, filename);
+  }
+
+  exportPdf(): void {
+    const subdomain = this.tenantService.getSubdomain();
+    const filename = this.exportService.buildFilename('funcionario-linhas', subdomain, 'pdf');
+    void this.exportService.exportToPdf(
+      this.dataSource(),
+      this.exportColumns,
+      filename,
+      'Funcion├írio ├ù Linhas',
+    );
+  }
+}
diff --git a/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.html b/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.html
index 4995013..5d901fd 100644
--- a/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.html
+++ b/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.html
@@ -171,10 +171,15 @@
           </mat-form-field>
           <mat-form-field appearance="outline">
             <mat-label>Data fim</mat-label>
             <input matInput type="date" formControlName="vtDataFim" />
           </mat-form-field>
+          @if (id() && podeVerLinhas) {
+            <p class="span-2">
+              <a [routerLink]="['/funcionario-linhas']" [queryParams]="{ funcionarioId: id() }">Ver linhas de ├┤nibus</a>
+            </p>
+          }
         </div>
       </section>
 
       <section>
         <h2>Jornada de trabalho</h2>
diff --git a/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.ts b/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.ts
index bf3c9b0..bf258b8 100644
--- a/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.ts
+++ b/Beneficios.Front/src/app/features/funcionarios/funcionario-form/funcionario-form.component.ts
@@ -55,10 +55,11 @@ export class FuncionarioFormComponent implements OnInit {
   readonly carregando = signal(false);
   readonly salvando = signal(false);
   readonly id = signal<string | null>(null);
   readonly podeEditar = this.permissao.possuiPermissao('funcionarios', 'editar');
   readonly podeCriar = this.permissao.possuiPermissao('funcionarios', 'criar');
+  readonly podeVerLinhas = this.permissao.possuiPermissao('funcionario_linhas', 'visualizar');
 
   readonly form = this.fb.nonNullable.group({
     nome: ['', Validators.required],
     cpf: ['', Validators.required],
     matricula: [''],
diff --git a/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.html b/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.html
index dcef6a2..920d725 100644
--- a/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.html
+++ b/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.html
@@ -139,10 +139,13 @@
               title="Afastamentos/F├®rias"
             >
               <mat-icon>event_busy</mat-icon>
             </a>
           }
+          @if (podeVerLinhas) {
+            <a mat-button [routerLink]="['/funcionario-linhas']" [queryParams]="{ funcionarioId: row.id }">Linhas</a>
+          }
         </td>
       </ng-container>
       <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
       <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
     </table>
diff --git a/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.ts b/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.ts
index 956a0b0..6fe0836 100644
--- a/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.ts
+++ b/Beneficios.Front/src/app/features/funcionarios/funcionario-list/funcionario-list.component.ts
@@ -53,10 +53,11 @@ export class FuncionarioListComponent {
   readonly dataSource = signal<Funcionario[]>([]);
   readonly displayedColumns = ['nome', 'cpf', 'cargo', 'situacao', 'motivo', 'jornada', 'acoes'];
   readonly podeCriar = this.permissao.possuiPermissao('funcionarios', 'criar');
   readonly podeEditar = this.permissao.possuiPermissao('funcionarios', 'editar');
   readonly podeVerAfastamentos = this.permissao.possuiPermissao('afastamentos', 'visualizar');
+  readonly podeVerLinhas = this.permissao.possuiPermissao('funcionario_linhas', 'visualizar');
   isMobile = false;
 
   readonly filtro = this.fb.nonNullable.group({
     nome: [''],
     cpf: [''],
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.html b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.html
new file mode 100644
index 0000000..be3c307
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.html
@@ -0,0 +1,41 @@
+´╗┐<div class="linha-onibus-form-page">
+  <div class="header">
+    <h1>{{ titulo }}</h1>
+    <a mat-button routerLink="/linhas-onibus">Voltar</a>
+  </div>
+
+  @if (carregando()) {
+    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
+  } @else {
+    <form [formGroup]="form" (ngSubmit)="salvar()" class="form">
+      <mat-form-field appearance="outline" class="full">
+        <mat-label>Descri├º├úo</mat-label>
+        <input matInput formControlName="descricao" />
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Data in├¡cio</mat-label>
+        <input matInput type="date" formControlName="dataInicio" />
+      </mat-form-field>
+
+      <mat-form-field appearance="outline">
+        <mat-label>Data fim</mat-label>
+        <input matInput type="date" formControlName="dataFim" />
+      </mat-form-field>
+
+      <mat-form-field appearance="outline" class="full">
+        <mat-label>Valor da tarifa</mat-label>
+        <input matInput type="number" formControlName="valorTarifa" min="0" step="0.01" />
+      </mat-form-field>
+
+      <div class="actions">
+        @if (podeSalvar) {
+          <button mat-flat-button color="primary" type="submit" [disabled]="salvando()">
+            {{ salvando() ? 'SalvandoÔÇª' : 'Salvar' }}
+          </button>
+        }
+        <a mat-button routerLink="/linhas-onibus">Cancelar</a>
+      </div>
+    </form>
+  }
+</div>
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.scss b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.scss
new file mode 100644
index 0000000..873962f
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.scss
@@ -0,0 +1,29 @@
+´╗┐.linha-onibus-form-page {
+  padding: 1.5rem;
+  max-width: 720px;
+}
+.header {
+  display: flex;
+  justify-content: space-between;
+  align-items: center;
+  margin-bottom: 1rem;
+}
+.form {
+  display: grid;
+  grid-template-columns: 1fr 1fr;
+  gap: 0.75rem;
+}
+.full {
+  grid-column: 1 / -1;
+}
+.actions {
+  grid-column: 1 / -1;
+  display: flex;
+  gap: 0.75rem;
+  margin-top: 0.5rem;
+}
+.loading {
+  display: flex;
+  justify-content: center;
+  padding: 2rem;
+}
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.ts b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.ts
new file mode 100644
index 0000000..4881a1c
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-form/linha-onibus-form.component.ts
@@ -0,0 +1,120 @@
+´╗┐import { Component, OnInit, inject, signal } from '@angular/core';
+import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
+import { ActivatedRoute, Router, RouterLink } from '@angular/router';
+import { MatButtonModule } from '@angular/material/button';
+import { MatFormFieldModule } from '@angular/material/form-field';
+import { MatInputModule } from '@angular/material/input';
+import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
+import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
+import { finalize } from 'rxjs';
+import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
+import { PermissaoService } from '../../../core/auth/permissao.service';
+
+@Component({
+  selector: 'app-linha-onibus-form',
+  standalone: true,
+  imports: [
+    ReactiveFormsModule,
+    RouterLink,
+    MatButtonModule,
+    MatFormFieldModule,
+    MatInputModule,
+    MatSnackBarModule,
+    MatProgressSpinnerModule,
+  ],
+  templateUrl: './linha-onibus-form.component.html',
+  styleUrl: './linha-onibus-form.component.scss',
+})
+export class LinhaOnibusFormComponent implements OnInit {
+  private readonly fb = inject(FormBuilder);
+  private readonly service = inject(LinhaOnibusService);
+  private readonly route = inject(ActivatedRoute);
+  private readonly router = inject(Router);
+  private readonly snack = inject(MatSnackBar);
+  private readonly permissao = inject(PermissaoService);
+
+  readonly carregando = signal(false);
+  readonly salvando = signal(false);
+  readonly id = signal<string | null>(null);
+  readonly podeEditar = this.permissao.possuiPermissao('linhas_onibus', 'editar');
+  readonly podeCriar = this.permissao.possuiPermissao('linhas_onibus', 'criar');
+
+  readonly form = this.fb.nonNullable.group({
+    descricao: ['', Validators.required],
+    dataInicio: ['', Validators.required],
+    dataFim: [''],
+    valorTarifa: [0 as number, [Validators.required, Validators.min(0)]],
+  });
+
+  get titulo(): string {
+    return this.id() ? 'Editar linha de ├┤nibus' : 'Nova linha de ├┤nibus';
+  }
+
+  get podeSalvar(): boolean {
+    return this.id() ? this.podeEditar : this.podeCriar;
+  }
+
+  ngOnInit(): void {
+    const id = this.route.snapshot.paramMap.get('id');
+
+    if (!id) {
+      if (!this.podeCriar && !this.podeEditar) this.form.disable();
+      return;
+    }
+
+    this.id.set(id);
+    this.carregando.set(true);
+    this.service
+      .obterPorId(id)
+      .pipe(finalize(() => this.carregando.set(false)))
+      .subscribe({
+        next: (linha) => {
+          this.form.patchValue({
+            descricao: linha.descricao,
+            dataInicio: linha.dataInicio?.substring(0, 10) ?? '',
+            dataFim: linha.dataFim?.substring(0, 10) ?? '',
+            valorTarifa: linha.valorTarifa,
+          });
+          if (!this.podeEditar) this.form.disable();
+        },
+        error: () => {
+          this.snack.open('Linha de ├┤nibus n├úo encontrada.', 'Fechar', { duration: 4000 });
+          void this.router.navigate(['/linhas-onibus']);
+        },
+      });
+  }
+
+  salvar(): void {
+    if (this.form.invalid || !this.podeSalvar) {
+      this.form.markAllAsTouched();
+      return;
+    }
+    const v = this.form.getRawValue();
+    this.salvando.set(true);
+    const done = () => this.salvando.set(false);
+    const payload = {
+      descricao: v.descricao.trim(),
+      dataInicio: v.dataInicio,
+      dataFim: v.dataFim || null,
+      valorTarifa: Number(v.valorTarifa),
+    };
+    const onOk = () => {
+      this.snack.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
+      void this.router.navigate(['/linhas-onibus']);
+    };
+    const onErr = (err: { error?: { message?: string } }) =>
+      this.snack.open(err?.error?.message ?? 'Erro ao salvar.', 'Fechar', { duration: 5000 });
+
+    if (this.id()) {
+      this.service
+        .atualizar(this.id()!, payload)
+        .pipe(finalize(done))
+        .subscribe({ next: onOk, error: onErr });
+    } else {
+      this.service
+        .criar(payload)
+        .pipe(finalize(done))
+        .subscribe({ next: onOk, error: onErr });
+    }
+  }
+}
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.html b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.html
new file mode 100644
index 0000000..372b6b6
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.html
@@ -0,0 +1,122 @@
+´╗┐<div class="linha-onibus-list-page">
+  <div class="linha-onibus-list-header">
+    <h1>Linhas de ├önibus</h1>
+
+    <div class="header-actions">
+      @if (isMobile) {
+        <button
+          type="button"
+          mat-icon-button
+          matTooltip="Exportar Excel"
+          aria-label="Exportar Excel"
+          (click)="exportExcel()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">table_view</mat-icon>
+        </button>
+
+        <button
+          type="button"
+          mat-icon-button
+          matTooltip="Exportar PDF"
+          aria-label="Exportar PDF"
+          (click)="exportPdf()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">picture_as_pdf</mat-icon>
+        </button>
+      } @else {
+        <button
+          type="button"
+          mat-stroked-button
+          (click)="exportExcel()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">table_view</mat-icon>
+          Excel
+        </button>
+
+        <button
+          type="button"
+          mat-stroked-button
+          (click)="exportPdf()"
+          [disabled]="carregando() || dataSource().length === 0"
+        >
+          <mat-icon aria-hidden="true">picture_as_pdf</mat-icon>
+          PDF
+        </button>
+      }
+
+      @if (podeCriar) {
+        <a mat-flat-button color="primary" routerLink="/linhas-onibus/nova">
+          <mat-icon aria-hidden="true">add</mat-icon>
+          Nova linha
+        </a>
+      }
+    </div>
+  </div>
+
+  <form class="filter-bar" [formGroup]="filtro" (ngSubmit)="buscar()">
+    <mat-form-field appearance="outline" class="filter-field">
+      <mat-label>Descri├º├úo</mat-label>
+      <input matInput type="text" formControlName="descricao" />
+    </mat-form-field>
+    <mat-form-field appearance="outline" class="filter-field">
+      <mat-label>Vig├¬ncia</mat-label>
+      <mat-select formControlName="vigencia">
+        <mat-option value="">Todas</mat-option>
+        <mat-option value="vigentes">S├│ vigentes</mat-option>
+      </mat-select>
+    </mat-form-field>
+
+    <div class="filter-actions">
+      <button mat-flat-button color="primary" type="submit" class="filter-button" [disabled]="carregando()">
+        <mat-icon aria-hidden="true">filter_alt</mat-icon>
+        Filtrar
+      </button>
+      <button
+        mat-stroked-button
+        type="button"
+        class="filter-button"
+        [disabled]="carregando()"
+        (click)="limpar()"
+      >
+        <mat-icon aria-hidden="true">cleaning_services</mat-icon>
+        Limpar
+      </button>
+    </div>
+  </form>
+
+  @if (carregando()) {
+    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
+  } @else {
+    <table mat-table [dataSource]="dataSource()" class="mat-elevation-z1">
+      <ng-container matColumnDef="descricao">
+        <th mat-header-cell *matHeaderCellDef>Descri├º├úo</th>
+        <td mat-cell *matCellDef="let row">{{ row.descricao }}</td>
+      </ng-container>
+      <ng-container matColumnDef="dataInicio">
+        <th mat-header-cell *matHeaderCellDef>Data in├¡cio</th>
+        <td mat-cell *matCellDef="let row">{{ row.dataInicio }}</td>
+      </ng-container>
+      <ng-container matColumnDef="dataFim">
+        <th mat-header-cell *matHeaderCellDef>Data fim</th>
+        <td mat-cell *matCellDef="let row">{{ row.dataFim || 'Em aberto' }}</td>
+      </ng-container>
+      <ng-container matColumnDef="valorTarifa">
+        <th mat-header-cell *matHeaderCellDef>Tarifa</th>
+        <td mat-cell *matCellDef="let row">{{ row.valorTarifa | number: '1.2-2' }}</td>
+      </ng-container>
+      <ng-container matColumnDef="acoes">
+        <th mat-header-cell *matHeaderCellDef></th>
+        <td mat-cell *matCellDef="let row" class="actions-cell">
+          @if (podeEditar) {
+            <a mat-button [routerLink]="['/linhas-onibus', row.id, 'editar']">Editar</a>
+          }
+        </td>
+      </ng-container>
+      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
+      <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
+    </table>
+  }
+</div>
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.scss b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.scss
new file mode 100644
index 0000000..8af9f44
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.scss
@@ -0,0 +1,78 @@
+´╗┐.linha-onibus-list-page {
+  padding: 16px;
+}
+
+.linha-onibus-list-header {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  justify-content: space-between;
+  gap: 16px;
+  margin-bottom: 24px;
+}
+
+.linha-onibus-list-header h1 {
+  margin: 0;
+  font-size: 1.5rem;
+  font-weight: 500;
+}
+
+.header-actions {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  gap: 8px;
+}
+
+.filter-bar {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: flex-end;
+  gap: 12px;
+  margin-bottom: 24px;
+}
+
+.filter-field {
+  flex: 1 1 160px;
+  min-width: 0;
+}
+
+.filter-actions {
+  display: flex;
+  flex-wrap: wrap;
+  align-items: center;
+  justify-content: center;
+  gap: 8px;
+  margin-bottom: 22px;
+  flex: 1 1 auto;
+  min-width: fit-content;
+}
+
+.filter-button {
+  height: 56px;
+  min-width: 120px;
+}
+
+.loading {
+  display: flex;
+  justify-content: center;
+  padding: 48px 0;
+}
+
+table {
+  width: 100%;
+}
+
+.header-actions,
+.actions-cell,
+.filter-actions {
+  .mat-icon {
+    font-family: 'Material Icons';
+    font-feature-settings: 'liga';
+    width: 24px;
+    height: 24px;
+    font-size: 24px;
+    line-height: 1;
+    overflow: hidden;
+  }
+}
diff --git a/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.ts b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.ts
new file mode 100644
index 0000000..22c2eca
--- /dev/null
+++ b/Beneficios.Front/src/app/features/linhas-onibus/linha-onibus-list/linha-onibus-list.component.ts
@@ -0,0 +1,128 @@
+´╗┐import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
+import { DecimalPipe } from '@angular/common';
+import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
+import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
+import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
+import { RouterLink } from '@angular/router';
+import { MatButtonModule } from '@angular/material/button';
+import { MatFormFieldModule } from '@angular/material/form-field';
+import { MatIconModule } from '@angular/material/icon';
+import { MatInputModule } from '@angular/material/input';
+import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
+import { MatSelectModule } from '@angular/material/select';
+import { MatTableModule } from '@angular/material/table';
+import { MatTooltipModule } from '@angular/material/tooltip';
+import { finalize } from 'rxjs';
+import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
+import { LinhaOnibusDto } from '../../../core/api/linha-onibus.models';
+import { PermissaoService } from '../../../core/auth/permissao.service';
+import { TenantService } from '../../../core/tenant/tenant.service';
+import { ExportColumn } from '../../../shared/utils/export.models';
+import { ExportService } from '../../../shared/utils/export.service';
+
+@Component({
+  selector: 'app-linha-onibus-list',
+  standalone: true,
+  imports: [
+    ReactiveFormsModule,
+    RouterLink,
+    DecimalPipe,
+    MatButtonModule,
+    MatFormFieldModule,
+    MatIconModule,
+    MatInputModule,
+    MatProgressSpinnerModule,
+    MatSelectModule,
+    MatTableModule,
+    MatTooltipModule,
+  ],
+  templateUrl: './linha-onibus-list.component.html',
+  styleUrl: './linha-onibus-list.component.scss',
+})
+export class LinhaOnibusListComponent implements OnInit {
+  private readonly fb = inject(FormBuilder);
+  private readonly service = inject(LinhaOnibusService);
+  private readonly permissao = inject(PermissaoService);
+  private readonly exportService = inject(ExportService);
+  private readonly tenantService = inject(TenantService);
+  private readonly breakpointObserver = inject(BreakpointObserver);
+  private readonly destroyRef = inject(DestroyRef);
+
+  readonly carregando = signal(false);
+  readonly dataSource = signal<LinhaOnibusDto[]>([]);
+  readonly displayedColumns = ['descricao', 'dataInicio', 'dataFim', 'valorTarifa', 'acoes'];
+  readonly podeCriar = this.permissao.possuiPermissao('linhas_onibus', 'criar');
+  readonly podeEditar = this.permissao.possuiPermissao('linhas_onibus', 'editar');
+  isMobile = false;
+
+  readonly filtro = this.fb.nonNullable.group({
+    descricao: [''],
+    vigencia: ['' as '' | 'vigentes'],
+  });
+
+  private readonly exportColumns: ExportColumn[] = [
+    { key: 'descricao', label: 'Descri├º├úo' },
+    { key: 'dataInicio', label: 'Data in├¡cio' },
+    {
+      key: 'dataFim',
+      label: 'Data fim',
+      format: (value) => (value ? String(value) : 'Em aberto'),
+    },
+    {
+      key: 'valorTarifa',
+      label: 'Tarifa',
+      format: (value) =>
+        Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }),
+    },
+  ];
+
+  constructor() {
+    this.breakpointObserver
+      .observe([Breakpoints.XSmall])
+      .pipe(takeUntilDestroyed(this.destroyRef))
+      .subscribe((result) => {
+        this.isMobile = result.matches;
+      });
+  }
+
+  ngOnInit(): void {
+    this.buscar();
+  }
+
+  buscar(): void {
+    this.carregando.set(true);
+    const v = this.filtro.getRawValue();
+    this.service
+      .filtrar({
+        descricao: v.descricao.trim() || undefined,
+        somenteVigentes: v.vigencia === 'vigentes' ? true : undefined,
+      })
+      .pipe(finalize(() => this.carregando.set(false)))
+      .subscribe({
+        next: (lista) => this.dataSource.set(lista),
+        error: () => this.dataSource.set([]),
+      });
+  }
+
+  limpar(): void {
+    this.filtro.reset({ descricao: '', vigencia: '' });
+    this.buscar();
+  }
+
+  exportExcel(): void {
+    const subdomain = this.tenantService.getSubdomain();
+    const filename = this.exportService.buildFilename('linhas-onibus', subdomain, 'xlsx');
+    void this.exportService.exportToExcel(this.dataSource(), this.exportColumns, filename);
+  }
+
+  exportPdf(): void {
+    const subdomain = this.tenantService.getSubdomain();
+    const filename = this.exportService.buildFilename('linhas-onibus', subdomain, 'pdf');
+    void this.exportService.exportToPdf(
+      this.dataSource(),
+      this.exportColumns,
+      filename,
+      'Linhas de ├önibus',
+    );
+  }
+}
diff --git a/src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs b/src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs
new file mode 100644
index 0000000..9a3b43a
--- /dev/null
+++ b/src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs
@@ -0,0 +1,174 @@
+´╗┐using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Application.Services;
+using Beneficios.Domain.Enums;
+using Microsoft.AspNetCore.Authorization;
+using Microsoft.AspNetCore.Mvc;
+using System.IdentityModel.Tokens.Jwt;
+using System.Security.Claims;
+
+namespace Beneficios.Api.Controllers;
+
+[ApiController]
+[Authorize]
+[Route("api/funcionario-linhas")]
+public class FuncionarioLinhasController : ControllerBase
+{
+    private const string CodigoMenu = "funcionario_linhas";
+
+    private readonly IFuncionarioLinhaService _service;
+    private readonly IPermissaoService _permissaoService;
+    private readonly ILogger<FuncionarioLinhasController> _logger;
+
+    public FuncionarioLinhasController(
+        IFuncionarioLinhaService service,
+        IPermissaoService permissaoService,
+        ILogger<FuncionarioLinhasController> logger)
+    {
+        _service = service;
+        _permissaoService = permissaoService;
+        _logger = logger;
+    }
+
+    [HttpGet]
+    public async Task<IActionResult> Filtrar(
+        [FromQuery] Guid? funcionarioId,
+        [FromQuery] bool? somenteVigentes)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
+                return denied;
+
+            var lista = await _service.FiltrarAsync(funcionarioId, somenteVigentes);
+            return Ok(lista);
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao listar v├¡nculos funcion├írio ├ù linhas");
+            return StatusCode(500, new { message = "Erro ao listar v├¡nculos funcion├írio ├ù linhas" });
+        }
+    }
+
+    [HttpGet("{id:guid}")]
+    public async Task<IActionResult> ObterPorId(Guid id)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
+                return denied;
+
+            var item = await _service.ObterPorIdAsync(id);
+            if (item is null)
+                return NotFound(new { message = FuncionarioLinhaService.MensagemVinculoNaoEncontrado });
+
+            return Ok(item);
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao obter v├¡nculo funcion├írio ├ù linha {Id}", id);
+            return StatusCode(500, new { message = "Erro ao obter v├¡nculo funcion├írio ├ù linha" });
+        }
+    }
+
+    [HttpPost]
+    public async Task<IActionResult> Criar([FromBody] FuncionarioLinhaSalvarDto dto)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
+                return denied;
+
+            var id = await _service.SalvarAsync(dto, GetUsuarioIdFromToken());
+            return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (InvalidOperationException ex)
+        {
+            if (ex.Message.Contains("n├úo encontrad", StringComparison.OrdinalIgnoreCase))
+                return NotFound(new { message = ex.Message });
+            return BadRequest(new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao criar v├¡nculo funcion├írio ├ù linha");
+            return StatusCode(500, new { message = "Erro ao criar v├¡nculo funcion├írio ├ù linha" });
+        }
+    }
+
+    [HttpPut("{id:guid}")]
+    public async Task<IActionResult> Atualizar(Guid id, [FromBody] FuncionarioLinhaAtualizarDto dto)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
+                return denied;
+
+            await _service.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
+            return NoContent();
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (InvalidOperationException ex)
+        {
+            if (ex.Message.Contains("n├úo encontrad", StringComparison.OrdinalIgnoreCase))
+                return NotFound(new { message = ex.Message });
+            return BadRequest(new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao atualizar v├¡nculo funcion├írio ├ù linha {Id}", id);
+            return StatusCode(500, new { message = "Erro ao atualizar v├¡nculo funcion├írio ├ù linha" });
+        }
+    }
+
+    private async Task<IActionResult?> DenyIfUnauthorizedAsync(AcaoPermissao acao)
+    {
+        if (IsAdminTenant())
+            return null;
+
+        var usuarioId = GetUsuarioIdFromToken()
+            ?? throw new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo.");
+
+        await _permissaoService.GarantirPermissaoAsync(usuarioId, CodigoMenu, acao);
+        return null;
+    }
+
+    private bool IsAdminTenant()
+    {
+        if (HttpContext?.Request is null)
+            return true;
+
+        var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
+            ?? ExtractSubdomain(Request.Host.Host);
+        return string.Equals(tenant, "admin", StringComparison.OrdinalIgnoreCase);
+    }
+
+    private Guid? GetUsuarioIdFromToken()
+    {
+        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
+            ?? User.FindFirst(ClaimTypes.NameIdentifier);
+        return Guid.TryParse(claim?.Value, out var id) ? id : null;
+    }
+
+    private static string ExtractSubdomain(string host)
+    {
+        if (string.IsNullOrWhiteSpace(host))
+            return "admin";
+        var parts = host.Split('.');
+        return parts.Length < 2 ? "admin" : parts[0];
+    }
+}
diff --git a/src/Beneficios.Api/Controllers/LinhasOnibusController.cs b/src/Beneficios.Api/Controllers/LinhasOnibusController.cs
new file mode 100644
index 0000000..c36512f
--- /dev/null
+++ b/src/Beneficios.Api/Controllers/LinhasOnibusController.cs
@@ -0,0 +1,174 @@
+´╗┐using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Application.Services;
+using Beneficios.Domain.Enums;
+using Microsoft.AspNetCore.Authorization;
+using Microsoft.AspNetCore.Mvc;
+using System.IdentityModel.Tokens.Jwt;
+using System.Security.Claims;
+
+namespace Beneficios.Api.Controllers;
+
+[ApiController]
+[Authorize]
+[Route("api/linhas-onibus")]
+public class LinhasOnibusController : ControllerBase
+{
+    private const string CodigoMenu = "linhas_onibus";
+
+    private readonly ILinhaOnibusService _service;
+    private readonly IPermissaoService _permissaoService;
+    private readonly ILogger<LinhasOnibusController> _logger;
+
+    public LinhasOnibusController(
+        ILinhaOnibusService service,
+        IPermissaoService permissaoService,
+        ILogger<LinhasOnibusController> logger)
+    {
+        _service = service;
+        _permissaoService = permissaoService;
+        _logger = logger;
+    }
+
+    [HttpGet]
+    public async Task<IActionResult> Filtrar(
+        [FromQuery] string? descricao,
+        [FromQuery] bool? somenteVigentes)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
+                return denied;
+
+            var lista = await _service.FiltrarAsync(descricao, somenteVigentes);
+            return Ok(lista);
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao listar linhas de ├┤nibus");
+            return StatusCode(500, new { message = "Erro ao listar linhas de ├┤nibus" });
+        }
+    }
+
+    [HttpGet("{id:guid}")]
+    public async Task<IActionResult> ObterPorId(Guid id)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
+                return denied;
+
+            var item = await _service.ObterPorIdAsync(id);
+            if (item is null)
+                return NotFound(new { message = LinhaOnibusService.MensagemNaoEncontrada });
+
+            return Ok(item);
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao obter linha de ├┤nibus {Id}", id);
+            return StatusCode(500, new { message = "Erro ao obter linha de ├┤nibus" });
+        }
+    }
+
+    [HttpPost]
+    public async Task<IActionResult> Criar([FromBody] LinhaOnibusSalvarDto dto)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
+                return denied;
+
+            var id = await _service.SalvarAsync(dto, GetUsuarioIdFromToken());
+            return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (InvalidOperationException ex)
+        {
+            if (ex.Message.Contains("n├úo encontrad", StringComparison.OrdinalIgnoreCase))
+                return NotFound(new { message = ex.Message });
+            return BadRequest(new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao criar linha de ├┤nibus");
+            return StatusCode(500, new { message = "Erro ao criar linha de ├┤nibus" });
+        }
+    }
+
+    [HttpPut("{id:guid}")]
+    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LinhaOnibusAtualizarDto dto)
+    {
+        try
+        {
+            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
+                return denied;
+
+            await _service.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
+            return NoContent();
+        }
+        catch (UnauthorizedAccessException ex)
+        {
+            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
+        }
+        catch (InvalidOperationException ex)
+        {
+            if (ex.Message.Contains("n├úo encontrad", StringComparison.OrdinalIgnoreCase))
+                return NotFound(new { message = ex.Message });
+            return BadRequest(new { message = ex.Message });
+        }
+        catch (Exception ex)
+        {
+            _logger.LogError(ex, "Erro ao atualizar linha de ├┤nibus {Id}", id);
+            return StatusCode(500, new { message = "Erro ao atualizar linha de ├┤nibus" });
+        }
+    }
+
+    private async Task<IActionResult?> DenyIfUnauthorizedAsync(AcaoPermissao acao)
+    {
+        if (IsAdminTenant())
+            return null;
+
+        var usuarioId = GetUsuarioIdFromToken()
+            ?? throw new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo.");
+
+        await _permissaoService.GarantirPermissaoAsync(usuarioId, CodigoMenu, acao);
+        return null;
+    }
+
+    private bool IsAdminTenant()
+    {
+        if (HttpContext?.Request is null)
+            return true;
+
+        var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
+            ?? ExtractSubdomain(Request.Host.Host);
+        return string.Equals(tenant, "admin", StringComparison.OrdinalIgnoreCase);
+    }
+
+    private Guid? GetUsuarioIdFromToken()
+    {
+        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
+            ?? User.FindFirst(ClaimTypes.NameIdentifier);
+        return Guid.TryParse(claim?.Value, out var id) ? id : null;
+    }
+
+    private static string ExtractSubdomain(string host)
+    {
+        if (string.IsNullOrWhiteSpace(host))
+            return "admin";
+        var parts = host.Split('.');
+        return parts.Length < 2 ? "admin" : parts[0];
+    }
+}
diff --git a/src/Beneficios.Api/Program.cs b/src/Beneficios.Api/Program.cs
index f82ce7a..5331776 100644
--- a/src/Beneficios.Api/Program.cs
+++ b/src/Beneficios.Api/Program.cs
@@ -72,16 +72,20 @@ builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
 builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
 builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
 builder.Services.AddScoped<ICalendarioDiaRepository, CalendarioDiaRepository>();
 builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
 builder.Services.AddScoped<IFuncionarioAfastamentoRepository, FuncionarioAfastamentoRepository>();
+builder.Services.AddScoped<ILinhaOnibusRepository, LinhaOnibusRepository>();
+builder.Services.AddScoped<IFuncionarioLinhaRepository, FuncionarioLinhaRepository>();
 builder.Services.AddScoped<IUsuarioService, UsuarioService>();
 builder.Services.AddScoped<IEmpresaService, EmpresaService>();
 builder.Services.AddScoped<IPerfilService, PerfilService>();
 builder.Services.AddScoped<ICalendarioDiaService, CalendarioDiaService>();
 builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
 builder.Services.AddScoped<IFuncionarioAfastamentoService, FuncionarioAfastamentoService>();
+builder.Services.AddScoped<ILinhaOnibusService, LinhaOnibusService>();
+builder.Services.AddScoped<IFuncionarioLinhaService, FuncionarioLinhaService>();
 builder.Services.AddScoped<IPermissaoService, PermissaoService>();
 builder.Services.AddScoped<IEmailService, EmailService>();
 builder.Services.AddScoped<ICalendarioAnoGarantia, CalendarioAnoGarantia>();
 builder.Services.AddHostedService<CalendarioAnoHostedService>();
 
diff --git a/src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs b/src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs
new file mode 100644
index 0000000..df6c060
--- /dev/null
+++ b/src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs
@@ -0,0 +1,13 @@
+´╗┐namespace Beneficios.Application.DTOs;
+
+public record FuncionarioLinhaDto(
+    Guid Id, Guid FuncionarioId, string FuncionarioNome,
+    Guid LinhaOnibusId, string LinhaDescricao,
+    int Quantidade, DateOnly DataInicio, DateOnly? DataFim);
+
+public record FuncionarioLinhaSalvarDto(
+    Guid FuncionarioId, Guid LinhaOnibusId, int Quantidade,
+    DateOnly DataInicio, DateOnly? DataFim);
+
+public record FuncionarioLinhaAtualizarDto(
+    Guid LinhaOnibusId, int Quantidade, DateOnly DataInicio, DateOnly? DataFim);
diff --git a/src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs b/src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs
new file mode 100644
index 0000000..c494016
--- /dev/null
+++ b/src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs
@@ -0,0 +1,5 @@
+´╗┐namespace Beneficios.Application.DTOs;
+
+public record LinhaOnibusDto(Guid Id, string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
+public record LinhaOnibusSalvarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
+public record LinhaOnibusAtualizarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
\ No newline at end of file
diff --git a/src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs b/src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs
new file mode 100644
index 0000000..f86f9ac
--- /dev/null
+++ b/src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs
@@ -0,0 +1,12 @@
+´╗┐using Beneficios.Application.DTOs;
+
+namespace Beneficios.Application.Interfaces;
+
+public interface IFuncionarioLinhaService
+{
+    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId);
+    Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId);
+    Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id);
+    Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes);
+    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId);
+}
diff --git a/src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs b/src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs
new file mode 100644
index 0000000..ca99991
--- /dev/null
+++ b/src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs
@@ -0,0 +1,11 @@
+´╗┐using Beneficios.Application.DTOs;
+
+namespace Beneficios.Application.Interfaces;
+
+public interface ILinhaOnibusService
+{
+    Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId);
+    Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId);
+    Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id);
+    Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(string? descricao, bool? somenteVigentes);
+}
\ No newline at end of file
diff --git a/src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs b/src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs
new file mode 100644
index 0000000..eeec2f4
--- /dev/null
+++ b/src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs
@@ -0,0 +1,13 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Domain.Models;
+
+namespace Beneficios.Application.Mappings;
+
+public class FuncionarioLinhaProfile : Profile
+{
+    public FuncionarioLinhaProfile()
+    {
+        CreateMap<FuncionarioLinhaQueryResult, FuncionarioLinhaDto>();
+    }
+}
diff --git a/src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs b/src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs
new file mode 100644
index 0000000..85b2a3a
--- /dev/null
+++ b/src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs
@@ -0,0 +1,13 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Domain.Models;
+
+namespace Beneficios.Application.Mappings;
+
+public class LinhaOnibusProfile : Profile
+{
+    public LinhaOnibusProfile()
+    {
+        CreateMap<LinhaOnibusQueryResult, LinhaOnibusDto>();
+    }
+}
\ No newline at end of file
diff --git a/src/Beneficios.Application/Services/FuncionarioLinhaService.cs b/src/Beneficios.Application/Services/FuncionarioLinhaService.cs
new file mode 100644
index 0000000..eb81f47
--- /dev/null
+++ b/src/Beneficios.Application/Services/FuncionarioLinhaService.cs
@@ -0,0 +1,150 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Domain;
+using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+
+namespace Beneficios.Application.Services;
+
+public class FuncionarioLinhaService : IFuncionarioLinhaService
+{
+    public const string MensagemVtInativo = "Funcion├írio sem vale transporte ativo; n├úo ├® poss├¡vel vincular linhas.";
+    public const string MensagemLinhaNaoVigente = "A linha selecionada n├úo est├í vigente na data de in├¡cio do v├¡nculo.";
+    public const string MensagemParDuplicado = "Esta linha j├í est├í vinculada a este funcion├írio.";
+    public const string MensagemQuantidade = "A quantidade de utiliza├º├Áes deve ser maior ou igual a 1.";
+    public const string MensagemDataFim = "A data fim deve ser maior ou igual ├á data in├¡cio.";
+    public const string MensagemVinculoNaoEncontrado = "V├¡nculo n├úo encontrado.";
+    public const string MensagemFuncionarioNaoEncontrado = "Funcion├írio n├úo encontrado.";
+    public const string MensagemLinhaNaoEncontrada = "Linha n├úo encontrada.";
+    public const string CodigoValeTransporte = "vale_transporte";
+
+    private readonly IFuncionarioLinhaRepository _vinculoRepository;
+    private readonly ILinhaOnibusRepository _linhaRepository;
+    private readonly IFuncionarioRepository _funcionarioRepository;
+    private readonly IMapper _mapper;
+
+    public FuncionarioLinhaService(
+        IFuncionarioLinhaRepository vinculoRepository,
+        ILinhaOnibusRepository linhaRepository,
+        IFuncionarioRepository funcionarioRepository,
+        IMapper mapper)
+    {
+        _vinculoRepository = vinculoRepository;
+        _linhaRepository = linhaRepository;
+        _funcionarioRepository = funcionarioRepository;
+        _mapper = mapper;
+    }
+
+    public async Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId)
+    {
+        Validar(dto.DataInicio, dto.DataFim, dto.Quantidade);
+        await GarantirFuncionarioComVtAsync(dto.FuncionarioId);
+        await GarantirLinhaVigenteAsync(dto.LinhaOnibusId, dto.DataInicio);
+
+        if (await _vinculoRepository.ExisteParAsync(dto.FuncionarioId, dto.LinhaOnibusId))
+            throw new InvalidOperationException(MensagemParDuplicado);
+
+        var id = Guid.NewGuid();
+        await _vinculoRepository.SalvarAsync(new FuncionarioLinhaSalvarParams
+        {
+            Id = id,
+            FuncionarioId = dto.FuncionarioId,
+            LinhaOnibusId = dto.LinhaOnibusId,
+            Quantidade = dto.Quantidade,
+            DataInicio = dto.DataInicio,
+            DataFim = dto.DataFim,
+            DataInclusao = DateTime.UtcNow,
+            UsuarioAlteracaoId = usuarioAlteracaoId,
+        });
+        return id;
+    }
+
+    public async Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId)
+    {
+        Validar(dto.DataInicio, dto.DataFim, dto.Quantidade);
+
+        var existente = await _vinculoRepository.ObterPorIdAsync(id)
+            ?? throw new InvalidOperationException(MensagemVinculoNaoEncontrado);
+
+        await GarantirFuncionarioComVtAsync(existente.FuncionarioId);
+        await GarantirLinhaVigenteAsync(dto.LinhaOnibusId, dto.DataInicio);
+
+        if (await _vinculoRepository.ExisteParAsync(existente.FuncionarioId, dto.LinhaOnibusId, id))
+            throw new InvalidOperationException(MensagemParDuplicado);
+
+        await _vinculoRepository.AtualizarAsync(new FuncionarioLinhaAtualizarParams
+        {
+            Id = id,
+            LinhaOnibusId = dto.LinhaOnibusId,
+            Quantidade = dto.Quantidade,
+            DataInicio = dto.DataInicio,
+            DataFim = dto.DataFim,
+            DataAlteracao = DateTime.UtcNow,
+            UsuarioAlteracaoId = usuarioAlteracaoId,
+        });
+    }
+
+    public async Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id)
+    {
+        var entity = await _vinculoRepository.ObterPorIdAsync(id);
+        return entity is null ? null : _mapper.Map<FuncionarioLinhaDto>(entity);
+    }
+
+    public async Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes)
+    {
+        var lista = await _vinculoRepository.FiltrarAsync(new FuncionarioLinhaFiltroParams
+        {
+            FuncionarioId = funcionarioId,
+            SomenteVigentes = somenteVigentes,
+            Referencia = somenteVigentes == true ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
+        });
+
+        IEnumerable<FuncionarioLinhaQueryResult> filtrada = lista;
+        if (somenteVigentes == true)
+        {
+            var referencia = DateOnly.FromDateTime(DateTime.UtcNow);
+            filtrada = lista.Where(v => AfastamentoPeriodo.EstaAtivoEm(v.DataInicio, v.DataFim, referencia));
+        }
+
+        return _mapper.Map<List<FuncionarioLinhaDto>>(filtrada.ToList());
+    }
+
+    public Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId)
+        => _vinculoRepository.EncerrarAbertosPorFuncionarioAsync(
+            funcionarioId,
+            DateOnly.FromDateTime(DateTime.UtcNow),
+            DateTime.UtcNow,
+            usuarioAlteracaoId);
+
+    private async Task GarantirFuncionarioComVtAsync(Guid funcionarioId)
+    {
+        _ = await _funcionarioRepository.ObterPorIdAsync(funcionarioId)
+            ?? throw new InvalidOperationException(MensagemFuncionarioNaoEncontrado);
+
+        var beneficios = await _funcionarioRepository.ObterBeneficiosAsync(funcionarioId);
+        var vtAtivo = beneficios.Any(b =>
+            string.Equals(b.CodigoBeneficio, CodigoValeTransporte, StringComparison.OrdinalIgnoreCase)
+            && b.Ativo);
+
+        if (!vtAtivo)
+            throw new InvalidOperationException(MensagemVtInativo);
+    }
+
+    private async Task GarantirLinhaVigenteAsync(Guid linhaOnibusId, DateOnly dataInicioVinculo)
+    {
+        var linha = await _linhaRepository.ObterPorIdAsync(linhaOnibusId)
+            ?? throw new InvalidOperationException(MensagemLinhaNaoEncontrada);
+
+        if (!AfastamentoPeriodo.EstaAtivoEm(linha.DataInicio, linha.DataFim, dataInicioVinculo))
+            throw new InvalidOperationException(MensagemLinhaNaoVigente);
+    }
+
+    private static void Validar(DateOnly dataInicio, DateOnly? dataFim, int quantidade)
+    {
+        if (quantidade < 1)
+            throw new InvalidOperationException(MensagemQuantidade);
+        if (dataFim is not null && dataFim < dataInicio)
+            throw new InvalidOperationException(MensagemDataFim);
+    }
+}
diff --git a/src/Beneficios.Application/Services/FuncionarioService.cs b/src/Beneficios.Application/Services/FuncionarioService.cs
index a29d820..0ac5011 100644
--- a/src/Beneficios.Application/Services/FuncionarioService.cs
+++ b/src/Beneficios.Application/Services/FuncionarioService.cs
@@ -21,19 +21,22 @@ public class FuncionarioService : IFuncionarioService
     public const string MensagemAtivoComAfastamento = "Existe afastamento ativo; encerre ou exclua o per├¡odo antes de marcar como ativo.";
     public const string CodigoValeTransporte = "vale_transporte";
 
     private readonly IFuncionarioRepository _repository;
     private readonly IFuncionarioAfastamentoRepository _afastamentoRepository;
+    private readonly IFuncionarioLinhaService _funcionarioLinhaService;
     private readonly IMapper _mapper;
 
     public FuncionarioService(
         IFuncionarioRepository repository,
         IFuncionarioAfastamentoRepository afastamentoRepository,
+        IFuncionarioLinhaService funcionarioLinhaService,
         IMapper mapper)
     {
         _repository = repository;
         _afastamentoRepository = afastamentoRepository;
+        _funcionarioLinhaService = funcionarioLinhaService;
         _mapper = mapper;
     }
 
     public async Task<Guid> SalvarAsync(FuncionarioSalvarDto dto, Guid? usuarioAlteracaoId)
     {
@@ -48,10 +51,11 @@ public class FuncionarioService : IFuncionarioService
         if (matricula is not null && await _repository.MatriculaExisteAsync(matricula))
             throw new InvalidOperationException(MensagemMatriculaDuplicada);
 
         var id = Guid.NewGuid();
         await _repository.SalvarAsync(MapearSalvar(dto, id, cpf, matricula), beneficios);
+        await EncerrarVinculosSeVtInativoAsync(id, beneficios, usuarioAlteracaoId);
         return id;
     }
 
     public async Task AtualizarAsync(Guid id, FuncionarioAtualizarDto dto, Guid? usuarioAlteracaoId)
     {
@@ -68,10 +72,11 @@ public class FuncionarioService : IFuncionarioService
             throw new InvalidOperationException(MensagemCpfDuplicado);
         if (matricula is not null && await _repository.MatriculaExisteAsync(matricula, id))
             throw new InvalidOperationException(MensagemMatriculaDuplicada);
 
         await _repository.AtualizarAsync(MapearAtualizar(dto, id, cpf, matricula, usuarioAlteracaoId), beneficios);
+        await EncerrarVinculosSeVtInativoAsync(id, beneficios, usuarioAlteracaoId);
     }
 
     public async Task<FuncionarioDto?> ObterPorIdAsync(Guid id)
     {
         var entity = await _repository.ObterPorIdAsync(id);
@@ -136,10 +141,23 @@ public class FuncionarioService : IFuncionarioService
             funcionarioId.Value, DateOnly.FromDateTime(DateTime.UtcNow));
         if (ativo is not null)
             throw new InvalidOperationException(MensagemAtivoComAfastamento);
     }
 
+    private async Task EncerrarVinculosSeVtInativoAsync(
+        Guid funcionarioId,
+        IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios,
+        Guid? usuarioAlteracaoId)
+    {
+        var vtAtivo = beneficios.Any(b =>
+            string.Equals(b.CodigoBeneficio, CodigoValeTransporte, StringComparison.OrdinalIgnoreCase)
+            && b.Ativo);
+
+        if (!vtAtivo)
+            await _funcionarioLinhaService.EncerrarAbertosPorFuncionarioAsync(funcionarioId, usuarioAlteracaoId);
+    }
+
     private static IReadOnlyList<FuncionarioBeneficioSalvarParams> NormalizarBeneficios(
         IReadOnlyList<FuncionarioBeneficioSalvarDto>? beneficios)
     {
         if (beneficios is null || beneficios.Count == 0)
             return Array.Empty<FuncionarioBeneficioSalvarParams>();
diff --git a/src/Beneficios.Application/Services/LinhaOnibusService.cs b/src/Beneficios.Application/Services/LinhaOnibusService.cs
new file mode 100644
index 0000000..d63e34d
--- /dev/null
+++ b/src/Beneficios.Application/Services/LinhaOnibusService.cs
@@ -0,0 +1,103 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Domain;
+using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+
+namespace Beneficios.Application.Services;
+
+public class LinhaOnibusService : ILinhaOnibusService
+{
+    public const string MensagemDataFim = "A data fim deve ser maior ou igual ├á data in├¡cio.";
+    public const string MensagemNaoEncontrada = "Linha n├úo encontrada.";
+    public const string MensagemVinculosAbertos = "Existem v├¡nculos em aberto para esta linha; encerre-os antes de finalizar a vig├¬ncia.";
+    public const string MensagemTarifa = "O valor da tarifa deve ser maior ou igual a zero.";
+
+    private readonly ILinhaOnibusRepository _linhaRepository;
+    private readonly IFuncionarioLinhaRepository _vinculoRepository;
+    private readonly IMapper _mapper;
+
+    public LinhaOnibusService(
+        ILinhaOnibusRepository linhaRepository,
+        IFuncionarioLinhaRepository vinculoRepository,
+        IMapper mapper)
+    {
+        _linhaRepository = linhaRepository;
+        _vinculoRepository = vinculoRepository;
+        _mapper = mapper;
+    }
+
+    public async Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId)
+    {
+        Validar(dto.DataInicio, dto.DataFim, dto.ValorTarifa);
+
+        var id = Guid.NewGuid();
+        await _linhaRepository.SalvarAsync(new LinhaOnibusSalvarParams
+        {
+            Id = id,
+            Descricao = dto.Descricao.Trim(),
+            DataInicio = dto.DataInicio,
+            DataFim = dto.DataFim,
+            ValorTarifa = dto.ValorTarifa,
+            DataInclusao = DateTime.UtcNow,
+            UsuarioAlteracaoId = usuarioAlteracaoId,
+        });
+        return id;
+    }
+
+    public async Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId)
+    {
+        Validar(dto.DataInicio, dto.DataFim, dto.ValorTarifa);
+
+        _ = await _linhaRepository.ObterPorIdAsync(id)
+            ?? throw new InvalidOperationException(MensagemNaoEncontrada);
+
+        if (dto.DataFim is not null && await _vinculoRepository.ExisteVinculoAbertoPorLinhaAsync(id))
+            throw new InvalidOperationException(MensagemVinculosAbertos);
+
+        await _linhaRepository.AtualizarAsync(new LinhaOnibusAtualizarParams
+        {
+            Id = id,
+            Descricao = dto.Descricao.Trim(),
+            DataInicio = dto.DataInicio,
+            DataFim = dto.DataFim,
+            ValorTarifa = dto.ValorTarifa,
+            DataAlteracao = DateTime.UtcNow,
+            UsuarioAlteracaoId = usuarioAlteracaoId,
+        });
+    }
+
+    public async Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id)
+    {
+        var entity = await _linhaRepository.ObterPorIdAsync(id);
+        return entity is null ? null : _mapper.Map<LinhaOnibusDto>(entity);
+    }
+
+    public async Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(string? descricao, bool? somenteVigentes)
+    {
+        var lista = await _linhaRepository.FiltrarAsync(new LinhaOnibusFiltroParams
+        {
+            Descricao = descricao,
+            SomenteVigentes = somenteVigentes,
+            Referencia = somenteVigentes == true ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
+        });
+
+        IEnumerable<LinhaOnibusQueryResult> filtrada = lista;
+        if (somenteVigentes == true)
+        {
+            var referencia = DateOnly.FromDateTime(DateTime.UtcNow);
+            filtrada = lista.Where(l => AfastamentoPeriodo.EstaAtivoEm(l.DataInicio, l.DataFim, referencia));
+        }
+
+        return _mapper.Map<List<LinhaOnibusDto>>(filtrada.ToList());
+    }
+
+    private static void Validar(DateOnly dataInicio, DateOnly? dataFim, decimal valorTarifa)
+    {
+        if (dataFim is not null && dataFim < dataInicio)
+            throw new InvalidOperationException(MensagemDataFim);
+        if (valorTarifa < 0)
+            throw new InvalidOperationException(MensagemTarifa);
+    }
+}
\ No newline at end of file
diff --git a/src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs b/src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs
new file mode 100644
index 0000000..ee93d71
--- /dev/null
+++ b/src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs
@@ -0,0 +1,14 @@
+´╗┐using Beneficios.Domain.Models;
+
+namespace Beneficios.Domain.Interfaces;
+
+public interface IFuncionarioLinhaRepository
+{
+    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros);
+    Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros);
+    Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id);
+    Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro);
+    Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null);
+    Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId);
+    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId);
+}
diff --git a/src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs b/src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs
new file mode 100644
index 0000000..06c00ff
--- /dev/null
+++ b/src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs
@@ -0,0 +1,11 @@
+´╗┐using Beneficios.Domain.Models;
+
+namespace Beneficios.Domain.Interfaces;
+
+public interface ILinhaOnibusRepository
+{
+    Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros);
+    Task AtualizarAsync(LinhaOnibusAtualizarParams parametros);
+    Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id);
+    Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro);
+}
diff --git a/src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs b/src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs
new file mode 100644
index 0000000..85aed2a
--- /dev/null
+++ b/src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs
@@ -0,0 +1,12 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class FuncionarioLinhaAtualizarParams
+{
+    public Guid Id { get; init; }
+    public Guid LinhaOnibusId { get; init; }
+    public int Quantidade { get; init; }
+    public DateOnly DataInicio { get; init; }
+    public DateOnly? DataFim { get; init; }
+    public DateTime DataAlteracao { get; init; }
+    public Guid? UsuarioAlteracaoId { get; init; }
+}
diff --git a/src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs b/src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs
new file mode 100644
index 0000000..8a67d2c
--- /dev/null
+++ b/src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs
@@ -0,0 +1,8 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class FuncionarioLinhaFiltroParams
+{
+    public Guid? FuncionarioId { get; init; }
+    public bool? SomenteVigentes { get; init; }
+    public DateOnly? Referencia { get; init; }
+}
diff --git a/src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs b/src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs
new file mode 100644
index 0000000..10fe7eb
--- /dev/null
+++ b/src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs
@@ -0,0 +1,15 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class FuncionarioLinhaQueryResult
+{
+    public Guid Id { get; set; }
+    public Guid FuncionarioId { get; set; }
+    public string FuncionarioNome { get; set; } = string.Empty;
+    public Guid LinhaOnibusId { get; set; }
+    public string LinhaDescricao { get; set; } = string.Empty;
+    public int Quantidade { get; set; }
+    public DateOnly DataInicio { get; set; }
+    public DateOnly? DataFim { get; set; }
+    public DateTime DataInclusao { get; set; }
+    public DateTime? DataAlteracao { get; set; }
+}
diff --git a/src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs b/src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs
new file mode 100644
index 0000000..95d568c
--- /dev/null
+++ b/src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs
@@ -0,0 +1,13 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class FuncionarioLinhaSalvarParams
+{
+    public Guid Id { get; init; }
+    public Guid FuncionarioId { get; init; }
+    public Guid LinhaOnibusId { get; init; }
+    public int Quantidade { get; init; }
+    public DateOnly DataInicio { get; init; }
+    public DateOnly? DataFim { get; init; }
+    public DateTime DataInclusao { get; init; }
+    public Guid? UsuarioAlteracaoId { get; init; }
+}
diff --git a/src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs b/src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs
new file mode 100644
index 0000000..6a05f29
--- /dev/null
+++ b/src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs
@@ -0,0 +1,12 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class LinhaOnibusAtualizarParams
+{
+    public Guid Id { get; init; }
+    public string Descricao { get; init; } = string.Empty;
+    public DateOnly DataInicio { get; init; }
+    public DateOnly? DataFim { get; init; }
+    public decimal ValorTarifa { get; init; }
+    public DateTime DataAlteracao { get; init; }
+    public Guid? UsuarioAlteracaoId { get; init; }
+}
diff --git a/src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs b/src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs
new file mode 100644
index 0000000..a48660d
--- /dev/null
+++ b/src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs
@@ -0,0 +1,8 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class LinhaOnibusFiltroParams
+{
+    public string? Descricao { get; init; }
+    public bool? SomenteVigentes { get; init; }
+    public DateOnly? Referencia { get; init; } // default hoje no service se SomenteVigentes
+}
diff --git a/src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs b/src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs
new file mode 100644
index 0000000..8cac627
--- /dev/null
+++ b/src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs
@@ -0,0 +1,12 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class LinhaOnibusQueryResult
+{
+    public Guid Id { get; set; }
+    public string Descricao { get; set; } = string.Empty;
+    public DateOnly DataInicio { get; set; }
+    public DateOnly? DataFim { get; set; }
+    public decimal ValorTarifa { get; set; }
+    public DateTime DataInclusao { get; set; }
+    public DateTime? DataAlteracao { get; set; }
+}
diff --git a/src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs b/src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs
new file mode 100644
index 0000000..e8463fb
--- /dev/null
+++ b/src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs
@@ -0,0 +1,12 @@
+´╗┐namespace Beneficios.Domain.Models;
+
+public sealed class LinhaOnibusSalvarParams
+{
+    public Guid Id { get; init; }
+    public string Descricao { get; init; } = string.Empty;
+    public DateOnly DataInicio { get; init; }
+    public DateOnly? DataFim { get; init; }
+    public decimal ValorTarifa { get; init; }
+    public DateTime DataInclusao { get; init; }
+    public Guid? UsuarioAlteracaoId { get; init; }
+}
diff --git a/src/Beneficios.Domain/ModulosSistemaCatalog.cs b/src/Beneficios.Domain/ModulosSistemaCatalog.cs
index d1da258..00e4d71 100644
--- a/src/Beneficios.Domain/ModulosSistemaCatalog.cs
+++ b/src/Beneficios.Domain/ModulosSistemaCatalog.cs
@@ -14,10 +14,12 @@ public static class ModulosSistemaCatalog
         new("usuarios", "Usu├írios", "/usuarios", AcaoPermissao.Todas),
         new("perfis", "Perfis", "/perfis", AcaoPermissao.Todas),
         new("calendario", "Calend├írio", "/calendario", AcaoPermissao.Todas),
         new("funcionarios", "Funcion├írios", "/funcionarios", AcaoPermissao.Todas),
         new("afastamentos", "Afastamentos/F├®rias", "/afastamentos", AcaoPermissao.Todas),
+        new("linhas_onibus", "Linhas de ├önibus", "/linhas-onibus", AcaoPermissao.Todas),
+        new("funcionario_linhas", "Funcion├írio ├ù Linhas", "/funcionario-linhas", AcaoPermissao.Todas),
     ];
 
     public static ModuloSistema? ObterPorCodigo(string codigo) =>
         Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
 }
diff --git a/src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs b/src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs
new file mode 100644
index 0000000..d1132de
--- /dev/null
+++ b/src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs
@@ -0,0 +1,223 @@
+´╗┐using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+using Dapper;
+using Npgsql;
+using System.Data;
+using System.Text;
+
+namespace Beneficios.Infrastructure.Repositories;
+
+public class FuncionarioLinhaRepository : IFuncionarioLinhaRepository
+{
+    private readonly IDbConnection _dbConnection;
+
+    public FuncionarioLinhaRepository(IDbConnection dbConnection)
+    {
+        _dbConnection = dbConnection;
+    }
+
+    public async Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros)
+    {
+        EnsureOpen();
+        const string sql = """
+            INSERT INTO funcionario_linhas (
+                id, funcionario_id, linha_onibus_id, quantidade,
+                data_inicio, data_fim, data_inclusao, usuario_alteracao_id)
+            VALUES (
+                @Id, @FuncionarioId, @LinhaOnibusId, @Quantidade,
+                @DataInicio, @DataFim, @DataInclusao, @UsuarioAlteracaoId)
+            """;
+        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros));
+        return parametros.Id;
+    }
+
+    public async Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros)
+    {
+        EnsureOpen();
+        const string sql = """
+            UPDATE funcionario_linhas SET
+                linha_onibus_id = @LinhaOnibusId,
+                quantidade = @Quantidade,
+                data_inicio = @DataInicio,
+                data_fim = @DataFim,
+                data_alteracao = @DataAlteracao,
+                usuario_alteracao_id = @UsuarioAlteracaoId
+            WHERE id = @Id
+            """;
+        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros));
+        if (rows == 0)
+            throw new InvalidOperationException("V├¡nculo n├úo encontrado.");
+    }
+
+    public async Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id)
+    {
+        const string sql = """
+            SELECT fl.id AS Id, fl.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
+                fl.linha_onibus_id AS LinhaOnibusId, l.descricao AS LinhaDescricao,
+                fl.quantidade AS Quantidade,
+                fl.data_inicio AS DataInicioObj, fl.data_fim AS DataFimObj,
+                fl.data_inclusao AS DataInclusao, fl.data_alteracao AS DataAlteracao
+            FROM funcionario_linhas fl
+            INNER JOIN funcionarios f ON f.id = fl.funcionario_id
+            INNER JOIN linhas_onibus l ON l.id = fl.linha_onibus_id
+            WHERE fl.id = @Id
+            """;
+        var row = await _dbConnection.QueryFirstOrDefaultAsync<VinculoRow>(sql, new { Id = id });
+        return row is null ? null : Mapear(row);
+    }
+
+    public async Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro)
+    {
+        var sql = new StringBuilder("""
+            SELECT fl.id AS Id, fl.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
+                fl.linha_onibus_id AS LinhaOnibusId, l.descricao AS LinhaDescricao,
+                fl.quantidade AS Quantidade,
+                fl.data_inicio AS DataInicioObj, fl.data_fim AS DataFimObj,
+                fl.data_inclusao AS DataInclusao, fl.data_alteracao AS DataAlteracao
+            FROM funcionario_linhas fl
+            INNER JOIN funcionarios f ON f.id = fl.funcionario_id
+            INNER JOIN linhas_onibus l ON l.id = fl.linha_onibus_id
+            WHERE 1=1
+            """);
+
+        if (filtro.FuncionarioId is not null)
+            sql.Append(" AND fl.funcionario_id = @FuncionarioId");
+
+        if (filtro.SomenteVigentes == true && filtro.Referencia is not null)
+        {
+            sql.Append("""
+                 AND fl.data_inicio <= @Referencia
+                 AND (fl.data_fim IS NULL OR fl.data_fim >= @Referencia)
+                """);
+        }
+
+        sql.Append(" ORDER BY f.nome, l.descricao");
+
+        try
+        {
+            var rows = await _dbConnection.QueryAsync<VinculoRow>(sql.ToString(), new
+            {
+                filtro.FuncionarioId,
+                Referencia = filtro.Referencia?.ToDateTime(TimeOnly.MinValue),
+            });
+            return rows.Select(Mapear).ToArray();
+        }
+        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
+        {
+            return Array.Empty<FuncionarioLinhaQueryResult>();
+        }
+    }
+
+    public async Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null)
+    {
+        const string sql = """
+            SELECT EXISTS(
+                SELECT 1 FROM funcionario_linhas
+                WHERE funcionario_id = @FuncionarioId
+                  AND linha_onibus_id = @LinhaOnibusId
+                  AND (@ExcetoId IS NULL OR id <> @ExcetoId))
+            """;
+        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
+        {
+            FuncionarioId = funcionarioId,
+            LinhaOnibusId = linhaOnibusId,
+            ExcetoId = excetoId,
+        });
+    }
+
+    public async Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId)
+    {
+        const string sql = """
+            SELECT EXISTS(
+                SELECT 1 FROM funcionario_linhas
+                WHERE linha_onibus_id = @LinhaOnibusId
+                  AND data_fim IS NULL)
+            """;
+        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { LinhaOnibusId = linhaOnibusId });
+    }
+
+    public async Task EncerrarAbertosPorFuncionarioAsync(
+        Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId)
+    {
+        EnsureOpen();
+        const string sql = """
+            UPDATE funcionario_linhas
+            SET data_fim = @DataFim, data_alteracao = @DataAlteracao, usuario_alteracao_id = @UsuarioAlteracaoId
+            WHERE funcionario_id = @FuncionarioId AND data_fim IS NULL
+            """;
+        await _dbConnection.ExecuteAsync(sql, new
+        {
+            FuncionarioId = funcionarioId,
+            DataFim = dataFim.ToDateTime(TimeOnly.MinValue),
+            DataAlteracao = dataAlteracao,
+            UsuarioAlteracaoId = usuarioAlteracaoId,
+        });
+    }
+
+    private void EnsureOpen()
+    {
+        if (_dbConnection.State != ConnectionState.Open)
+            _dbConnection.Open();
+    }
+
+    private static object MapearInsert(FuncionarioLinhaSalvarParams p) => new
+    {
+        p.Id,
+        p.FuncionarioId,
+        p.LinhaOnibusId,
+        p.Quantidade,
+        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
+        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
+        p.DataInclusao,
+        p.UsuarioAlteracaoId,
+    };
+
+    private static object MapearUpdate(FuncionarioLinhaAtualizarParams p) => new
+    {
+        p.Id,
+        p.LinhaOnibusId,
+        p.Quantidade,
+        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
+        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
+        p.DataAlteracao,
+        p.UsuarioAlteracaoId,
+    };
+
+    private static FuncionarioLinhaQueryResult Mapear(VinculoRow row) => new()
+    {
+        Id = row.Id,
+        FuncionarioId = row.FuncionarioId,
+        FuncionarioNome = row.FuncionarioNome,
+        LinhaOnibusId = row.LinhaOnibusId,
+        LinhaDescricao = row.LinhaDescricao,
+        Quantidade = row.Quantidade,
+        DataInicio = ToDateOnly(row.DataInicioObj),
+        DataFim = ToDateOnlyNullable(row.DataFimObj),
+        DataInclusao = row.DataInclusao,
+        DataAlteracao = row.DataAlteracao,
+    };
+
+    private static DateOnly ToDateOnly(object valor) => valor switch
+    {
+        DateOnly d => d,
+        DateTime dt => DateOnly.FromDateTime(dt),
+        _ => DateOnly.FromDateTime(Convert.ToDateTime(valor)),
+    };
+
+    private static DateOnly? ToDateOnlyNullable(object? valor) =>
+        valor is null or DBNull ? null : ToDateOnly(valor);
+
+    private sealed class VinculoRow
+    {
+        public Guid Id { get; set; }
+        public Guid FuncionarioId { get; set; }
+        public string FuncionarioNome { get; set; } = "";
+        public Guid LinhaOnibusId { get; set; }
+        public string LinhaDescricao { get; set; } = "";
+        public int Quantidade { get; set; }
+        public object DataInicioObj { get; set; } = default!;
+        public object? DataFimObj { get; set; }
+        public DateTime DataInclusao { get; set; }
+        public DateTime? DataAlteracao { get; set; }
+    }
+}
diff --git a/src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs b/src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs
new file mode 100644
index 0000000..68c091e
--- /dev/null
+++ b/src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs
@@ -0,0 +1,166 @@
+´╗┐using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+using Dapper;
+using Npgsql;
+using System.Data;
+using System.Text;
+
+namespace Beneficios.Infrastructure.Repositories;
+
+public class LinhaOnibusRepository : ILinhaOnibusRepository
+{
+    private readonly IDbConnection _dbConnection;
+
+    public LinhaOnibusRepository(IDbConnection dbConnection)
+    {
+        _dbConnection = dbConnection;
+    }
+
+    public async Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros)
+    {
+        EnsureOpen();
+        const string sql = """
+            INSERT INTO linhas_onibus (
+                id, descricao, data_inicio, data_fim, valor_tarifa,
+                data_inclusao, usuario_alteracao_id)
+            VALUES (
+                @Id, @Descricao, @DataInicio, @DataFim, @ValorTarifa,
+                @DataInclusao, @UsuarioAlteracaoId)
+            """;
+        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros));
+        return parametros.Id;
+    }
+
+    public async Task AtualizarAsync(LinhaOnibusAtualizarParams parametros)
+    {
+        EnsureOpen();
+        const string sql = """
+            UPDATE linhas_onibus SET
+                descricao = @Descricao,
+                data_inicio = @DataInicio,
+                data_fim = @DataFim,
+                valor_tarifa = @ValorTarifa,
+                data_alteracao = @DataAlteracao,
+                usuario_alteracao_id = @UsuarioAlteracaoId
+            WHERE id = @Id
+            """;
+        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros));
+        if (rows == 0)
+            throw new InvalidOperationException("Linha n├úo encontrada.");
+    }
+
+    public async Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id)
+    {
+        const string sql = """
+            SELECT id AS Id, descricao AS Descricao,
+                data_inicio AS DataInicioObj, data_fim AS DataFimObj,
+                valor_tarifa AS ValorTarifa,
+                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
+            FROM linhas_onibus
+            WHERE id = @Id
+            """;
+        var row = await _dbConnection.QueryFirstOrDefaultAsync<LinhaRow>(sql, new { Id = id });
+        return row is null ? null : Mapear(row);
+    }
+
+    public async Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro)
+    {
+        var sql = new StringBuilder("""
+            SELECT id AS Id, descricao AS Descricao,
+                data_inicio AS DataInicioObj, data_fim AS DataFimObj,
+                valor_tarifa AS ValorTarifa,
+                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
+            FROM linhas_onibus
+            WHERE 1=1
+            """);
+
+        if (!string.IsNullOrWhiteSpace(filtro.Descricao))
+            sql.Append(" AND descricao ILIKE @Descricao");
+
+        if (filtro.SomenteVigentes == true && filtro.Referencia is not null)
+        {
+            sql.Append("""
+                 AND data_inicio <= @Referencia
+                 AND (data_fim IS NULL OR data_fim >= @Referencia)
+                """);
+        }
+
+        sql.Append(" ORDER BY descricao");
+
+        try
+        {
+            var rows = await _dbConnection.QueryAsync<LinhaRow>(sql.ToString(), new
+            {
+                Descricao = string.IsNullOrWhiteSpace(filtro.Descricao)
+                    ? null
+                    : $"%{filtro.Descricao.Trim()}%",
+                Referencia = filtro.Referencia?.ToDateTime(TimeOnly.MinValue),
+            });
+            return rows.Select(Mapear).ToArray();
+        }
+        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
+        {
+            return Array.Empty<LinhaOnibusQueryResult>();
+        }
+    }
+
+    private void EnsureOpen()
+    {
+        if (_dbConnection.State != ConnectionState.Open)
+            _dbConnection.Open();
+    }
+
+    private static object MapearInsert(LinhaOnibusSalvarParams p) => new
+    {
+        p.Id,
+        p.Descricao,
+        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
+        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
+        p.ValorTarifa,
+        p.DataInclusao,
+        p.UsuarioAlteracaoId,
+    };
+
+    private static object MapearUpdate(LinhaOnibusAtualizarParams p) => new
+    {
+        p.Id,
+        p.Descricao,
+        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
+        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
+        p.ValorTarifa,
+        p.DataAlteracao,
+        p.UsuarioAlteracaoId,
+    };
+
+    private static LinhaOnibusQueryResult Mapear(LinhaRow row) => new()
+    {
+        Id = row.Id,
+        Descricao = row.Descricao,
+        DataInicio = ToDateOnly(row.DataInicioObj),
+        DataFim = ToDateOnlyNullable(row.DataFimObj),
+        ValorTarifa = row.ValorTarifa,
+        DataInclusao = row.DataInclusao,
+        DataAlteracao = row.DataAlteracao,
+    };
+
+    private static DateOnly ToDateOnly(object valor) => valor switch
+    {
+        DateOnly d => d,
+        DateTime dt => DateOnly.FromDateTime(dt),
+        _ => DateOnly.FromDateTime(Convert.ToDateTime(valor)),
+    };
+
+    private static DateOnly? ToDateOnlyNullable(object? valor) =>
+        valor is null or DBNull ? null : ToDateOnly(valor);
+
+    private sealed class LinhaRow
+    {
+        public Guid Id { get; set; }
+        public string Descricao { get; set; } = "";
+        public object DataInicioObj { get; set; } = default!;
+        public object? DataFimObj { get; set; }
+        public decimal ValorTarifa { get; set; }
+        public DateTime DataInclusao { get; set; }
+        public DateTime? DataAlteracao { get; set; }
+    }
+}
diff --git a/src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql b/src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql
new file mode 100644
index 0000000..6812b0f
--- /dev/null
+++ b/src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql
@@ -0,0 +1,2 @@
+´╗┐-- Refer├¬ncia: tabela linhas_onibus no schema tenant
+-- Aplicado via TenantSchemaSql.CriarTabelaLinhasOnibus
diff --git a/src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql b/src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql
new file mode 100644
index 0000000..3528b7b
--- /dev/null
+++ b/src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql
@@ -0,0 +1,2 @@
+´╗┐-- Refer├¬ncia: tabela funcionario_linhas no schema tenant
+-- Aplicado via TenantSchemaSql.CriarTabelaFuncionarioLinhas
diff --git a/src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs b/src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs
index ed402f8..50f482b 100644
--- a/src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs
+++ b/src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs
@@ -54,10 +54,18 @@ public class TenantProvisioner : ITenantProvisioner
 
         await connection.ExecuteAsync(new CommandDefinition(
             TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName),
             cancellationToken: cancellationToken));
 
+        await connection.ExecuteAsync(new CommandDefinition(
+            TenantSchemaSql.CriarTabelaLinhasOnibus(schemaName),
+            cancellationToken: cancellationToken));
+
+        await connection.ExecuteAsync(new CommandDefinition(
+            TenantSchemaSql.CriarTabelaFuncionarioLinhas(schemaName),
+            cancellationToken: cancellationToken));
+
         await connection.ExecuteAsync(new CommandDefinition(
             TenantSchemaSql.DropColunaMotivoAfastamento(schemaName),
             cancellationToken: cancellationToken));
 
         await connection.ExecuteAsync(new CommandDefinition(
@@ -140,10 +148,18 @@ public class TenantProvisioner : ITenantProvisioner
 
         await connection.ExecuteAsync(new CommandDefinition(
             TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName),
             cancellationToken: cancellationToken));
 
+        await connection.ExecuteAsync(new CommandDefinition(
+            TenantSchemaSql.CriarTabelaLinhasOnibus(schemaName),
+            cancellationToken: cancellationToken));
+
+        await connection.ExecuteAsync(new CommandDefinition(
+            TenantSchemaSql.CriarTabelaFuncionarioLinhas(schemaName),
+            cancellationToken: cancellationToken));
+
         await connection.ExecuteAsync(new CommandDefinition(
             TenantSchemaSql.DropColunaMotivoAfastamento(schemaName),
             cancellationToken: cancellationToken));
 
         await connection.ExecuteAsync(new CommandDefinition(
diff --git a/src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs b/src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs
index 983224e..a182d7d 100644
--- a/src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs
+++ b/src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs
@@ -238,10 +238,66 @@ public static class TenantSchemaSql
             CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_afast_datas
                 ON {quotedSchema}.funcionario_afastamentos(data_inicio, data_fim);
             """;
     }
 
+    public static string CriarTabelaLinhasOnibus(string schemaName)
+    {
+        var quotedSchema = CitarIdentificador(schemaName);
+        var indexPrefix = schemaName.Replace('-', '_');
+
+        return $"""
+            CREATE TABLE IF NOT EXISTS {quotedSchema}.linhas_onibus (
+                id UUID PRIMARY KEY,
+                descricao VARCHAR NOT NULL,
+                data_inicio DATE NOT NULL,
+                data_fim DATE NULL,
+                valor_tarifa NUMERIC(18,2) NOT NULL,
+                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
+                data_alteracao TIMESTAMP NULL,
+                usuario_alteracao_id UUID NULL
+            );
+
+            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_descricao
+                ON {quotedSchema}.linhas_onibus(descricao);
+
+            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_datas
+                ON {quotedSchema}.linhas_onibus(data_inicio, data_fim);
+            """;
+    }
+
+    public static string CriarTabelaFuncionarioLinhas(string schemaName)
+    {
+        var quotedSchema = CitarIdentificador(schemaName);
+        var indexPrefix = schemaName.Replace('-', '_');
+
+        return $"""
+            CREATE TABLE IF NOT EXISTS {quotedSchema}.funcionario_linhas (
+                id UUID PRIMARY KEY,
+                funcionario_id UUID NOT NULL,
+                linha_onibus_id UUID NOT NULL,
+                quantidade INT NOT NULL,
+                data_inicio DATE NOT NULL,
+                data_fim DATE NULL,
+                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
+                data_alteracao TIMESTAMP NULL,
+                usuario_alteracao_id UUID NULL,
+                CONSTRAINT fk_{indexPrefix}_func_linha_funcionario
+                    FOREIGN KEY (funcionario_id) REFERENCES {quotedSchema}.funcionarios(id),
+                CONSTRAINT fk_{indexPrefix}_func_linha_linha
+                    FOREIGN KEY (linha_onibus_id) REFERENCES {quotedSchema}.linhas_onibus(id),
+                CONSTRAINT uq_{indexPrefix}_func_linha UNIQUE (funcionario_id, linha_onibus_id)
+            );
+
+            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_funcionario
+                ON {quotedSchema}.funcionario_linhas(funcionario_id);
+
+            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_linha
+                ON {quotedSchema}.funcionario_linhas(linha_onibus_id);
+            """;
+    }
+
     public static string DropColunaMotivoAfastamento(string schemaName)
     {
         var quotedSchema = CitarIdentificador(schemaName);
         return $"""
             ALTER TABLE {quotedSchema}.funcionarios DROP COLUMN IF EXISTS motivo_afastamento;
diff --git a/tests/Beneficios.Tests/Api/FuncionarioLinhasControllerTests.cs b/tests/Beneficios.Tests/Api/FuncionarioLinhasControllerTests.cs
new file mode 100644
index 0000000..7f4c970
--- /dev/null
+++ b/tests/Beneficios.Tests/Api/FuncionarioLinhasControllerTests.cs
@@ -0,0 +1,238 @@
+´╗┐using Beneficios.Api.Controllers;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Domain.Enums;
+using Microsoft.AspNetCore.Http;
+using Microsoft.AspNetCore.Mvc;
+using Microsoft.Extensions.Logging.Abstractions;
+using Moq;
+using System.Security.Claims;
+using Xunit;
+
+namespace Beneficios.Tests.Api;
+
+public class FuncionarioLinhasControllerTests
+{
+    private readonly Mock<IFuncionarioLinhaService> _service = new();
+    private readonly Mock<IPermissaoService> _permissao = new();
+
+    private FuncionarioLinhasController CreateController()
+    {
+        var controller = new FuncionarioLinhasController(
+            _service.Object,
+            _permissao.Object,
+            NullLogger<FuncionarioLinhasController>.Instance);
+
+        var http = new DefaultHttpContext();
+        http.Request.Headers["X-Tenant"] = "empresa1";
+        http.User = new ClaimsPrincipal(new ClaimsIdentity(
+        [
+            new Claim("sub", Guid.NewGuid().ToString()),
+        ], "test"));
+        controller.ControllerContext = new ControllerContext { HttpContext = http };
+        return controller;
+    }
+
+    [Fact]
+    public async Task Filtrar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Filtrar(null, null);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Filtrar_Valido_DeveRetornarOk()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.FiltrarAsync(null, null))
+            .ReturnsAsync(Array.Empty<FuncionarioLinhaDto>());
+
+        var result = await CreateController().Filtrar(null, null);
+        Assert.IsType<OkObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Criar_Valido_DeveRetornarCreated()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarDto>(), It.IsAny<Guid?>()))
+            .ReturnsAsync(id);
+
+        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
+            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+
+        var created = Assert.IsType<CreatedAtActionResult>(result);
+        Assert.Equal(nameof(FuncionarioLinhasController.ObterPorId), created.ActionName);
+    }
+
+    [Fact]
+    public async Task Criar_VtInativo_DeveRetornarBadRequest()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new InvalidOperationException(
+                "Funcion├írio sem vale transporte ativo; n├úo ├® poss├¡vel vincular linhas."));
+
+        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
+            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        Assert.IsType<BadRequestObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaDto?)null);
+
+        var result = await CreateController().ObterPorId(id);
+        Assert.IsType<NotFoundObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task ObterPorId_Valido_DeveRetornarOk()
+    {
+        var id = Guid.NewGuid();
+        var funcionarioId = Guid.NewGuid();
+        var linhaId = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioLinhaDto(
+            id, funcionarioId, "Ana", linhaId, "Linha 100", 1, new DateOnly(2026, 1, 1), null));
+
+        var result = await CreateController().ObterPorId(id);
+        Assert.IsType<OkObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_Valido_DeveRetornarNoContent()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+
+        var result = await CreateController().Atualizar(id, new FuncionarioLinhaAtualizarDto(
+            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        Assert.IsType<NoContentResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.AtualizarAsync(id, It.IsAny<FuncionarioLinhaAtualizarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new InvalidOperationException("V├¡nculo n├úo encontrado."));
+
+        var result = await CreateController().Atualizar(id, new FuncionarioLinhaAtualizarDto(
+            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        Assert.IsType<NotFoundObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Atualizar(Guid.NewGuid(), new FuncionarioLinhaAtualizarDto(
+            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Criar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
+            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Filtrar_Excecao_DeveRetornar500()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.FiltrarAsync(null, null)).ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Filtrar(null, null);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task ObterPorId_Excecao_DeveRetornar500()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().ObterPorId(id);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Criar_Excecao_DeveRetornar500()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
+            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Atualizar_Excecao_DeveRetornar500()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.AtualizarAsync(id, It.IsAny<FuncionarioLinhaAtualizarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Atualizar(id, new FuncionarioLinhaAtualizarDto(
+            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+}
diff --git a/tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs b/tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs
new file mode 100644
index 0000000..848711a
--- /dev/null
+++ b/tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs
@@ -0,0 +1,252 @@
+´╗┐using Beneficios.Api.Controllers;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
+using Beneficios.Domain.Enums;
+using Microsoft.AspNetCore.Http;
+using Microsoft.AspNetCore.Mvc;
+using Microsoft.Extensions.Logging.Abstractions;
+using Moq;
+using System.Security.Claims;
+using Xunit;
+
+namespace Beneficios.Tests.Api;
+
+public class LinhasOnibusControllerTests
+{
+    private readonly Mock<ILinhaOnibusService> _service = new();
+    private readonly Mock<IPermissaoService> _permissao = new();
+
+    private LinhasOnibusController CreateController()
+    {
+        var controller = new LinhasOnibusController(
+            _service.Object,
+            _permissao.Object,
+            NullLogger<LinhasOnibusController>.Instance);
+
+        var http = new DefaultHttpContext();
+        http.Request.Headers["X-Tenant"] = "empresa1";
+        http.User = new ClaimsPrincipal(new ClaimsIdentity(
+        [
+            new Claim("sub", Guid.NewGuid().ToString()),
+        ], "test"));
+        controller.ControllerContext = new ControllerContext { HttpContext = http };
+        return controller;
+    }
+
+    [Fact]
+    public async Task Filtrar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Filtrar(null, null);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Filtrar_Valido_DeveRetornarOk()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.FiltrarAsync(null, null))
+            .ReturnsAsync(Array.Empty<LinhaOnibusDto>());
+
+        var result = await CreateController().Filtrar(null, null);
+        Assert.IsType<OkObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Criar_Valido_DeveRetornarCreated()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<LinhaOnibusSalvarDto>(), It.IsAny<Guid?>()))
+            .ReturnsAsync(id);
+
+        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+
+        var created = Assert.IsType<CreatedAtActionResult>(result);
+        Assert.Equal(nameof(LinhasOnibusController.ObterPorId), created.ActionName);
+    }
+
+    [Fact]
+    public async Task Criar_TarifaInvalida_DeveRetornarBadRequest()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<LinhaOnibusSalvarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new InvalidOperationException("O valor da tarifa deve ser maior ou igual a zero."));
+
+        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, -1m));
+        Assert.IsType<BadRequestObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusDto?)null);
+
+        var result = await CreateController().ObterPorId(id);
+        Assert.IsType<NotFoundObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task ObterPorId_Valido_DeveRetornarOk()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new LinhaOnibusDto(
+            id, "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+
+        var result = await CreateController().ObterPorId(id);
+        Assert.IsType<OkObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_Valido_DeveRetornarNoContent()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+
+        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        Assert.IsType<NoContentResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.AtualizarAsync(id, It.IsAny<LinhaOnibusAtualizarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new InvalidOperationException("Linha n├úo encontrada."));
+
+        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        Assert.IsType<NotFoundObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_VinculosAbertos_DeveRetornarBadRequest()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.AtualizarAsync(id, It.IsAny<LinhaOnibusAtualizarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new InvalidOperationException(
+                "Existem v├¡nculos em aberto para esta linha; encerre-os antes de finalizar a vig├¬ncia."));
+
+        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), 4.50m));
+        Assert.IsType<BadRequestObjectResult>(result);
+    }
+
+    [Fact]
+    public async Task Atualizar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Atualizar(Guid.NewGuid(), new LinhaOnibusAtualizarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Criar_SemPermissao_DeveRetornar403()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
+            .ThrowsAsync(new UnauthorizedAccessException("Sem permiss├úo para esta opera├º├úo."));
+
+        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(403, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Filtrar_Excecao_DeveRetornar500()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.FiltrarAsync(null, null)).ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Filtrar(null, null);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task ObterPorId_Excecao_DeveRetornar500()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
+            .Returns(Task.CompletedTask);
+        _service.Setup(x => x.ObterPorIdAsync(id)).ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().ObterPorId(id);
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Criar_Excecao_DeveRetornar500()
+    {
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.SalvarAsync(It.IsAny<LinhaOnibusSalvarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+
+    [Fact]
+    public async Task Atualizar_Excecao_DeveRetornar500()
+    {
+        var id = Guid.NewGuid();
+        _permissao
+            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
+            .Returns(Task.CompletedTask);
+        _service
+            .Setup(x => x.AtualizarAsync(id, It.IsAny<LinhaOnibusAtualizarDto>(), It.IsAny<Guid?>()))
+            .ThrowsAsync(new Exception("boom"));
+
+        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
+            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
+        var obj = Assert.IsType<ObjectResult>(result);
+        Assert.Equal(500, obj.StatusCode);
+    }
+}
diff --git a/tests/Beneficios.Tests/Application/FuncionarioLinhaServiceTests.cs b/tests/Beneficios.Tests/Application/FuncionarioLinhaServiceTests.cs
new file mode 100644
index 0000000..4315532
--- /dev/null
+++ b/tests/Beneficios.Tests/Application/FuncionarioLinhaServiceTests.cs
@@ -0,0 +1,357 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Mappings;
+using Beneficios.Application.Services;
+using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+using Moq;
+using Xunit;
+
+namespace Beneficios.Tests.Application;
+
+public class FuncionarioLinhaServiceTests
+{
+    private readonly Mock<IFuncionarioLinhaRepository> _vinculoRepo = new();
+    private readonly Mock<ILinhaOnibusRepository> _linhaRepo = new();
+    private readonly Mock<IFuncionarioRepository> _funcionarioRepo = new();
+    private readonly FuncionarioLinhaService _sut;
+
+    private readonly Guid _funcionarioId = Guid.NewGuid();
+    private readonly Guid _linhaId = Guid.NewGuid();
+
+    public FuncionarioLinhaServiceTests()
+    {
+        var mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioLinhaProfile>()).CreateMapper();
+        _sut = new FuncionarioLinhaService(
+            _vinculoRepo.Object,
+            _linhaRepo.Object,
+            _funcionarioRepo.Object,
+            mapper);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_VtInativo_DeveFalhar()
+    {
+        SetupFuncionarioExiste();
+        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
+            .ReturnsAsync(
+            [
+                new FuncionarioBeneficioQueryResult
+                {
+                    CodigoBeneficio = FuncionarioLinhaService.CodigoValeTransporte,
+                    Ativo = false,
+                },
+            ]);
+        SetupLinhaVigente();
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemVtInativo, ex.Message);
+        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_SemBeneficioVt_DeveFalhar()
+    {
+        SetupFuncionarioExiste();
+        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
+            .ReturnsAsync(Array.Empty<FuncionarioBeneficioQueryResult>());
+        SetupLinhaVigente();
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemVtInativo, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_LinhaNaoVigente_DeveFalhar()
+    {
+        SetupFuncionarioComVt();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = _linhaId,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2025, 1, 1),
+                DataFim = new DateOnly(2025, 12, 31),
+                ValorTarifa = 4.50m,
+            });
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(dataInicio: new DateOnly(2026, 1, 15)), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemLinhaNaoVigente, ex.Message);
+        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_ParDuplicado_DeveFalhar()
+    {
+        SetupFuncionarioComVt();
+        SetupLinhaVigente();
+        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, null)).ReturnsAsync(true);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemParDuplicado, ex.Message);
+        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_QuantidadeZero_DeveFalhar()
+    {
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(quantidade: 0), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemQuantidade, ex.Message);
+        _funcionarioRepo.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_DataFimMenor_DeveFalhar()
+    {
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(
+                dataInicio: new DateOnly(2026, 1, 31),
+                dataFim: new DateOnly(2026, 1, 1)), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemDataFim, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_FuncionarioNaoEncontrado_DeveFalhar()
+    {
+        _funcionarioRepo.Setup(r => r.ObterPorIdAsync(_funcionarioId))
+            .ReturnsAsync((FuncionarioQueryResult?)null);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemFuncionarioNaoEncontrado, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_LinhaNaoEncontrada_DeveFalhar()
+    {
+        SetupFuncionarioComVt();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId)).ReturnsAsync((LinhaOnibusQueryResult?)null);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.SalvarAsync(DtoSalvar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemLinhaNaoEncontrada, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_Valido_DevePersistir()
+    {
+        SetupFuncionarioComVt();
+        SetupLinhaVigente();
+        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, null)).ReturnsAsync(false);
+        _vinculoRepo.Setup(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()))
+            .ReturnsAsync((FuncionarioLinhaSalvarParams p) => p.Id);
+
+        var id = await _sut.SalvarAsync(DtoSalvar(quantidade: 2), null);
+
+        Assert.NotEqual(Guid.Empty, id);
+        _vinculoRepo.Verify(r => r.SalvarAsync(It.Is<FuncionarioLinhaSalvarParams>(
+            p => p.FuncionarioId == _funcionarioId
+                 && p.LinhaOnibusId == _linhaId
+                 && p.Quantidade == 2
+                 && p.DataInicio == new DateOnly(2026, 1, 1))), Times.Once);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_VinculoNaoEncontrado_DeveFalhar()
+    {
+        var id = Guid.NewGuid();
+        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaQueryResult?)null);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
+            _sut.AtualizarAsync(id, DtoAtualizar(), null));
+
+        Assert.Equal(FuncionarioLinhaService.MensagemVinculoNaoEncontrado, ex.Message);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_Valido_DevePersistir()
+    {
+        var id = Guid.NewGuid();
+        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new FuncionarioLinhaQueryResult
+            {
+                Id = id,
+                FuncionarioId = _funcionarioId,
+                LinhaOnibusId = _linhaId,
+                Quantidade = 1,
+                DataInicio = new DateOnly(2026, 1, 1),
+            });
+        SetupFuncionarioComVt();
+        SetupLinhaVigente();
+        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, id)).ReturnsAsync(false);
+
+        await _sut.AtualizarAsync(id, DtoAtualizar(quantidade: 3), null);
+
+        _vinculoRepo.Verify(r => r.AtualizarAsync(It.Is<FuncionarioLinhaAtualizarParams>(
+            p => p.Id == id && p.Quantidade == 3 && p.LinhaOnibusId == _linhaId)), Times.Once);
+    }
+
+    [Fact]
+    public async Task FiltrarAsync_SomenteVigentes_DeveFiltrar()
+    {
+        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
+        _vinculoRepo.Setup(r => r.FiltrarAsync(It.IsAny<FuncionarioLinhaFiltroParams>()))
+            .ReturnsAsync(
+            [
+                new FuncionarioLinhaQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    FuncionarioId = _funcionarioId,
+                    FuncionarioNome = "Ana",
+                    LinhaOnibusId = _linhaId,
+                    LinhaDescricao = "Vigente",
+                    Quantidade = 1,
+                    DataInicio = hoje.AddDays(-10),
+                    DataFim = null,
+                },
+                new FuncionarioLinhaQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    FuncionarioId = _funcionarioId,
+                    FuncionarioNome = "Ana",
+                    LinhaOnibusId = Guid.NewGuid(),
+                    LinhaDescricao = "Encerrada",
+                    Quantidade = 1,
+                    DataInicio = hoje.AddDays(-30),
+                    DataFim = hoje.AddDays(-1),
+                },
+            ]);
+
+        var lista = await _sut.FiltrarAsync(_funcionarioId, true);
+        Assert.Single(lista);
+        Assert.Equal("Vigente", lista[0].LinhaDescricao);
+    }
+
+    [Fact]
+    public async Task ObterPorIdAsync_NaoEncontrado_DeveRetornarNull()
+    {
+        var id = Guid.NewGuid();
+        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaQueryResult?)null);
+        Assert.Null(await _sut.ObterPorIdAsync(id));
+    }
+
+    [Fact]
+    public async Task ObterPorIdAsync_Encontrado_DeveMapear()
+    {
+        var id = Guid.NewGuid();
+        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new FuncionarioLinhaQueryResult
+            {
+                Id = id,
+                FuncionarioId = _funcionarioId,
+                FuncionarioNome = "Ana",
+                LinhaOnibusId = _linhaId,
+                LinhaDescricao = "Linha 100",
+                Quantidade = 2,
+                DataInicio = new DateOnly(2026, 1, 1),
+            });
+
+        var dto = await _sut.ObterPorIdAsync(id);
+        Assert.NotNull(dto);
+        Assert.Equal("Ana", dto!.FuncionarioNome);
+        Assert.Equal(2, dto.Quantidade);
+    }
+
+    [Fact]
+    public async Task FiltrarAsync_SemSomenteVigentes_DeveRetornarTodos()
+    {
+        _vinculoRepo.Setup(r => r.FiltrarAsync(It.IsAny<FuncionarioLinhaFiltroParams>()))
+            .ReturnsAsync(
+            [
+                new FuncionarioLinhaQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    FuncionarioId = _funcionarioId,
+                    FuncionarioNome = "Ana",
+                    LinhaOnibusId = _linhaId,
+                    LinhaDescricao = "A",
+                    Quantidade = 1,
+                    DataInicio = new DateOnly(2026, 1, 1),
+                },
+                new FuncionarioLinhaQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    FuncionarioId = _funcionarioId,
+                    FuncionarioNome = "Ana",
+                    LinhaOnibusId = Guid.NewGuid(),
+                    LinhaDescricao = "B",
+                    Quantidade = 1,
+                    DataInicio = new DateOnly(2025, 1, 1),
+                    DataFim = new DateOnly(2025, 6, 1),
+                },
+            ]);
+
+        var lista = await _sut.FiltrarAsync(_funcionarioId, false);
+        Assert.Equal(2, lista.Count);
+    }
+
+    [Fact]
+    public async Task EncerrarAbertosPorFuncionarioAsync_DeveDelegarRepositorio()
+    {
+        var usuarioId = Guid.NewGuid();
+        await _sut.EncerrarAbertosPorFuncionarioAsync(_funcionarioId, usuarioId);
+
+        _vinculoRepo.Verify(
+            r => r.EncerrarAbertosPorFuncionarioAsync(
+                _funcionarioId,
+                It.IsAny<DateOnly>(),
+                It.IsAny<DateTime>(),
+                usuarioId),
+            Times.Once);
+    }
+
+    private FuncionarioLinhaSalvarDto DtoSalvar(
+        int quantidade = 1,
+        DateOnly? dataInicio = null,
+        DateOnly? dataFim = null) =>
+        new(_funcionarioId, _linhaId, quantidade, dataInicio ?? new DateOnly(2026, 1, 1), dataFim);
+
+    private FuncionarioLinhaAtualizarDto DtoAtualizar(int quantidade = 1) =>
+        new(_linhaId, quantidade, new DateOnly(2026, 1, 1), null);
+
+    private void SetupFuncionarioExiste()
+    {
+        _funcionarioRepo.Setup(r => r.ObterPorIdAsync(_funcionarioId))
+            .ReturnsAsync(new FuncionarioQueryResult { Id = _funcionarioId, Nome = "Ana" });
+    }
+
+    private void SetupFuncionarioComVt()
+    {
+        SetupFuncionarioExiste();
+        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
+            .ReturnsAsync(
+            [
+                new FuncionarioBeneficioQueryResult
+                {
+                    CodigoBeneficio = FuncionarioLinhaService.CodigoValeTransporte,
+                    Ativo = true,
+                },
+            ]);
+    }
+
+    private void SetupLinhaVigente()
+    {
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = _linhaId,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2026, 1, 1),
+                DataFim = null,
+                ValorTarifa = 4.50m,
+            });
+    }
+}
diff --git a/tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs b/tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs
index 87cadf6..69d4061 100644
--- a/tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs
+++ b/tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs
@@ -1,7 +1,8 @@
 ´╗┐using AutoMapper;
 using Beneficios.Application.DTOs;
+using Beneficios.Application.Interfaces;
 using Beneficios.Application.Mappings;
 using Beneficios.Application.Services;
 using Beneficios.Domain.Enums;
 using Beneficios.Domain.Interfaces;
 using Beneficios.Domain.Models;
@@ -12,16 +13,17 @@ namespace Beneficios.Tests.Application;
 
 public class FuncionarioServiceTests
 {
     private readonly Mock<IFuncionarioRepository> _repo = new();
     private readonly Mock<IFuncionarioAfastamentoRepository> _afastRepo = new();
+    private readonly Mock<IFuncionarioLinhaService> _linhaService = new();
     private readonly FuncionarioService _sut;
 
     public FuncionarioServiceTests()
     {
         var mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioProfile>()).CreateMapper();
-        _sut = new FuncionarioService(_repo.Object, _afastRepo.Object, mapper);
+        _sut = new FuncionarioService(_repo.Object, _afastRepo.Object, _linhaService.Object, mapper);
     }
 
     [Fact]
     public async Task SalvarAsync_SituacaoAfastado_DeveFalhar()
     {
@@ -171,10 +173,83 @@ public class FuncionarioServiceTests
         _repo.Verify(r => r.AtualizarAsync(
             It.Is<FuncionarioAtualizarParams>(p => p.Id == id),
             It.IsAny<IReadOnlyList<FuncionarioBeneficioSalvarParams>>()), Times.Once);
     }
 
+    [Fact]
+    public async Task AtualizarAsync_VtInativo_DeveEncerrarVinculosAbertos()
+    {
+        var id = Guid.NewGuid();
+        var usuarioId = Guid.NewGuid();
+        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
+        {
+            Id = id,
+            Nome = "Ana",
+            Cpf = "52998224725",
+            Cargo = "Analista",
+            SalarioBase = 1,
+            TipoContrato = TipoContrato.Clt,
+            Situacao = SituacaoFuncionario.Ativo,
+            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
+            DataAdmissao = new DateOnly(2024, 1, 1),
+            DataInclusao = DateTime.UtcNow,
+        });
+        _afastRepo.Setup(r => r.ObterAtivoEmAsync(id, It.IsAny<DateOnly>()))
+            .ReturnsAsync((FuncionarioAfastamentoQueryResult?)null);
+        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), id)).ReturnsAsync(false);
+
+        var dto = CriarAtualizarDto(SituacaoFuncionario.Ativo) with
+        {
+            Beneficios =
+            [
+                new FuncionarioBeneficioSalvarDto("vale_transporte", false, new DateOnly(2024, 1, 10), null, true),
+            ],
+        };
+
+        await _sut.AtualizarAsync(id, dto, usuarioId);
+
+        _linhaService.Verify(
+            s => s.EncerrarAbertosPorFuncionarioAsync(id, usuarioId),
+            Times.Once);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_VtAtivo_NaoDeveEncerrarVinculos()
+    {
+        var id = Guid.NewGuid();
+        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
+        {
+            Id = id,
+            Nome = "Ana",
+            Cpf = "52998224725",
+            Cargo = "Analista",
+            SalarioBase = 1,
+            TipoContrato = TipoContrato.Clt,
+            Situacao = SituacaoFuncionario.Ativo,
+            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
+            DataAdmissao = new DateOnly(2024, 1, 1),
+            DataInclusao = DateTime.UtcNow,
+        });
+        _afastRepo.Setup(r => r.ObterAtivoEmAsync(id, It.IsAny<DateOnly>()))
+            .ReturnsAsync((FuncionarioAfastamentoQueryResult?)null);
+        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), id)).ReturnsAsync(false);
+
+        var dto = CriarAtualizarDto(SituacaoFuncionario.Ativo) with
+        {
+            Beneficios =
+            [
+                new FuncionarioBeneficioSalvarDto("vale_transporte", true, new DateOnly(2024, 1, 10), null, true),
+            ],
+        };
+
+        await _sut.AtualizarAsync(id, dto, null);
+
+        _linhaService.Verify(
+            s => s.EncerrarAbertosPorFuncionarioAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()),
+            Times.Never);
+    }
+
     [Fact]
     public async Task ObterPorIdAsync_DeveCarregarBeneficios()
     {
         var id = Guid.NewGuid();
         _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
diff --git a/tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs b/tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs
new file mode 100644
index 0000000..11920ba
--- /dev/null
+++ b/tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs
@@ -0,0 +1,238 @@
+´╗┐using AutoMapper;
+using Beneficios.Application.DTOs;
+using Beneficios.Application.Mappings;
+using Beneficios.Application.Services;
+using Beneficios.Domain.Interfaces;
+using Beneficios.Domain.Models;
+using Moq;
+using Xunit;
+
+namespace Beneficios.Tests.Application;
+
+public class LinhaOnibusServiceTests
+{
+    private readonly Mock<ILinhaOnibusRepository> _linhaRepo = new();
+    private readonly Mock<IFuncionarioLinhaRepository> _vinculoRepo = new();
+    private readonly LinhaOnibusService _sut;
+
+    public LinhaOnibusServiceTests()
+    {
+        var mapper = new MapperConfiguration(c => c.AddProfile<LinhaOnibusProfile>()).CreateMapper();
+        _sut = new LinhaOnibusService(_linhaRepo.Object, _vinculoRepo.Object, mapper);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_DataFimMenor_DeveFalhar()
+    {
+        var dto = new LinhaOnibusSalvarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 31),
+            new DateOnly(2026, 1, 1),
+            4.50m);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
+        Assert.Equal(LinhaOnibusService.MensagemDataFim, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_TarifaNegativa_DeveFalhar()
+    {
+        var dto = new LinhaOnibusSalvarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            null,
+            -1m);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
+        Assert.Equal(LinhaOnibusService.MensagemTarifa, ex.Message);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_EncerrarComVinculoAberto_DeveFalhar()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = id,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2026, 1, 1),
+                ValorTarifa = 4.50m,
+            });
+        _vinculoRepo.Setup(r => r.ExisteVinculoAbertoPorLinhaAsync(id)).ReturnsAsync(true);
+
+        var dto = new LinhaOnibusAtualizarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            new DateOnly(2026, 6, 30),
+            4.50m);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AtualizarAsync(id, dto, null));
+        Assert.Equal(LinhaOnibusService.MensagemVinculosAbertos, ex.Message);
+        _linhaRepo.Verify(r => r.AtualizarAsync(It.IsAny<LinhaOnibusAtualizarParams>()), Times.Never);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_NaoEncontrada_DeveFalhar()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusQueryResult?)null);
+
+        var dto = new LinhaOnibusAtualizarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            null,
+            4.50m);
+
+        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AtualizarAsync(id, dto, null));
+        Assert.Equal(LinhaOnibusService.MensagemNaoEncontrada, ex.Message);
+    }
+
+    [Fact]
+    public async Task SalvarAsync_Valido_DevePersistir()
+    {
+        _linhaRepo.Setup(r => r.SalvarAsync(It.IsAny<LinhaOnibusSalvarParams>()))
+            .ReturnsAsync((LinhaOnibusSalvarParams p) => p.Id);
+
+        var id = await _sut.SalvarAsync(new LinhaOnibusSalvarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            null,
+            4.50m), null);
+
+        Assert.NotEqual(Guid.Empty, id);
+        _linhaRepo.Verify(r => r.SalvarAsync(It.Is<LinhaOnibusSalvarParams>(
+            p => p.Descricao == "Linha 100" && p.ValorTarifa == 4.50m)), Times.Once);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_EncerrarSemVinculo_DevePersistir()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = id,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2026, 1, 1),
+                ValorTarifa = 4.50m,
+            });
+        _vinculoRepo.Setup(r => r.ExisteVinculoAbertoPorLinhaAsync(id)).ReturnsAsync(false);
+
+        await _sut.AtualizarAsync(id, new LinhaOnibusAtualizarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            new DateOnly(2026, 6, 30),
+            5.00m), null);
+
+        _linhaRepo.Verify(r => r.AtualizarAsync(It.Is<LinhaOnibusAtualizarParams>(
+            p => p.Id == id && p.DataFim == new DateOnly(2026, 6, 30) && p.ValorTarifa == 5.00m)), Times.Once);
+    }
+
+    [Fact]
+    public async Task FiltrarAsync_SomenteVigentes_DeveFiltrar()
+    {
+        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
+        _linhaRepo.Setup(r => r.FiltrarAsync(It.IsAny<LinhaOnibusFiltroParams>()))
+            .ReturnsAsync(
+            [
+                new LinhaOnibusQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    Descricao = "Vigente",
+                    DataInicio = hoje.AddDays(-10),
+                    DataFim = null,
+                    ValorTarifa = 1m,
+                },
+                new LinhaOnibusQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    Descricao = "Encerrada",
+                    DataInicio = hoje.AddDays(-30),
+                    DataFim = hoje.AddDays(-1),
+                    ValorTarifa = 2m,
+                },
+            ]);
+
+        var lista = await _sut.FiltrarAsync(null, true);
+        Assert.Single(lista);
+        Assert.Equal("Vigente", lista[0].Descricao);
+    }
+
+    [Fact]
+    public async Task ObterPorIdAsync_NaoEncontrada_DeveRetornarNull()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusQueryResult?)null);
+        Assert.Null(await _sut.ObterPorIdAsync(id));
+    }
+
+    [Fact]
+    public async Task ObterPorIdAsync_Encontrada_DeveMapear()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = id,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2026, 1, 1),
+                ValorTarifa = 4.50m,
+            });
+
+        var dto = await _sut.ObterPorIdAsync(id);
+        Assert.NotNull(dto);
+        Assert.Equal("Linha 100", dto!.Descricao);
+        Assert.Equal(4.50m, dto.ValorTarifa);
+    }
+
+    [Fact]
+    public async Task FiltrarAsync_SemSomenteVigentes_DeveRetornarTodas()
+    {
+        _linhaRepo.Setup(r => r.FiltrarAsync(It.IsAny<LinhaOnibusFiltroParams>()))
+            .ReturnsAsync(
+            [
+                new LinhaOnibusQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    Descricao = "A",
+                    DataInicio = new DateOnly(2026, 1, 1),
+                    ValorTarifa = 1m,
+                },
+                new LinhaOnibusQueryResult
+                {
+                    Id = Guid.NewGuid(),
+                    Descricao = "B",
+                    DataInicio = new DateOnly(2025, 1, 1),
+                    DataFim = new DateOnly(2025, 12, 31),
+                    ValorTarifa = 2m,
+                },
+            ]);
+
+        var lista = await _sut.FiltrarAsync("x", false);
+        Assert.Equal(2, lista.Count);
+    }
+
+    [Fact]
+    public async Task AtualizarAsync_SemDataFim_NaoConsultaVinculos()
+    {
+        var id = Guid.NewGuid();
+        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
+            .ReturnsAsync(new LinhaOnibusQueryResult
+            {
+                Id = id,
+                Descricao = "Linha 100",
+                DataInicio = new DateOnly(2026, 1, 1),
+                ValorTarifa = 4.50m,
+            });
+
+        await _sut.AtualizarAsync(id, new LinhaOnibusAtualizarDto(
+            "Linha 100",
+            new DateOnly(2026, 1, 1),
+            null,
+            5.00m), null);
+
+        _vinculoRepo.Verify(r => r.ExisteVinculoAbertoPorLinhaAsync(It.IsAny<Guid>()), Times.Never);
+        _linhaRepo.Verify(r => r.AtualizarAsync(It.IsAny<LinhaOnibusAtualizarParams>()), Times.Once);
+    }
+}
\ No newline at end of file
diff --git a/tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs b/tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
index 48b0ce8..ad0cbf9 100644
--- a/tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
+++ b/tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
@@ -34,10 +34,24 @@ public class ModulosSistemaCatalogTests
     public void Todos_DeveConterAfastamentos()
     {
         Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "afastamentos" && m.Rota == "/afastamentos");
     }
 
+    [Fact]
+    public void Todos_DeveConterLinhasOnibus()
+    {
+        Assert.Contains(ModulosSistemaCatalog.Todos,
+            m => m.Codigo == "linhas_onibus" && m.Rota == "/linhas-onibus");
+    }
+
+    [Fact]
+    public void Todos_DeveConterFuncionarioLinhas()
+    {
+        Assert.Contains(ModulosSistemaCatalog.Todos,
+            m => m.Codigo == "funcionario_linhas" && m.Rota == "/funcionario-linhas");
+    }
+
     [Fact]
     public void ModulosDePerfil_DevemSuportarTodasAsAcoes()
     {
         foreach (var codigo in new[] { "usuarios", "perfis", "calendario", "funcionarios", "afastamentos" })
         {
diff --git a/tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs b/tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs
index 54c7944..07bd204 100644
--- a/tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs
+++ b/tests/Beneficios.Tests/Infrastructure/PostgresFixture.cs
@@ -159,10 +159,12 @@ public class PostgresFixture : IDisposable
         await connection.ExecuteAsync(TenantSchemaSql.AlterarUsuariosAdicionarPerfilId(schemaName));
         await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaCalendarioDias(schemaName));
         await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarios(schemaName));
         await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarioBeneficios(schemaName));
         await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName));
+        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaLinhasOnibus(schemaName));
+        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarioLinhas(schemaName));
         await connection.ExecuteAsync(TenantSchemaSql.DropColunaMotivoAfastamento(schemaName));
     }
 
     public void Dispose()
     {
diff --git a/tests/Beneficios.Tests/Infrastructure/TenantProvisionerTests.cs b/tests/Beneficios.Tests/Infrastructure/TenantProvisionerTests.cs
index 6dfbf89..9dc1b37 100644
--- a/tests/Beneficios.Tests/Infrastructure/TenantProvisionerTests.cs
+++ b/tests/Beneficios.Tests/Infrastructure/TenantProvisionerTests.cs
@@ -136,10 +136,26 @@ public class TenantProvisionerTests(PostgresFixture fixture)
               SELECT 1 FROM information_schema.tables
               WHERE table_schema = @SchemaName AND table_name = 'funcionario_afastamentos')
             """, new { SchemaName = schemaName });
         Assert.True(afastamentosExiste);
 
+        var linhasOnibusExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
+            """
+            SELECT EXISTS(
+              SELECT 1 FROM information_schema.tables
+              WHERE table_schema = @SchemaName AND table_name = 'linhas_onibus')
+            """, new { SchemaName = schemaName });
+        Assert.True(linhasOnibusExiste);
+
+        var funcionarioLinhasExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
+            """
+            SELECT EXISTS(
+              SELECT 1 FROM information_schema.tables
+              WHERE table_schema = @SchemaName AND table_name = 'funcionario_linhas')
+            """, new { SchemaName = schemaName });
+        Assert.True(funcionarioLinhasExiste);
+
         var motivoExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
             """
             SELECT EXISTS(
               SELECT 1 FROM information_schema.columns
               WHERE table_schema = @SchemaName AND table_name = 'funcionarios'
@@ -296,10 +312,26 @@ public class TenantProvisionerTests(PostgresFixture fixture)
             SELECT EXISTS(
               SELECT 1 FROM information_schema.tables
               WHERE table_schema = @SchemaName AND table_name = 'funcionario_afastamentos')
             """, new { SchemaName = schemaName });
         Assert.True(afastamentosExiste);
+
+        var linhasOnibusExiste = await fixture.Connection.ExecuteScalarAsync<bool>(
+            """
+            SELECT EXISTS(
+              SELECT 1 FROM information_schema.tables
+              WHERE table_schema = @SchemaName AND table_name = 'linhas_onibus')
+            """, new { SchemaName = schemaName });
+        Assert.True(linhasOnibusExiste);
+
+        var funcionarioLinhasExiste = await fixture.Connection.ExecuteScalarAsync<bool>(
+            """
+            SELECT EXISTS(
+              SELECT 1 FROM information_schema.tables
+              WHERE table_schema = @SchemaName AND table_name = 'funcionario_linhas')
+            """, new { SchemaName = schemaName });
+        Assert.True(funcionarioLinhasExiste);
     }
 
     private sealed class DonoProvisionadoResult
     {
         public Guid Id { get; init; }
diff --git a/tests/Beneficios.Tests/Infrastructure/TenantSchemaSqlTests.cs b/tests/Beneficios.Tests/Infrastructure/TenantSchemaSqlTests.cs
index ec964bc..fc768ab 100644
--- a/tests/Beneficios.Tests/Infrastructure/TenantSchemaSqlTests.cs
+++ b/tests/Beneficios.Tests/Infrastructure/TenantSchemaSqlTests.cs
@@ -31,10 +31,31 @@ public class TenantSchemaSqlTests
         Assert.Contains("funcionario_afastamentos", sql);
         Assert.Contains("CREATE INDEX IF NOT EXISTS", sql);
         Assert.Contains("FOREIGN KEY", sql);
     }
 
+    [Fact]
+    public void CriarTabelaLinhasOnibus_DeveCriarTabelaEIndices()
+    {
+        var sql = TenantSchemaSql.CriarTabelaLinhasOnibus("tenant_acme");
+        Assert.Contains("linhas_onibus", sql);
+        Assert.Contains("valor_tarifa", sql);
+        Assert.Contains("CREATE INDEX IF NOT EXISTS", sql);
+        Assert.False(string.IsNullOrWhiteSpace(sql));
+    }
+
+    [Fact]
+    public void CriarTabelaFuncionarioLinhas_DeveCriarTabelaComFksEUnique()
+    {
+        var sql = TenantSchemaSql.CriarTabelaFuncionarioLinhas("tenant_acme");
+        Assert.Contains("funcionario_linhas", sql);
+        Assert.Contains("FOREIGN KEY", sql);
+        Assert.Contains("linhas_onibus", sql);
+        Assert.Contains("UNIQUE (funcionario_id, linha_onibus_id)", sql);
+        Assert.False(string.IsNullOrWhiteSpace(sql));
+    }
+
     [Fact]
     public void RecalcularSituacoesPorAfastamento_DeveUsarAfastadoEPreservarDesligado()
     {
         var sql = TenantSchemaSql.RecalcularSituacoesPorAfastamento("tenant_acme");
         Assert.Contains("'afastado'", sql);
