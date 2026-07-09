# Beneficios.Front Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Criar o frontend Angular 22 em `Beneficios.Front/` (multitenant por subdomínio, Material, dashboard mock) e ajustar o backend .NET 9 (CORS, Perfil, JWT, login por tenant) com testes e cobertura Sonar ≥ 80%.

**Architecture:** App Angular única detecta `admin.*` vs `{tenant}.*` via `TenantService`; auth JWT com interceptor (`Authorization` + `X-Tenant`); backend mantém DDD enxuto existente; TenantMiddleware em task separada (fase 3).

**Tech Stack:** Angular 22, Angular Material, Chart.js/ng2-charts, xlsx, jspdf; .NET 9, Dapper, xUnit, Moq, Coverlet, SonarQube.

## Global Constraints

- Front em `Beneficios.Front/` no mesmo repositório
- Multitenancy: subdomínio → `empresas.dominio` → conexão DB (middleware fase 3)
- Admin em `admin.*`; tenants em `{tenant}.*`
- Dashboard mockado (barras transporte + pizza alimentação)
- Mobile first; menu hambúrguer toggle; export Excel/PDF nas listagens
- Backend: seguir padrões existentes (`SalvarAsync`, `*Params`, `*Profile`, controllers enxutos)
- Backend cobertura ≥ **80%** (`scripts/check-coverage.ps1`)
- Front testes Opção A: unitários core (60–70%); E2E Playwright fase 4
- Redis **fora** do escopo

---

## File Map

### Backend (criar/modificar)

| Arquivo | Responsabilidade |
|---------|------------------|
| `src/Beneficios.Infrastructure/Scripts/04_Alter_Table_Usuarios_Perfil.sql` | Coluna `perfil` |
| `src/Beneficios.Domain/Enums/UsuarioPerfil.cs` | Enum `Admin`, `Empresa` |
| `src/Beneficios.Domain/Entities/Usuario.cs` | Propriedade `Perfil` |
| `src/Beneficios.Domain/Models/UsuarioAuthResult.cs` | + `Perfil`, `EmpresaDominio` |
| `src/Beneficios.Domain/Models/UsuarioQueryResult.cs` | + `Perfil` |
| `src/Beneficios.Domain/Models/UsuarioSalvarParams.cs` | + `Perfil` |
| `src/Beneficios.Application/DTOs/LoginResponseDto.cs` | + `Perfil`, `EmpresaId`, `EmpresaDominio` |
| `src/Beneficios.Application/Interfaces/ITokenService.cs` | `GenerateToken` com perfil/empresaId |
| `src/Beneficios.Application/Services/TokenService.cs` | Claims `perfil`, `empresa_id` |
| `src/Beneficios.Application/Interfaces/IUsuarioService.cs` | `LoginAsync(LoginDto, string tenantSubdomain)` |
| `src/Beneficios.Application/Services/UsuarioService.cs` | Validação perfil + tenant no login |
| `src/Beneficios.Infrastructure/Repositories/UsuarioRepository.cs` | SQL inclui `perfil`; join dominio |
| `src/Beneficios.Api/Program.cs` | CORS policy |
| `src/Beneficios.Api/Controllers/UsuariosController.cs` | Lê header `X-Tenant` no login |
| `tests/Beneficios.Tests/Application/UsuarioServiceTests.cs` | Novos cenários login |
| `tests/Beneficios.Tests/Application/TokenServiceTests.cs` | Claims JWT |
| `tests/Beneficios.Tests/Api/UsuariosControllerTests.cs` | Login com X-Tenant |

### Frontend (criar)

| Arquivo | Responsabilidade |
|---------|------------------|
| `Beneficios.Front/` | Projeto Angular 22 standalone |
| `src/app/core/tenant/tenant.service.ts` | Subdomínio + modo Admin/Tenant |
| `src/app/core/auth/auth.service.ts` | Login/logout/sessionStorage |
| `src/app/core/auth/auth.interceptor.ts` | Bearer + X-Tenant |
| `src/app/core/auth/*.guard.ts` | auth, admin, tenant, guest |
| `src/app/core/api/usuario.service.ts` | HTTP usuarios |
| `src/app/core/api/empresa.service.ts` | HTTP empresas |
| `src/app/layout/shell/` | Sidenav hambúrguer responsivo |
| `src/app/features/auth/login/` | Tela login |
| `src/app/features/dashboard/` | Gráficos mock Chart.js |
| `src/app/features/usuarios/` | CRUD tenant |
| `src/app/features/empresas/` | CRUD admin |
| `src/app/shared/utils/export.service.ts` | Excel/PDF |
| `src/environments/` | apiUrl, baseDomain |

---

## Task 1: Backend — Campo Perfil (schema + domain)

**Files:**
- Create: `src/Beneficios.Infrastructure/Scripts/04_Alter_Table_Usuarios_Perfil.sql`
- Create: `src/Beneficios.Domain/Enums/UsuarioPerfil.cs`
- Modify: `src/Beneficios.Domain/Entities/Usuario.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioAuthResult.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioQueryResult.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioSalvarParams.cs`
- Modify: `src/Beneficios.Infrastructure/Scripts/03_Insert_Sample_Data.sql`
- Test: `tests/Beneficios.Tests/Domain/UsuarioTests.cs`

**Interfaces:**
- Produces: `UsuarioPerfil` enum (`Admin = 0`, `Empresa = 1`), propriedade `Perfil` nas entidades/models

- [ ] **Step 1: Write the failing test**

Adicionar em `tests/Beneficios.Tests/Domain/UsuarioTests.cs`:

```csharp
[Fact]
public void Usuario_DeveTerPropriedadePerfil()
{
    var usuario = new Usuario { Perfil = UsuarioPerfil.Admin };
    Assert.Equal(UsuarioPerfil.Admin, usuario.Perfil);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n`
Expected: FAIL — `UsuarioPerfil` / `Perfil` não existem

- [ ] **Step 3: Write minimal implementation**

`04_Alter_Table_Usuarios_Perfil.sql`:

```sql
ALTER TABLE beneficios.usuarios
    ADD COLUMN perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa';

UPDATE beneficios.usuarios SET perfil = 'Admin' WHERE email = 'admin@exemplo.com';
```

`UsuarioPerfil.cs`:

```csharp
namespace Beneficios.Domain.Enums;

public enum UsuarioPerfil
{
    Empresa = 0,
    Admin = 1
}
```

Adicionar `public UsuarioPerfil Perfil { get; set; }` em `Usuario`, `UsuarioAuthResult`, `UsuarioQueryResult`, `UsuarioSalvarParams`.

Atualizar `03_Insert_Sample_Data.sql` para incluir coluna `perfil` nos INSERTs (`Admin` para admin@exemplo.com, `Empresa` para teste@exemplo.com).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain src/Beneficios.Infrastructure/Scripts tests/Beneficios.Tests/Domain/UsuarioTests.cs
git commit -m "feat(domain): add UsuarioPerfil enum and perfil column script"
```

---

## Task 2: Backend — JWT claims + LoginResponseDto

**Files:**
- Modify: `src/Beneficios.Application/DTOs/LoginResponseDto.cs`
- Modify: `src/Beneficios.Application/Interfaces/ITokenService.cs`
- Modify: `src/Beneficios.Application/Services/TokenService.cs`
- Modify: `src/Beneficios.Application/Mappings/UsuarioProfile.cs`
- Create: `tests/Beneficios.Tests/Application/TokenServiceTests.cs`

**Interfaces:**
- Consumes: `UsuarioPerfil` from Task 1
- Produces: `ITokenService.GenerateToken(Guid usuarioId, string email, UsuarioPerfil perfil, Guid? empresaId)`
- Produces: `LoginResponseDto` with `Perfil`, `EmpresaId`, `EmpresaDominio`

- [ ] **Step 1: Write the failing test**

`tests/Beneficios.Tests/Application/TokenServiceTests.cs`:

```csharp
using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Beneficios.Tests.Application;

public class TokenServiceTests
{
    private const string Secret = "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres";
    private readonly TokenService _service = new(Secret, "BeneficiosApi", "BeneficiosClient");

    [Fact]
    public void GenerateToken_DeveIncluirClaimsPerfilEEmpresaId()
    {
        var empresaId = Guid.NewGuid();
        var token = _service.GenerateToken(Guid.NewGuid(), "admin@test.com", UsuarioPerfil.Admin, null);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == "perfil").Value);
        Assert.False(jwt.Claims.Any(c => c.Type == "empresa_id"));
    }

    [Fact]
    public void GenerateToken_ComEmpresa_DeveIncluirEmpresaId()
    {
        var empresaId = Guid.NewGuid();
        var token = _service.GenerateToken(Guid.NewGuid(), "u@test.com", UsuarioPerfil.Empresa, empresaId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(empresaId.ToString(), jwt.Claims.First(c => c.Type == "empresa_id").Value);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "TokenServiceTests" -v n`
Expected: FAIL — overload `GenerateToken` não existe

- [ ] **Step 3: Write minimal implementation**

`LoginResponseDto.cs` — adicionar:

```csharp
public UsuarioPerfil Perfil { get; set; }
public Guid? EmpresaId { get; set; }
public string EmpresaDominio { get; set; } = string.Empty;
```

`ITokenService.cs`:

```csharp
string GenerateToken(Guid usuarioId, string email, UsuarioPerfil perfil, Guid? empresaId);
```

`TokenService.cs` — atualizar `GenerateToken` para adicionar claims `perfil` e `empresa_id` (quando não null).

Atualizar chamadas existentes de `GenerateToken` (buscar com grep e corrigir).

`UsuarioProfile.cs` — mapear `Perfil` e `EmpresaDominio` de `UsuarioAuthResult`.

- [ ] **Step 4: Run tests**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "TokenServiceTests" -v n`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Application tests/Beneficios.Tests/Application/TokenServiceTests.cs
git commit -m "feat(auth): add perfil and empresa_id JWT claims"
```

---

## Task 3: Backend — Login com validação tenant + perfil

**Files:**
- Modify: `src/Beneficios.Domain/Models/UsuarioAuthResult.cs` (+ `EmpresaDominio`)
- Modify: `src/Beneficios.Infrastructure/Repositories/UsuarioRepository.cs`
- Modify: `src/Beneficios.Application/Interfaces/IUsuarioService.cs`
- Modify: `src/Beneficios.Application/Services/UsuarioService.cs`
- Modify: `src/Beneficios.Api/Controllers/UsuariosController.cs`
- Modify: `tests/Beneficios.Tests/Application/UsuarioServiceTests.cs`
- Modify: `tests/Beneficios.Tests/Api/UsuariosControllerTests.cs`

**Interfaces:**
- Consumes: `GenerateToken(..., perfil, empresaId)` from Task 2
- Produces: `Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string tenantSubdomain)`

- [ ] **Step 1: Write failing tests**

`UsuarioServiceTests.cs` — adicionar:

```csharp
[Fact]
public async Task LoginAsync_EmAdmin_ComPerfilEmpresa_DeveRetornarNull()
{
    var authResult = new UsuarioAuthResult
    {
        Id = Guid.NewGuid(), Email = "u@test.com", Senha = Criptografia.Encrypt("senha123"),
        Perfil = UsuarioPerfil.Empresa, EmpresaDominio = "exemplo"
    };
    _repositoryMock.Setup(x => x.GetByEmailAsync("u@test.com")).ReturnsAsync(authResult);

    var result = await _service.LoginAsync(new LoginDto("u@test.com", "senha123"), "admin");

    Assert.Null(result);
}

[Fact]
public async Task LoginAsync_EmTenant_ComDominioDiferente_DeveRetornarNull()
{
    var authResult = new UsuarioAuthResult
    {
        Id = Guid.NewGuid(), Email = "u@test.com", Senha = Criptografia.Encrypt("senha123"),
        Perfil = UsuarioPerfil.Empresa, EmpresaDominio = "exemplo"
    };
    _repositoryMock.Setup(x => x.GetByEmailAsync("u@test.com")).ReturnsAsync(authResult);

    var result = await _service.LoginAsync(new LoginDto("u@test.com", "senha123"), "outra");

    Assert.Null(result);
}

[Fact]
public async Task LoginAsync_EmAdmin_ComPerfilAdmin_DeveRetornarToken()
{
    var authResult = new UsuarioAuthResult
    {
        Id = Guid.NewGuid(), Nome = "Admin", Email = "admin@test.com",
        Senha = Criptografia.Encrypt("admin123"), Perfil = UsuarioPerfil.Admin
    };
    _repositoryMock.Setup(x => x.GetByEmailAsync("admin@test.com")).ReturnsAsync(authResult);
    _tokenServiceMock.Setup(x => x.GenerateToken(authResult.Id, authResult.Email, UsuarioPerfil.Admin, null))
        .Returns("jwt-token");

    var result = await _service.LoginAsync(new LoginDto("admin@test.com", "admin123"), "admin");

    Assert.NotNull(result);
    Assert.Equal("jwt-token", result!.Token);
}
```

- [ ] **Step 2: Run tests — expect FAIL**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "LoginAsync_Em" -v n`

- [ ] **Step 3: Implement**

Regras em `UsuarioService.LoginAsync`:
- `tenantSubdomain == "admin"` → exige `Perfil == Admin`
- outro subdomínio → exige `Perfil == Empresa` e `EmpresaDominio == tenantSubdomain` (case-insensitive)

`UsuarioRepository.GetByEmailAsync` — SQL join `empresas` para trazer `e.dominio AS EmpresaDominio` e `u.perfil`.

`UsuariosController.Login` — ler header `X-Tenant` (fallback: `"admin"` se host começa com admin):

```csharp
var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
    ?? ExtractSubdomain(Request.Host.Host);
var response = await _usuarioService.LoginAsync(loginDto, tenant);
```

- [ ] **Step 4: Run all tests + coverage**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj -v n`
Run: `powershell -File scripts/check-coverage.ps1`
Expected: all PASS, coverage ≥ 80%

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(auth): validate login by tenant subdomain and perfil"
```

---

## Task 4: Backend — CORS

**Files:**
- Modify: `src/Beneficios.Api/Program.cs`
- Create: `tests/Beneficios.Tests/Api/CorsConfigurationTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
public class CorsConfigurationTests
{
    [Fact]
    public void CorsPolicy_DevePermitirLocalhost4200()
    {
        // WebApplicationFactory ou reflection na policy registrada
        // Assert policy "FrontPolicy" exists and allows http://admin.localhost:4200
    }
}
```

*(Implementar com `WebApplicationFactory<Program>` se já existir padrão; senão teste de integração mínimo verificando header `Access-Control-Allow-Origin` em OPTIONS.)*

- [ ] **Step 2: Implement CORS in Program.cs**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy => policy
        .SetIsOriginAllowed(origin =>
            origin.Contains(".localhost:4200", StringComparison.OrdinalIgnoreCase) ||
            origin.EndsWith(".minhaempresa.com.br", StringComparison.OrdinalIgnoreCase))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// após var app = builder.Build(); antes de UseAuthentication:
app.UseCors("FrontPolicy");
```

- [ ] **Step 3: Verify**

Run: `dotnet test` + `scripts/check-coverage.ps1`

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(api): add CORS policy for Angular front subdomains"
```

---

## Task 5: Scaffold Angular 22 + Material + dependências

**Files:**
- Create: `Beneficios.Front/` (ng new)
- Modify: `Beneficios.Front/package.json`
- Create: `Beneficios.Front/src/environments/environment.ts`
- Create: `Beneficios.Front/src/environments/environment.development.ts`

- [ ] **Step 1: Create project**

Run from repo root:

```bash
ng new Beneficios.Front --directory Beneficios.Front --routing --style=scss --ssr=false --standalone
cd Beneficios.Front
ng add @angular/material --theme=custom --typography=true --animations=enabled
npm install chart.js ng2-charts xlsx jspdf jspdf-autotable
```

- [ ] **Step 2: Configure environments**

`environment.development.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  baseDomain: 'localhost',
};
```

- [ ] **Step 3: Custom theme in styles.scss**

Primary `#1565C0`, background `#F5F7FA`, success `#2E7D32` (Material custom theme).

- [ ] **Step 4: Verify build**

Run: `cd Beneficios.Front && npm run build`
Expected: SUCCESS

- [ ] **Step 5: Commit**

```bash
git add Beneficios.Front
git commit -m "feat(front): scaffold Angular 22 with Material and dependencies"
```

---

## Task 6: TenantService + unit tests

**Files:**
- Create: `Beneficios.Front/src/app/core/tenant/tenant.service.ts`
- Create: `Beneficios.Front/src/app/core/tenant/tenant.service.spec.ts`

**Interfaces:**
- Produces: `TenantService.getSubdomain(): string`, `getMode(): 'admin' | 'tenant'`, `isAdminMode(): boolean`

- [ ] **Step 1: Write failing tests**

```typescript
describe('TenantService', () => {
  it('admin.localhost deve retornar modo admin', () => {
    spyOnProperty(window, 'location').and.returnValue({ hostname: 'admin.localhost' } as Location);
    expect(service.getSubdomain()).toBe('admin');
    expect(service.getMode()).toBe('admin');
    expect(service.isAdminMode()).toBeTrue();
  });

  it('empresa1.localhost deve retornar modo tenant', () => {
    spyOnProperty(window, 'location').and.returnValue({ hostname: 'empresa1.localhost' } as Location);
    expect(service.getSubdomain()).toBe('empresa1');
    expect(service.getMode()).toBe('tenant');
  });
});
```

- [ ] **Step 2: Implement TenantService**

Parse hostname: `{subdomain}.{baseDomain}` usando `environment.baseDomain`.

- [ ] **Step 3: Run tests**

Run: `cd Beneficios.Front && npm test -- --include=**/tenant.service.spec.ts --browsers=ChromeHeadless --watch=false`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(front): add TenantService with subdomain detection"
```

---

## Task 7: Auth core (service, interceptor, guards) + tests

**Files:**
- Create: `Beneficios.Front/src/app/core/auth/auth.service.ts`
- Create: `Beneficios.Front/src/app/core/auth/auth.interceptor.ts`
- Create: `Beneficios.Front/src/app/core/auth/auth.guard.ts`
- Create: `Beneficios.Front/src/app/core/auth/admin.guard.ts`
- Create: `Beneficios.Front/src/app/core/auth/tenant.guard.ts`
- Create: `Beneficios.Front/src/app/core/auth/guest.guard.ts`
- Create: `*.spec.ts` para cada um
- Modify: `Beneficios.Front/src/app/app.config.ts`

**Interfaces:**
- Produces: `AuthService.login(email, senha): Observable<LoginResponse>`
- Produces: `AuthService.logout(): void`, `isAuthenticated(): boolean`, `currentUser(): UserSession | null`
- Session keys: `beneficios_token`, `beneficios_user`

- [ ] **Step 1: Write failing tests** (auth.service.spec.ts, auth.interceptor.spec.ts, guards)

- [ ] **Step 2: Implement AuthService** — POST `{apiUrl}/usuarios/login` com header `X-Tenant` from TenantService

- [ ] **Step 3: Implement authInterceptor** — clone request with `Authorization` + `X-Tenant`; on 401 call logout

- [ ] **Step 4: Implement guards** conforme spec seção 3

- [ ] **Step 5: Register in app.config.ts** — `provideHttpClient(withInterceptors([authInterceptor]))`

- [ ] **Step 6: Run unit tests**

- [ ] **Step 7: Commit**

```bash
git commit -m "feat(front): add auth service, interceptor and route guards"
```

---

## Task 8: Layout shell (hambúrguer, responsivo)

**Files:**
- Create: `Beneficios.Front/src/app/layout/shell/shell.component.ts`
- Create: `Beneficios.Front/src/app/layout/shell/shell.component.html`
- Create: `Beneficios.Front/src/app/layout/shell/shell.component.scss`

- [ ] **Step 1: Implement ShellComponent**

- `MatSidenav` mode `over` when `(max-width: 959px)` else `side`
- Hamburger toggles sidenav; item click closes on mobile
- Toolbar: "Benefícios", subdomain label, user menu with logout
- Sidebar items from `@Input() navItems` based on TenantService mode

- [ ] **Step 2: Verify manually**

Run: `ng serve --host 0.0.0.0 --disable-host-check`
Open: `http://admin.localhost:4200`

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add responsive shell layout with hamburger sidenav"
```

---

## Task 9: Login feature

**Files:**
- Create: `Beneficios.Front/src/app/features/auth/login/login.component.ts`
- Modify: `Beneficios.Front/src/app/app.routes.ts`

- [ ] **Step 1: LoginComponent** — Reactive form (email, senha), MatCard full-page, error messages PT

- [ ] **Step 2: Routes**

Admin mode: `/login` (guestGuard), `/empresas` (authGuard + adminGuard)
Tenant mode: `/login`, `/dashboard` (authGuard + tenantGuard)

Post-login redirect: admin → `/empresas`, tenant → `/dashboard`

- [ ] **Step 3: Validate against API** with backend running

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(front): add login screen with admin/tenant routing"
```

---

## Task 10: Dashboard mock (Chart.js)

**Files:**
- Create: `Beneficios.Front/src/app/features/dashboard/dashboard-mock.service.ts`
- Create: `Beneficios.Front/src/app/features/dashboard/dashboard.component.ts`
- Create: `dashboard-mock.service.spec.ts`

- [ ] **Step 1: DashboardMockService** — barras 6 meses transporte; pizza alimentação 52/35/13; totais R$

- [ ] **Step 2: DashboardComponent** — 2 cards responsivos, ng2-charts bar + pie, cores spec (#1565C0, #43A047, #FB8C00, #29B6F6)

- [ ] **Step 3: Unit test mock service structure**

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(front): add dashboard with mock bar and pie charts"
```

---

## Task 11: CRUD Usuários (tenant)

**Files:**
- Create: `Beneficios.Front/src/app/core/api/usuario.service.ts`
- Create: `Beneficios.Front/src/app/features/usuarios/usuario-list/`
- Create: `Beneficios.Front/src/app/features/usuarios/usuario-form/`

- [ ] **Step 1: UsuarioService** — GET/POST/PUT/DELETE `/usuarios`

- [ ] **Step 2: List** — MatTable desktop; card list mobile; paginação MatPaginator

- [ ] **Step 3: Form** — nome, email, senha; EmpresaId from auth session (not shown)

- [ ] **Step 4: Confirm delete** — MatDialog

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(front): add usuarios CRUD for tenant mode"
```

---

## Task 12: CRUD Empresas (admin)

**Files:**
- Create: `Beneficios.Front/src/app/core/api/empresa.service.ts`
- Create: `Beneficios.Front/src/app/features/empresas/empresa-list/`
- Create: `Beneficios.Front/src/app/features/empresas/empresa-form/`

- [ ] **Step 1: EmpresaService** — GET/POST/PUT/DELETE `/empresas`

- [ ] **Step 2: List + Form** — campos spec (RazaoSocial, Dominio, NomeBanco, UsuarioBanco, SenhaBanco)

- [ ] **Step 3: Commit**

```bash
git commit -m "feat(front): add empresas CRUD for admin mode"
```

---

## Task 13: Export Excel/PDF + tests

**Files:**
- Create: `Beneficios.Front/src/app/shared/utils/export.service.ts`
- Create: `Beneficios.Front/src/app/shared/utils/export.service.spec.ts`
- Modify: list components (usuarios, empresas)

- [ ] **Step 1: ExportService**

```typescript
exportToExcel(rows: Record<string, unknown>[], columns: ExportColumn[], filename: string): void
exportToPdf(rows: Record<string, unknown>[], columns: ExportColumn[], filename: string, title: string): void
```

Filename: `{entidade}-{subdomain}-{yyyy-MM-dd}.xlsx`

- [ ] **Step 2: Unit tests** — verify column headers and row count in generated output (mock blob)

- [ ] **Step 3: Wire buttons** top-right of list screens; icons on mobile, icon+text on tablet+

- [ ] **Step 4: Commit**

```bash
git commit -m "feat(front): add Excel and PDF export on listing screens"
```

---

## Task 14: Backend — TenantMiddleware (fase 3)

**Files:**
- Create: `src/Beneficios.Api/Middleware/TenantMiddleware.cs`
- Create: `src/Beneficios.Application/Interfaces/ITenantResolver.cs`
- Create: `src/Beneficios.Application/Services/TenantResolver.cs`
- Modify: `src/Beneficios.Api/Program.cs`
- Create: `tests/Beneficios.Tests/Application/TenantResolverTests.cs`

- [ ] **Step 1: TenantResolver** — read `X-Tenant` or Host subdomain → query `empresas` by `dominio` → return connection string

- [ ] **Step 2: TenantMiddleware** — set tenant connection in `HttpContext.Items` or scoped `IDbConnection` factory

- [ ] **Step 3: Tests** — unit + PostgresFixture integration

- [ ] **Step 4: Coverage ≥ 80%**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(api): add tenant middleware for dynamic DB connection"
```

---

## Task 15: Polish — README + error handling + E2E (fase 4)

**Files:**
- Create: `Beneficios.Front/README.md`
- Modify: error handler interceptor or central HTTP error handler
- Create: `Beneficios.Front/e2e/` (Playwright)

- [ ] **Step 1: MatSnackBar** for 401/403/404/500 per spec

- [ ] **Step 2: README** — hosts file, subdomains, ng serve, dotnet run

- [ ] **Step 3: Playwright** — 5 flows from spec section 9

- [ ] **Step 4: Commit**

```bash
git commit -m "docs(front): add README and Playwright E2E smoke tests"
```

---

## Spec Coverage Checklist

| Spec requirement | Task |
|------------------|------|
| Angular 22 + Material + Chart.js | 5, 10 |
| Beneficios.Front folder | 5 |
| Multitenancy subdomain | 6, 14 |
| admin.* vs tenant.* | 6, 7, 9 |
| Dashboard mock barras + pizza | 10 |
| CRUD usuarios | 11 |
| CRUD empresas admin | 12 |
| JWT + X-Tenant | 3, 7 |
| CORS | 4 |
| Perfil + JWT claims | 1, 2, 3 |
| Export Excel/PDF | 13 |
| Mobile first hambúrguer | 8 |
| Backend tests ≥ 80% | 1-4, 14 |
| Front core unit tests | 6, 7, 10, 13 |
| E2E Playwright | 15 |
| Redis out of scope | — |
| Backend patterns | all backend tasks |

---

## Verification Commands (final)

```bash
# Backend
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj
powershell -File scripts/check-coverage.ps1

# Frontend
cd Beneficios.Front
npm test -- --watch=false --browsers=ChromeHeadless
npm run build
npx playwright test   # after Task 15
```
