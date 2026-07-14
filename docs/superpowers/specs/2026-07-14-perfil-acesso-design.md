# Perfil de Acesso (tenant) — Design Spec

**Data:** 2026-07-14  
**Status:** Aprovado em brainstorming  
**Escopo:** Cadastro de perfil por empresa (schema tenant), matriz de permissões por módulo/menu, vínculo 1:1 opcional no usuário, enforcement front + API, provisionamento do usuário dono

---

## 1. Contexto

O sistema Beneficios já possui multitenancy por subdomínio: ao cadastrar uma empresa, cria-se um schema PostgreSQL e a tabela `usuarios` naquele schema. Existe cadastro de empresas (admin) e usuários (tenant), com telas list/form (filtrar, limpar, export PDF/Excel no front).

Hoje o enum `UsuarioPerfil` (`Admin` | `Empresa`) apenas distingue plataforma admin vs tenant. **Não** controla menus nem ações CRUD dentro da empresa.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Escopo | Somente schemas de **empresa (tenant)**; subdomain `admin` fora da v1 |
| Modelo | Abordagem **relacional**: `perfis` + `perfil_permissoes` + `usuarios.perfil_id` |
| Vínculo | Usuário tem **um** perfil (`perfil_id`); no banco pode ser null, mas **login exige perfil** |
| UI | Tela **Perfis** separada (padrão Usuários) + **select** no form de Usuário |
| Flags por módulo | **Visualizar**, **Criar**, **Editar**, **Excluir** |
| Visualizar | Mostra menu e tela em **consulta**; botões de escrita ocultos |
| Sem Visualizar | Item de menu **não renderiza** (oculto, não desabilitado) |
| Layout | Ocultar controles sem deixar gaps/elementos tortos; responsividade preservada |
| Enforcement | **Front + API** na mesma entrega |
| Menus v1 | `dashboard`, `usuarios`, `perfis` + catálogo extensível para módulos futuros |
| Provisionamento | Perfil **Dono** (todas permissões) vinculado ao usuário padrão do schema |
| Sem perfil no login | Não autentica; mensagem fixa (ver §5) |
| Encoding | **Todos** os arquivos novos/alterados: **UTF-8 com BOM** |
| Export | PDF/Excel só no front (como Usuários); tratados como parte de Visualizar |
| Fora da v1 | Perfis no admin; tabela de menus no banco; matriz dentro do JWT |

---

## 2. Separação de conceitos

| Conceito | Nome | Papel |
|----------|------|--------|
| Tipo de plataforma | Enum `UsuarioPerfil` (`Admin` / `Empresa`) | **Não muda** — gates de admin vs tenant |
| Perfil de acesso | Entidade `Perfil` / tabela `perfis` | Nome + matriz de permissões no tenant |
| Módulo/menu | `codigo_menu` no catálogo de código | Chave estável: `dashboard`, `usuarios`, `perfis`, … |

O claim JWT `perfil` continua sendo Admin/Empresa. O acesso fino usa `perfilId` + lista `permissoes` no payload de login/sessão; a API **revalida no banco**.

---

## 3. Arquitetura de dados

### 3.1 Tabelas no schema do tenant

```text
perfis
  id                     UUID PK
  nome                   VARCHAR NOT NULL
  eh_sistema             BOOLEAN NOT NULL DEFAULT FALSE  -- true = perfil Dono
  data_inclusao          TIMESTAMP NOT NULL
  data_alteracao         TIMESTAMP NULL
  usuario_alteracao_id   UUID NULL

perfil_permissoes
  id                     UUID PK (ou PK composta perfil_id + codigo_menu)
  perfil_id              UUID NOT NULL → perfis(id) ON DELETE CASCADE
  codigo_menu            VARCHAR NOT NULL   -- NÃO é FK para tabela de menus
  visualizar             BOOLEAN NOT NULL DEFAULT FALSE
  criar                  BOOLEAN NOT NULL DEFAULT FALSE
  editar                 BOOLEAN NOT NULL DEFAULT FALSE
  excluir                BOOLEAN NOT NULL DEFAULT FALSE
  UNIQUE (perfil_id, codigo_menu)

usuarios
  + perfil_id            UUID NULL → perfis(id)
```

Menus **não** são persistidos por tenant. O catálogo vive no código (Domain + espelho no Front).

### 3.2 Catálogo de módulos (contrato de crescimento)

Registro único no Domain (espelhado no Front), por exemplo:

```text
ModuloSistema {
  Codigo: "usuarios"        // chave imutável
  NomeExibicao: "Usuários"
  Rota: "/usuarios"
  AcoesSuportadas: Visualizar | Criar | Editar | Excluir
}
```

**v1:** `dashboard`, `usuarios`, `perfis`.

Dashboard na v1: tipicamente só **Visualizar** nas `AcoesSuportadas` (checkboxes Criar/Editar/Excluir não aparecem no form).

#### Como adicionar um módulo futuro (ex.: `beneficios`)

1. Entrada no catálogo (back + front)  
2. Feature/rota/controller do módulo  
3. Checagens `EnsureAsync("beneficios", acao)` na API  
4. Form de Perfil e menu **passam a listar a linha automaticamente**  
5. Provisionamento do Dono gera permissões full para **todos** os códigos do catálogo  

**Não exige:** novas colunas em `perfil_permissoes`, scripts por empresa, nem alterar shape da tabela.

#### Tenants já existentes após novo módulo

Ao carregar/editar um perfil antigo, códigos novos ausentes são tratados como `false`. Dono ou administrador de perfil concede depois. Sem migration de dados obrigatória.

### 3.3 Provisionamento (empresa nova)

Fluxo atual (`TenantProvisioner` + `TenantSchemaSql`) passa a:

1. `CREATE SCHEMA`  
2. `CREATE TABLE usuarios` (+ índices)  
3. `CREATE TABLE perfis` + `perfil_permissoes`  
4. Inserir perfil **Dono** (`eh_sistema = true`) com todas as flags `true` para cada código do catálogo  
5. Inserir usuário padrão com `perfil_id` = Dono  
6. Script de migração **idempotente** para schemas de empresas já criadas (criar tabelas, seed Dono se faltar, vincular usuário padrão se `perfil_id` null)

### 3.4 Regras de integridade

- Não excluir perfil com `eh_sistema = true`  
- Não excluir perfil com usuários vinculados (bloquear e exigir reassociação)  
- Não desvincular / não remover perfil do usuário padrão do tenant (`TenantDefaultUser`)  
- API ignora `codigo_menu` desconhecido; completa faltantes do catálogo com `false`

---

## 4. API, autenticação e enforcement

### 4.1 `PerfisController` — `api/perfis`

Padrão fiel a `UsuariosController` (Authorize, try/catch, mensagens em português, BOM).

| Método | Rota | Comportamento |
|--------|------|----------------|
| GET | `/` | Lista |
| GET | `/filtrar?nome=` | Filtro |
| GET | `/{id}` | Detalhe + matriz |
| POST | `/` | Cria perfil + permissões |
| PUT | `/{id}` | Atualiza nome + permissões |
| DELETE | `/{id}` | Exclui (regras §3.4) |

Vertical slice: DTOs, AutoMapper, `IPerfilService` / `PerfilService`, `IPerfilRepository` / `PerfilRepository`, testes Api/Application/Infrastructure.

Sem endpoints de export no backend.

### 4.2 Login

1. Validar acesso ao tenant (igual hoje: Admin vs Empresa + domínio)  
2. Se tenant e `perfil_id` nulo → **não** gera token; erro com mensagem fixa  
3. Carregar `perfil_permissoes`  
4. Estender `LoginResponseDto` / sessão front:
   - `perfilId`, `perfilNome` (acesso)  
   - `permissoes: [{ codigoMenu, visualizar, criar, editar, excluir }]`  
5. JWT mantém claim `perfil` = Admin/Empresa; **matriz não vai no JWT**

### 4.3 Mensagem fixa (login sem perfil)

```text
Usuário sem perfil configurado, procure o administrador do sistema !
```

### 4.4 `IPermissaoService`

```text
EnsureAsync(usuarioId, codigoMenu, AcaoPermissao)
// AcaoPermissao: Visualizar | Criar | Editar | Excluir
```

Falhou → **403** com mensagem clara (ex.: *"Sem permissão para esta operação."*).

Uso nos controllers tenant (exemplo Usuários):

| Endpoint | Checagem |
|----------|----------|
| GET / list / filtrar / id | `usuarios` + Visualizar |
| POST | `usuarios` + Criar |
| PUT | `usuarios` + Editar |
| DELETE | `usuarios` + Excluir |

`PerfisController` usa código `perfis`. Admin (`EmpresasController`) não usa essa matriz na v1.

### 4.5 Ajuste em Usuário

- DTOs salvar/atualizar e form: `perfilId`  
- Create/Update validam existência do perfil no tenant  
- Usuário sem perfil não consegue logar (campo efetivamente obrigatório para operação)

---

## 5. Frontend

### 5.1 Encoding

Todo arquivo criado ou alterado no Front/Back/Tests/Scripts: **UTF-8 com BOM**.

### 5.2 Telas

| Rota | Componente | Comportamento |
|------|------------|---------------|
| `/perfis` | `perfil-list` | Filtro (nome), Limpar, grid, export PDF/Excel, Novo/Editar/Excluir |
| `/perfis/novo` | `perfil-form` | Nome + matriz |
| `/perfis/:id/editar` | `perfil-form` | Perfil sistema: restrições (não excluir; edição segura) |

Espelho de pastas/nomenclatura de `features/usuarios/` e `core/api/usuario.*`.

### 5.3 Matriz no form

Linhas = catálogo; colunas = Visualizar | Criar | Editar | Excluir.  
Ações não suportadas pelo módulo: checkbox **não renderizado** (célula limpa, sem quebrar alinhamento).

### 5.4 Form Usuário

Select **Perfil** obrigatório (lista via `GET /perfis`), alinhado à regra de login.

### 5.5 Menu e UI

- `ShellComponent` (tenant): itens do catálogo filtrados por `visualizar === true`  
- Controles sem permissão: **`@if` / não renderizar** — nunca botão desabilitado “fantasma”  
- Coluna Ações some por completo se nenhuma ação restante; header/flex sem gaps  
- Helper/`can(codigo, acao)` + guard de rota exigindo Visualizar  
- 403 da API → feedback amigável  
- Login: exibir mensagem §4.3 quando aplicável  

### 5.6 Extensão futura no front

Novo módulo = entrada no catálogo + feature/rota + `can(...)`. Menu e form de Perfil acompanham sem refactor estrutural.

---

## 6. Plano de ação (fases)

| Fase | Conteúdo |
|------|----------|
| **0** | Catálogo Domain + Front; enum `AcaoPermissao`; regra BOM |
| **1** | DDL tenant + migração schemas existentes; provisionar Dono + vínculo usuário padrão; testes provisioner |
| **2** | Vertical slice CRUD Perfis (API → Domain → Infra → testes) |
| **3** | Login + `IPermissaoService` + vínculo `perfilId` em Usuário + 403 |
| **4** | Front: list/form Perfis, select Usuário, menu dinâmico, guards, layout |
| **5** | Verificação: testes, e2e tenant, checklist BOM e fluxos dono/sem perfil |

Ordem: `0 → 1 → 2 → 3 → 4 → 5`.

---

## 7. Testes

- **Api:** PerfisController; 403 sem permissão em Usuarios/Perfis  
- **Application:** login sem perfil; EnsureAsync; regras dono/em uso  
- **Infrastructure:** CRUD perfil; provisioner cria Dono e vincula default user  
- **Front/E2E:** login dono; menus; CRUD perfil; usuário sem perfil bloqueado  

Seguir padrões e cobertura já usados no projeto (xUnit/Moq; espelho de `Usuarios*Tests`).

---

## 8. Riscos e mitigações

| Risco | Mitigação |
|-------|-----------|
| Confundir `UsuarioPerfil` com entidade Perfil | Nomes distintos na API/UI (“Perfil de acesso”); docs claros |
| Schemas antigos sem tabela | Script de migração idempotente na Fase 1 |
| Token grande | Matriz só no login/sessão; API lê banco |
| Layout quebrado ao ocultar | Não renderizar; revisar list/form nos breakpoints |
| Chicken-egg (ninguém gerencia perfis) | Usuário padrão = Dono com todas as permissões |

---

## 9. Critérios de sucesso

- Empresa nova: usuário padrão loga com todos os menus/ações da v1  
- Usuário sem `perfil_id` não loga e vê a mensagem fixa  
- Menu e botões refletem a matriz; API rejeita operação sem permissão (403)  
- Novo módulo futuro: só catálogo + feature + EnsureAsync, sem redesign  
- Arquivos com acentuação sem corrupção (UTF-8 BOM)  
- Padrão visual/UX das telas Usuários preservado em Perfis  
