# Beneficios.Front

Frontend Angular 22 multitenant do sistema Benefícios. Cada empresa acessa pelo subdomínio; o modo admin gerencia tenants em `admin.*`.

## Pré-requisitos

- Node.js **20 LTS** (use `.nvmrc`: `nvm use` ou `fnm use`)
- .NET 9 SDK (API)
- PostgreSQL (ou Docker Compose na raiz do repositório)

## Configurar hosts (Windows)

Edite `C:\Windows\System32\drivers\etc\hosts` como administrador:

```
127.0.0.1  admin.localhost
127.0.0.1  empresa1.localhost
```

## Subdomínios

| URL | Modo | Rotas principais |
|-----|------|------------------|
| `http://admin.localhost:4200` | Admin | `/login`, `/empresas` |
| `http://empresa1.localhost:4200` | Tenant | `/login`, `/dashboard`, `/usuarios` |

## Backend

Na raiz do repositório:

```bash
dotnet run --project src/Beneficios.Api
```

API em desenvolvimento: **http://localhost:5000/api** (HTTP fixo em Development). O front chama essa URL diretamente; o proxy (`proxy.conf.json`) fica como fallback.

## Frontend

```bash
cd Beneficios.Front
npm install
npx ng serve --host 0.0.0.0 --disable-host-check
```

Acesse:

- Admin: http://admin.localhost:4200/login
- Tenant: http://empresa1.localhost:4200/login

## Scripts

| Comando | Descrição |
|---------|-----------|
| `npm start` | `ng serve` (localhost apenas) |
| `npm run build` | Build de produção |
| `npm test` | Testes unitários (Karma/Jasmine) |
| `npm run e2e` | Testes E2E (Playwright, API mockada) |

## Testes E2E

Os testes Playwright em `e2e/` mockam a API HTTP — não é obrigatório subir o backend para executá-los.

```bash
cd Beneficios.Front
npm run e2e
```

Para validar contra API real, suba o backend e remova/comente os mocks em `e2e/fixtures/api-mocks.ts`.

## Tratamento de erros HTTP

O interceptor `httpErrorInterceptor` exibe `MatSnackBar` para:

| Status | Mensagem |
|--------|----------|
| 401 | Sessão expirada + redirect `/login` |
| 403 | Sem permissão |
| 404 | Registro não encontrado |
| 500 | Erro interno |

Erros de login (401 em `/usuarios/login`) são tratados na tela de login.

## Estrutura

```
src/app/
  core/       auth, tenant, api, http
  features/   login, dashboard, usuarios, empresas
  layout/     shell responsivo
  shared/     dialogs, export utils
```
