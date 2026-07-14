# Cadastro de Dias Úteis (tenant) — Design Spec

**Data:** 2026-07-14  
**Status:** Aprovado em brainstorming  
**Escopo:** Calendário anual dia a dia por empresa (schema tenant), com geração automática (seg–sex/sáb–dom + feriados nacionais), tela mensal simples, botão Gerar ano, job de virada de ano e integração ao perfil de acesso

---

## 1. Contexto

O sistema Beneficios calcula benefícios que dependem de jornada ou presença (vale transporte, vale refeição, etc.). É necessário um **calendário da empresa** que diga, para cada data, se o dia conta como útil ou não, com exceções (feriados, ponto facultativo, férias coletivas, etc.).

Jornadas distintas por funcionário (seg–sex, seg–sáb, plantão, dia sim/dia não) existirão no futuro; **nesta v1** cadastra-se apenas o calendário **compartilhado da empresa**.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Persistência | **Um registro por dia** do ano (calendário aberto) |
| Escopo | Somente schema **tenant**; admin fora da v1 |
| Conteúdo v1 | Só calendário da **empresa**; jornada por funcionário **fora** |
| Padrão ao gerar ano | Seg–sex = útil; sáb/dom = não útil |
| Import automático | Somente **feriados nacionais** (catálogo no **código**) |
| Estadual / municipal / facultativo / férias | Cadastro **manual** via edição do dia |
| Geração do ano corrente | No **provisionamento** da empresa |
| Outros anos | Botão explícito **Gerar ano** |
| Virada de ano | Serviço no back: se o ano corrente **não existir** no tenant, cria o calendário completo |
| Fonte dos nacionais | Catálogo Domain (fixos + móveis); sem API externa na v1 |
| UI | Calendário **mensal simples** (ano + mês + clique no dia); sem filtros avançados |
| Feriado nacional editável | **Sim** (útil, tipo, observação) |
| Perfil | Novo módulo `dias_uteis` no catálogo (Visualizar/Criar/Editar/Excluir) |
| Encoding | UTF-8 **com BOM** em arquivos novos/alterados |
| Padrões | Mesmos de Usuários/Perfis (camadas, DTOs, Repository, controller enxuto, front standalone) |

---

## 2. Separação de conceitos

| Conceito | Papel na v1 |
|----------|-------------|
| Calendário da empresa | Tabela `calendario_dias`: cada data com `eh_dia_util`, tipo, observação |
| Feriado nacional (catálogo) | Lista em código usada na **geração**; após gravar, o dia é editável pela empresa |
| Jornada do funcionário | **Fora da v1** — não faz parte deste módulo |
| Cálculo de benefício | **Fora da v1** — este módulo só prepara a base de dias |

---

## 3. Arquitetura de dados

### 3.1 Tabela no schema do tenant

```text
calendario_dias
  id                     UUID PK
  data                   DATE NOT NULL
  eh_dia_util            BOOLEAN NOT NULL
  tipo_excecao           VARCHAR(40) NULL
                         -- nacional | estadual | municipal | ferias_coletivas | ponto_facultativo
                         -- NULL = dia “normal” (apenas regra de semana)
  origem                 VARCHAR(20) NOT NULL
                         -- geracao | nacional | manual
  observacao             TEXT NULL
  data_inclusao          TIMESTAMP NOT NULL
  data_alteracao         TIMESTAMP NULL
  usuario_alteracao_id   UUID NULL

  UNIQUE (data)
  INDEX (data)
```

**Semântica de `origem`**

| Valor | Significado |
|-------|-------------|
| `geracao` | Criado pela regra seg–sex / sáb–dom, sem feriado nacional aplicado |
| `nacional` | Marcado na geração a partir do catálogo de feriados nacionais |
| `manual` | Alterado pelo usuário (qualquer campo); job/gerar não sobrescreve |

### 3.2 Modelo de geração do ano (idempotente)

Algoritmo `GerarAno(ano)`:

1. Se já existe **qualquer** dia com `data` no ano → **não faz nada** e sinaliza “ano já cadastrado”.
2. Caso contrário, para cada data de 01/01 a 31/12:
   - Seg–sex → `eh_dia_util = true`, `tipo_excecao = null`, `origem = geracao`
   - Sáb–dom → `eh_dia_util = false`, `tipo_excecao = null`, `origem = geracao`
3. Para cada feriado nacional do catálogo naquele ano:
   - Localiza o dia gerado
   - Define `eh_dia_util = false`, `tipo_excecao = nacional`, `origem = nacional`, `observacao = nome do feriado` (ou texto padrão)

**Não** há exclusão física de dias na v1. Não há “regerar sobrescrevendo” na v1.

### 3.3 Catálogo de feriados nacionais (código)

- Classe/listas no Domain (ex.: `FeriadosNacionaisCatalog`)
- Inclui feriados **fixos** (ex.: Confraternização, Tiradentes, Independência, Natal) e **móveis** (Carnaval, Sexta-feira Santa, Corpus Christi) calculados a partir da Páscoa (algoritmo documentado nos testes)
- Sem dependência de API externa
- Espelhamento no front **não** é necessário para a lista completa (só enums de tipo de exceção e labels de UI)

### 3.4 DDL / migração

- Inclusão em `TenantSchemaSql` + script de referência em `Scripts/` (padrão perfis)
- Garantir criação em tenants novos no provisionamento
- Migração idempotente para tenants existentes (criar tabela se não existir); calendário do ano corrente gerado pelo job ou sob demanda no primeiro uso/provision legado — regra: job + botão cobrem tenants já existentes

---

## 4. Integração com perfil de acesso

Código do módulo: **`dias_uteis`**  
Nome de exibição: **Dias úteis**  
Rota: **`/dias-uteis`**  
Ações: Visualizar | Criar | Editar | Excluir (catálogo com `AcaoPermissao.Todas`)

| Ação | Comportamento UI/API |
|------|----------------------|
| Visualizar | Menu + tela; consulta calendário e diálogo em leitura |
| Criar | Botão **Gerar ano** + endpoint de geração |
| Editar | Salvar alterações do dia |
| Excluir | **Sem operação na v1** (reservado no catálogo/matriz para não quebrar o contrato de 4 flags; botão de excluir dia **não** existe) |

- Incluir em `ModulosSistemaCatalog` (back) e `MODULOS_SISTEMA` (front)
- Perfil **Dono** passa a receber permissões full do novo código (provisionamento / normalização de matriz)
- Perfís já existentes: ao editar/salvar, o `NormalizarPermissoes` completa a linha `dias_uteis` com `false` se ausente (comportamento atual do catálogo)
- Enforcement: `permissaoGuard('dias_uteis')` + `GarantirPermissaoAsync` nos endpoints

**Dashboard** permanece fora do catálogo de permissões (acesso livre a tenant autenticado).

---

## 5. Backend

### 5.1 Camadas (padrão existente)

- Domain: entidade/params/`CalendarioDiaQueryResult`, enums `TipoExcecaoCalendario`, `OrigemCalendarioDia`
- `ICalendarioDiaRepository` / `CalendarioDiaRepository` (Dapper, search_path do tenant)
- `ICalendarioDiaService` / `CalendarioDiaService`
- `IFeriadosNacionaisProvider` (ou método estático do catálogo) usado só na geração
- `CalendariosController` → `api/calendarios` ou `api/dias-uteis` (preferência: **`api/dias-uteis`**)

### 5.2 Endpoints

| Método | Rota | Permissão | Comportamento |
|--------|------|-----------|---------------|
| GET | `/api/dias-uteis?ano=&mes=` | Visualizar | Dias do mês (para a grade) |
| GET | `/api/dias-uteis/{id}` ou por data | Visualizar | Detalhe do dia |
| PUT | `/api/dias-uteis/{id}` | Editar | Atualiza `eh_dia_util`, `tipo_excecao`, `observacao`; define `origem = manual` |
| POST | `/api/dias-uteis/gerar-ano` body `{ ano }` | Criar | Executa `GerarAno`; 409/400 amigável se já existir |

Sem DELETE de dia na v1.

### 5.3 Provisionamento

Em `TenantProvisioner`, após criar tabelas (incluindo `calendario_dias`):

- Chamar `GerarAno(DateTime.UtcNow.Year)` (ou clock injetável) no schema novo

### 5.4 Job de virada de ano

- `BackgroundService` (ou equivalente hosted) registrado na API
- Periodicidade: pelo menos **1× ao dia** (e execução segura no **startup**, no mesmo espírito de `GarantirPerfisEmTenantsExistentesAsync`)
- Para cada schema `tenant_*`: se o **ano corrente** não possui dias → `GerarAno(anoCorrente)`
- Falha em um tenant: log + continua nos demais; **não** derruba o host

### 5.5 Mensagens

| Situação | Mensagem (PT) |
|----------|----------------|
| Ano já cadastrado | `Este ano já está cadastrado.` |
| Dia não encontrado | `Dia não encontrado.` |
| Sem permissão | Padrão atual (`Sem permissão para esta operação.`) |

---

## 6. Frontend

### 6.1 Estrutura

- `features/dias-uteis/` (tela principal; diálogo de edição pode ser componente local ou shared dialog simples)
- `core/api/dias-uteis.models.ts` + `dias-uteis.service.ts`
- Rota filha do shell: `/dias-uteis` com `tenantGuard` + `permissaoGuard('dias_uteis')`
- Menu: sempre filtrado por Visualizar; ícone sugerido `calendar_month`

### 6.2 UX (usuário final)

1. Título **Dias úteis**
2. Seletor **Ano** + **Mês** (setas anterior/próximo)
3. Botão **Gerar ano** (só com Criar)
4. Legenda: útil / não útil / com exceção (feriado etc.)
5. Grade mensal; clique abre diálogo
6. Diálogo: data (readonly), útil sim/não, tipo de exceção, observação, Salvar/Cancelar
7. Se o mês/ano não tiver dados (ano não gerado): estado vazio amigável + orientação a usar **Gerar ano**

**Sem** barra de filtros avançados, **sem** export PDF/Excel na v1.

### 6.3 Permissões na UI

- Sem Editar: diálogo somente leitura (ou clique não abre edição)
- Sem Criar: oculta **Gerar ano**
- Sem Visualizar: item de menu oculto; rota bloqueada pelo guard

---

## 7. Fluxos principais

```text
[Empresa criada]
    → TenantProvisioner cria calendario_dias
    → GerarAno(ano corrente)

[Job diário / startup]
    → Para cada tenant: se ano corrente ausente → GerarAno

[Usuário tenant]
    → Abre Dias úteis (mês/ano atuais)
    → Navega meses
    → Clica dia → edita → PUT (origem=manual)
    → Ano futuro/passado sem dados → Gerar ano → POST
```

---

## 8. Testes

| Camada | Cobrir |
|--------|--------|
| Domain | Feriados nacionais de um ano conhecido (fixos + móveis); dia útil da semana vs fim de semana |
| Application | `GerarAno` cria 365/366; idempotente; PUT marca `origem=manual` |
| Infrastructure | Persistência e unique por data; job/helper “só se ausente” |
| API | 403 sem permissão; 409/400 ano já cadastrado |
| Front/E2E smoke | Abrir tela, gerar ano (mock), editar dia (mock) |

Cobertura backend: manter meta do projeto (≥ 80% onde aplicável ao pacote de testes).

---

## 9. Fora da v1

- Jornada / escala por funcionário  
- Import automático estadual/municipal  
- Campos UF/município na empresa  
- Export PDF/Excel do calendário  
- API de “contar dias úteis no período” para cálculo de benefícios (pode ser v1.1)  
- Exclusão de dias ou regeneração destrutiva  
- Calendário no subdomain admin  

---

## 10. Critérios de aceite

1. Empresa nova nasce com calendário do ano corrente (seg–sex/sáb–dom + nacionais).  
2. Botão **Gerar ano** cria ano inexistente e recusa ano já existente com mensagem clara.  
3. Job/startup cria o ano corrente ausente em tenants existentes.  
4. Tela mensal simples; edição por clique; sem filtros complexos.  
5. Módulo `dias_uteis` no perfil; Dono com acesso total; enforcement front + API.  
6. Edição de dia (incl. ex-nacional) persiste e marca origem manual.  
7. Encoding UTF-8 com BOM nos arquivos tocados; padrões de código alinhados a Usuários/Perfis.

---

## 11. Riscos e mitigação

| Risco | Mitigação |
|-------|-----------|
| Job lento com muitos tenants | Geração só se ano ausente; log por tenant; timeout generoso |
| Feriados móveis incorretos | Testes unitários com datas oficiais de referência |
| Usuário tenta “regerar” após edições | Idempotência: nunca sobrescreve ano existente |
| Matriz de perfil sem a linha nova | NormalizarPermissoes + seed Dono no provisionamento |
