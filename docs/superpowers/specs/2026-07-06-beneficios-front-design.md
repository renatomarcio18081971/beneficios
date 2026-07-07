# Beneficios.Front — Design Spec

**Data:** 2026-07-06  
**Status:** Aprovado em brainstorming  
**Escopo:** Frontend Angular 22 + ajustes mínimos no backend (CORS, JWT, Perfil)

---

## 1. Contexto

Sistema de gestão de benefícios com arquitetura **multitenant** por subdomínio. Cada empresa (tenant) possui subdomínio próprio e conexão de banco isolada. O backend .NET 9 já expõe CRUD de usuários e empresas com JWT; o frontend será criado em `Beneficios.Front/` no mesmo repositório.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Stack front | Angular 22 + Angular Material + Chart.js |
| Pasta | `Beneficios.Front/` no mesmo repo |
| Multitenancy | Subdomínio → `empresas.dominio` → conexão do banco |
| Isolamento | Empresa nunca vê dados de outra |
| Admin | Acessa `admin.*` para gestão de tenants; para ver dados de uma empresa, entra no subdomínio dela |
| Dashboard | Gráfico de barras (vale transporte) + pizza (vale alimentação), dados mockados |
| Cadastro empresas | Modo Admin em `admin.*` |
| Cadastro usuários | CRUD completo nos subdomínios das empresas |
| Auth | JWT + CORS no backend |
| UI | Mobile first, responsivo (celular, tablet, PC, TV) |
| Menu | Hambúrguer toggle (abre/fecha sidenav) |
| Listagens | Botões Export Excel e Export PDF à direita |
| Redis | **Fora do escopo fase 1** (YAGNI); considerar fase 2 para cache de tenant/dashboard |
| Backend | Seguir fielmente padrões existentes + testes unitários em toda alteração |
| Cobertura back | Mínimo **80%** (SonarQube + `scripts/check-coverage.ps1`) |
| Testes front | **Opção A:** unitários no core (fase 1) + E2E fluxos críticos (fase 2) |
| Cobertura front | Meta **60–70%** em services/guards/utils (sem exigência em templates) |

---

## 2. Arquitetura

### Abordagem: app única com detecção de tenant por hostname

Um único projeto Angular detecta o contexto pelo `hostname`:

- `admin.minhaempresa.com.br` → modo **Admin** (gestão de empresas/tenants)
- `{tenant}.minhaempresa.com.br` → modo **Tenant** (dashboard + usuários)

```
Beneficios.Front/
├── src/app/
│   ├── core/                    # Singletons
│   │   ├── auth/                # AuthService, JWT interceptor, guards
│   │   ├── tenant/              # TenantService (lê subdomínio)
│   │   └── api/                 # HttpClient base, services
│   ├── shared/                  # Componentes reutilizáveis (export, dialogs)
│   ├── features/
│   │   ├── auth/                # Login
│   │   ├── dashboard/           # Gráficos mockados (Chart.js)
│   │   ├── usuarios/            # CRUD usuários (modo tenant)
│   │   └── empresas/            # CRUD empresas (modo admin)
│   ├── layout/                  # Shell com sidenav Material
│   └── app.config.ts
```

### Fluxo multitenant

```
Browser → empresa1.dominio.com
       → TenantService extrai "empresa1"
       → Header X-Tenant: empresa1 em toda requisição API
       → Backend resolve empresa + conexão DB
       → Dados isolados da tenant
```

### Modos da aplicação

| Hostname | Modo | Rotas |
|----------|------|-------|
| `admin.*` | Admin | `/login`, `/empresas`, `/empresas/nova`, `/empresas/:id/editar` |
| `{tenant}.*` | Tenant | `/login`, `/dashboard`, `/usuarios`, `/usuarios/novo`, `/usuarios/:id/editar` |

### Dev local

```
127.0.0.1  admin.localhost
127.0.0.1  empresa1.localhost

http://admin.localhost:4200     → modo Admin
http://empresa1.localhost:4200  → modo Tenant
```

API dev: `http://localhost:5000/api`

---

## 3. Telas e navegação

### Layout compartilhado

- **Angular Material:** `MatSidenav` + `MatToolbar` + área de conteúdo
- **Menu hambúrguer:** toggle abre/fecha sidenav; clicar fora fecha (modo `over` no mobile)
- **Toolbar:** nome do app, subdomínio, menu usuário (logout)
- **Login:** full-page, sem sidebar
- **Interface:** português

### Comportamento responsivo (mobile first)

| Dispositivo | Largura | Comportamento |
|-------------|---------|---------------|
| Celular | < 600px | 1 coluna, sidenav `over`, fechado por padrão |
| Tablet | 600–960px | Dashboard 2 colunas, tabelas com scroll horizontal |
| PC | 960–1920px | Sidenav `side`, aberto por padrão |
| TV | > 1920px | Fontes e espaçamentos ampliados, alvos de toque 48px+ |

**Tabelas no celular:** lista em cards em vez de tabela estreita.  
**Export no celular:** ícones + tooltip; tablet+ ícone + texto.

### Modo Admin (`admin.*`)

**Sidebar:** Empresas

**Login (`/login`):** email, senha. Rejeita usuários sem perfil Admin.

**Listagem empresas (`/empresas`):**
- MatTable paginada: Razão Social, Domínio, Data inclusão, Ações
- Botões à direita: Export Excel, Export PDF
- Ações: Editar, Excluir (confirmação)

**Formulário empresa (nova/editar):**

| Campo | Obrigatório |
|-------|-------------|
| Razão Social | Sim |
| Domínio | Sim (ex: `empresa1`) |
| Nome do Banco | Sim |
| Usuário do Banco | Sim |
| Senha do Banco | Sim (edição: vazio = manter) |

### Modo Tenant (`{tenant}.*`)

**Sidebar:** Dashboard, Usuários

**Login (`/login`):** email, senha. Rejeita usuário de outra tenant.

**Dashboard (`/dashboard`):**

| Card | Tipo | Dados mock |
|------|------|------------|
| Vale Transporte | Barras | Gasto últimos 6 meses |
| Vale Alimentação | Pizza | Refeições 52%, Mercado 35%, Outros 13% |

Totais em R$ em destaque no topo de cada card. Títulos em português simples. Dados via `DashboardMockService` (sem API).

**Paleta:**

| Uso | Cor | Hex |
|-----|-----|-----|
| Primária | Azul calmo | `#1565C0` |
| Fundo | Cinza claro | `#F5F7FA` |
| Texto | Cinza escuro | `#263238` |
| Totais | Verde | `#2E7D32` |
| Barras | Azul | `#1565C0` |
| Pizza | Verde / Laranja / Azul claro | `#43A047` / `#FB8C00` / `#29B6F6` |

**Listagem usuários (`/usuarios`):**
- MatTable: Nome, Email, Data inclusão, Ações
- Export Excel + PDF à direita
- CRUD completo

**Formulário usuário:**

| Campo | Obrigatório |
|-------|-------------|
| Nome | Sim |
| Email | Sim |
| Senha | Sim (cadastro); opcional na edição |

`EmpresaId` inferido do subdomínio — não exibido no formulário.

### Exportação (listagens)

- Client-side: `xlsx` (Excel), `jspdf` + `jspdf-autotable` (PDF)
- Exporta colunas visíveis (sem Ações)
- Nome: `{entidade}-{subdominio}-{data}.xlsx|pdf`

### Guards

| Guard | Função |
|-------|--------|
| `authGuard` | Redireciona `/login` se não autenticado |
| `adminGuard` | Modo Admin; exige Perfil Admin |
| `tenantGuard` | Modo Tenant; exige usuário da tenant |
| `guestGuard` | `/login`: se logado, redireciona home |

---

## 4. Auth, API e multitenancy

### Fluxo JWT

1. Usuário envia email + senha
2. Front envia `POST /api/usuarios/login` com header `X-Tenant`
3. API valida credenciais, perfil e tenant
4. Resposta: `{ token, usuarioId, nome, email, perfil, empresaId }`
5. Token em `sessionStorage` (`beneficios_token`)
6. Interceptor adiciona `Authorization: Bearer {token}` e `X-Tenant: {subdominio}`

### Regras de login

| Subdomínio | Perfil aceito | Redirect |
|------------|---------------|----------|
| `admin.*` | Admin | `/empresas` |
| `{tenant}.*` | Empresa da tenant | `/dashboard` |
| `{tenant}.*` | Admin ou usuário de outra tenant | Rejeitar |

### Endpoints

**Admin:** `POST /usuarios/login`, `GET|POST|PUT|DELETE /empresas`

**Tenant:** `POST /usuarios/login`, `GET|POST|PUT|DELETE /usuarios`

**Dashboard:** mock local (sem endpoint fase 1)

### Ajustes backend (fase 1)

1. Campo `Perfil` em `usuarios` (`Admin` | `Empresa`)
2. JWT claims: `perfil`, `empresa_id`
3. `LoginResponseDto` estendido
4. CORS para `*.localhost:4200` (dev) e `*.minhaempresa.com.br` (prod)
5. Login valida perfil + tenant
6. `TenantMiddleware` (fase 1b): resolve conexão DB por `X-Tenant`/hostname

> **Regra:** cada item acima exige implementação alinhada aos padrões da seção 9 e testes com cobertura compatível com Sonar (≥ 80%).

### Redis

Fora do escopo fase 1. Fase 2 futura: cache de resolução de tenant e agregações do dashboard.

### Tratamento de erros

| Status | Front |
|--------|-------|
| 400 | Erros no formulário |
| 401 | Logout + `/login` + snackbar sessão expirada |
| 403 | Snackbar sem permissão |
| 404 | Snackbar registro não encontrado |
| 500 | Snackbar erro interno |

Feedback via `MatSnackBar`.

### Environments

```typescript
// development
{ apiUrl: 'http://localhost:5000/api', baseDomain: 'localhost' }

// production
{ apiUrl: 'https://api.minhaempresa.com.br/api', baseDomain: 'minhaempresa.com.br' }
```

---

## 5. Dependências npm

| Pacote | Uso |
|--------|-----|
| `@angular/material` | UI |
| `@angular/cdk` | BreakpointObserver |
| `chart.js` + `ng2-charts` | Gráficos |
| `xlsx` | Export Excel |
| `jspdf` + `jspdf-autotable` | Export PDF |

---

## 6. Plano de entrega

### Fase 1 — Fundação
- Scaffold Angular 22 em `Beneficios.Front/`
- Tema Material + layout responsivo
- TenantService
- Backend: CORS + Perfil + claims JWT **+ testes (Application, Api) + cobertura ≥ 80%**
- AuthService + interceptor + guards

### Fase 2 — Telas
- Login (admin + tenant)
- Dashboard mockado
- CRUD usuários e empresas
- Export Excel/PDF
- **Testes unitários do core front** (TenantService, AuthService, guards, interceptor, export)

### Fase 3 — Multitenancy backend
- TenantMiddleware **+ testes unitários**
- Validação login por tenant **+ testes de cenários Admin/Tenant**
- Conexão dinâmica por tenant **+ testes Infrastructure (PostgresFixture)**
- Verificar `check-coverage.ps1` ≥ 80% antes de merge

### Fase 4 — Polish
- Testes responsivos
- Tratamento de erros
- README setup local
- **E2E (Playwright):** login tenant/admin, CRUD básico, logout/401, export

### Fora do escopo fase 1
- Redis
- Dashboard com API real
- Testes E2E (previstos fase 4)
- Testes unitários de telas/templates (Material, layout)

---

## 7. Ordem de implementação

```
Backend (CORS + Perfil + JWT)
    ↓
Scaffold Angular + Material + TenantService
    ↓
Auth (login + guards + interceptor)
    ↓
Layout (shell hambúrguer responsivo)
    ↓
Dashboard mock + CRUD usuários + CRUD empresas
    ↓
Export Excel/PDF
    ↓
TenantMiddleware (backend)
    ↓
Polish + README
```

---

## 8. Padrões backend e qualidade (Sonar)

Toda alteração no backend **deve seguir fielmente** os padrões já adotados no repositório. Não introduzir estilos, bibliotecas ou abordagens divergentes sem necessidade.

### Arquitetura e convenções (obrigatório)

| Camada | Padrão existente |
|--------|------------------|
| **Domain** | Entities, `*Params` em `Models/`, interfaces em `Interfaces/` |
| **Application** | Services, DTOs (`record` quando aplicável), interfaces, AutoMapper (`*Profile` por entidade) |
| **Infrastructure** | Repositories com Dapper, scripts SQL em `Scripts/` |
| **Api** | Controllers enxutos, `[Authorize]`/`[AllowAnonymous]`, logs Serilog, tratamento de exceções consistente |

**Nomenclatura (manter):** `SalvarAsync`, `AtualizarAsync`, `ObterUmAsync`, `ObterTodosAsync`, `DeleteAsync`, DTOs `*SalvarDto` / `*AtualizarDto`.

**Novos artefatos seguem o mesmo fluxo:**
```
DTO → Service → Repository → SQL Script (se schema)
         ↓
    *Profile (AutoMapper) + *Params (Domain)
```

### Testes unitários (obrigatório em toda mudança no back)

| Camada | Ferramenta | Padrão |
|--------|------------|--------|
| Application | xUnit + Moq | Mock de repositórios/interfaces; testes de sucesso, falha e borda |
| Api | xUnit + Moq | Controllers via `ControllerTestHelper`; status HTTP e payloads |
| Domain | xUnit | Entidades e value objects |
| Infrastructure | xUnit + PostgreSQL | `PostgresFixture` / `PostgresCollection` quando envolver banco |

**Convenções de teste (manter):**
- Classe `{Classe}Tests` no espelho da estrutura (`Application/`, `Api/`, `Domain/`, `Infrastructure/`)
- Métodos `{Metodo}_{Cenario}_Deve{Resultado}` (ex.: `LoginAsync_ComPerfilAdmin_DeveRetornarToken`)
- Arrange / Act / Assert explícitos
- Verificar mocks com `Verify(..., Times.Once)` quando aplicável

### Cobertura SonarQube (≥ 80%)

- **Threshold:** 80% de cobertura de linhas (sequência)
- **Validação local:** `scripts/check-coverage.ps1` (Coverlet OpenCover)
- **CI/Sonar:** `sonar-project.properties` + `coverage.opencover.xml`
- **Exclusões Sonar (existentes):** `Program.cs`, `*Dto.cs`, `*Params.cs`, `Migrations/`

**Antes de concluir qualquer task de backend:**
1. `dotnet test` — todos os testes passando
2. `scripts/check-coverage.ps1` — cobertura ≥ 80%
3. Novos arquivos de produção sem teste correspondente devem ser evitados (services, middleware, validações de login/tenant)

### Checklist por feature backend

- [ ] Script SQL (se alterar schema) em `Infrastructure/Scripts/`
- [ ] Entity / Params / DTO / Profile atualizados
- [ ] Service + Repository seguindo padrão existente
- [ ] Controller apenas orquestra (sem lógica de negócio)
- [ ] Testes Application + Api (mínimo); Infrastructure se tocar repositório/SQL
- [ ] Cobertura ≥ 80% verificada

---

## 9. Testes frontend (Opção A)

### Fase 1 — unitários no core (obrigatório)

| Área | O que testar |
|------|--------------|
| `TenantService` | `admin.localhost` → Admin; `empresa1.localhost` → tenant + subdomínio |
| `AuthService` | login persiste token; logout limpa sessão |
| `authInterceptor` | headers `Authorization: Bearer` + `X-Tenant` |
| Guards | `adminGuard`, `tenantGuard`, `authGuard`, `guestGuard` |
| Export utils | Excel/PDF com colunas corretas |
| `DashboardMockService` | estrutura dos dados mock |

**Ferramenta:** Jasmine/Karma (padrão Angular CLI) ou Vitest se configurado no scaffold.

**Convenção:** `{Classe}.spec.ts` colocado ao lado do arquivo testado.

**Meta de cobertura:** 60–70% em `core/` e `shared/utils/` (templates e SCSS excluídos).

**Não testar na fase 1:** renderização de telas, Material, gráficos Chart.js, CSS responsivo.

### Fase 2/4 — E2E (fluxos críticos)

**Ferramenta sugerida:** Playwright

| # | Fluxo |
|---|-------|
| 1 | Login tenant → redirect `/dashboard` |
| 2 | Login admin → redirect `/empresas` |
| 3 | Criar usuário (tenant) |
| 4 | Logout / 401 → redirect `/login` |
| 5 | Export Excel na listagem |

E2E executado após telas estáveis; requer API rodando ou mock HTTP.

---

## 10. Próximo passo

Após aprovação deste spec → skill **writing-plans** para plano de implementação detalhado.
