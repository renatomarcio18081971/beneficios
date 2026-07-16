# Afastamentos / Férias (tenant) — Design Spec

**Data:** 2026-07-14  
**Status:** Aprovado em brainstorming  
**Escopo:** Histórico de afastamentos e férias por funcionário (schema tenant), com módulo próprio (listagem + formulário), atalho na listagem de funcionários, derivação automática de `situacao`, remoção de `motivo_afastamento`, provisionamento e permissões — base para o motor futuro excluir dias do cálculo

**Relacionado:** `2026-07-14-funcionarios-design.md` (ajustes neste spec supersedem a coluna/campo `motivo_afastamento` e a edição manual de `situacao=afastado`)

---

## 1. Contexto

O cadastro de funcionários já cobre identificação, contrato, jornada e benefício (VT). A `situacao` inclui `afastado`, mas na v1 o único detalhe era um texto livre `motivo_afastamento`, sem datas nem histórico — insuficiente para o motor excluir dias do cálculo e para o RH consultar períodos.

Este módulo cria o **histórico estruturado** de afastamentos/férias por colaborador, acessível por menu dedicado (padrão Usuários/Funcionários) e por atalho na listagem de funcionários.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Escopo tenant | Somente schema **tenant**; admin fora da v1 |
| UX | **Híbrido:** menu próprio (listagem + form) **e** atalho na listagem de Funcionários |
| `situacao` | **Derivada** dos períodos: com afastamento ativo → `afastado`; sem → `ativo` (se não `desligado`) |
| Form funcionário | RH só escolhe `Ativo` / `Desligado`; `Afastado` automático |
| `motivo_afastamento` | **Removido** (coluna e campo) |
| Listagem funcionários | Colunas Situação + **Motivo** (tipo do afastamento ativo) |
| Data fim | **Opcional** (período em aberto) |
| Sobreposição | **Bloqueada** no mesmo funcionário |
| Motor | Só tabela + CRUD; **sem** API de exclusão de dias nesta v1 |
| Delete de afastamento | **Sim** (físico), com permissão Excluir e confirmação |
| Delete de funcionário | Continua **sem** DELETE (inalterado) |
| Provisionamento | Tabela em empresa nova + garantia idempotente em existentes |
| Perfil | Módulo `afastamentos` (Visualizar/Criar/Editar/Excluir) |
| Encoding | UTF-8 **com BOM** em arquivos novos/alterados |
| Padrões | Mesmos de Usuários/Perfis/Funcionários/Calendário |

---

## 2. Separação de conceitos

| Conceito | Papel na v1 |
|----------|-------------|
| Funcionário | Cadastro RH; `situacao` refletindo afastamento ativo ou desligamento |
| Afastamento/férias | Linha em `funcionario_afastamentos` com tipo e período |
| Calendário da empresa (`calendario_dias`) | Dias úteis compartilhados — **independente**; férias coletivas no calendário ≠ este histórico individual |
| Motor de benefícios | **Fora da v1** — consome esta tabela no futuro |

---

## 3. Arquitetura de dados

### 3.1 Tabela `funcionario_afastamentos` (schema tenant)

```text
funcionario_afastamentos
  id                     UUID PK
  funcionario_id         UUID NOT NULL FK → funcionarios(id)
  tipo                   VARCHAR(40) NOT NULL
                         -- ferias | licenca_medica | maternidade_paternidade
                         -- acidente | suspensao | outros
  data_inicio            DATE NOT NULL
  data_fim               DATE NULL              -- NULL = em aberto
  observacao             TEXT NULL
  data_inclusao          TIMESTAMP NOT NULL
  data_alteracao         TIMESTAMP NULL
  usuario_alteracao_id   UUID NULL

  INDEX (funcionario_id)
  INDEX (data_inicio, data_fim)
```

### 3.2 Catálogo de tipos (código)

| Código | Label |
|--------|-------|
| `ferias` | Férias |
| `licenca_medica` | Licença médica |
| `maternidade_paternidade` | Maternidade/paternidade |
| `acidente` | Acidente |
| `suspensao` | Suspensão |
| `outros` | Outros |

### 3.3 Regras de período e atividade

| Regra | Detalhe |
|-------|---------|
| Fim ≥ início | Se `data_fim` informada, deve ser ≥ `data_inicio` |
| Sobreposição | Mesmo `funcionario_id`: intervalos não podem sobrepor; `data_fim` NULL trata-se como infinito |
| Ativo “hoje” | `data_inicio ≤ hoje` e (`data_fim` IS NULL ou `data_fim ≥ hoje`) |
| Motivo na listagem | Tipo do (único) afastamento ativo; se nenhum, “—” |

Definição de overlap (dois intervalos [A_ini, A_fim] e [B_ini, B_fim], fim nulo = +∞):

`A_ini ≤ B_fim_efetivo AND B_ini ≤ A_fim_efetivo` → conflito.

### 3.4 Derivação de `situacao` do funcionário

Após create / update / delete de afastamento:

1. Se `situacao` atual é `desligado` → **não altera**.
2. Senão, se existe afastamento ativo hoje → grava `afastado`.
3. Senão → grava `ativo`.

### 3.5 Ajustes em `funcionarios`

- **Dropar** coluna `motivo_afastamento` (script de migração + atualização de repository/DTOs/form).
- Listagem/detalhe passam a expor `motivoAfastamentoAtivo` (código ou label do tipo ativo), calculado na leitura — **não** persistido em `funcionarios`.
- No save de funcionário:
  - Situação aceita apenas `ativo` \| `desligado` (enviar `afastado` → BadRequest).
  - Marcar `ativo` com afastamento ativo existente → BadRequest: `Existe afastamento ativo; encerre ou exclua o período antes de marcar como ativo.`
  - Marcar `desligado` continua exigindo `data_desligamento` (regra atual).

### 3.6 DDL / migração

- Inclusão em `TenantSchemaSql` + script(s) em `Scripts/`
- `TenantProvisioner`: criar `funcionario_afastamentos` em empresa nova
- Garantia idempotente para tenants existentes
- Script de `ALTER TABLE ... DROP COLUMN motivo_afastamento` (ou equivalente) em tenants existentes

---

## 4. Integração com perfil de acesso

Código do módulo: **`afastamentos`**  
Nome de exibição: **Afastamentos/Férias**  
Rota: **`/afastamentos`**  
Ícone sugerido: `event_busy`  
Ações: Visualizar | Criar | Editar | Excluir (`AcaoPermissao.Todas`)

| Ação | Comportamento UI/API |
|------|----------------------|
| Visualizar | Menu + listagem + formulário em leitura se sem Editar |
| Criar | Novo + POST |
| Editar | Salvar + PUT |
| Excluir | Botão Excluir + DELETE do registro de afastamento |

Incluir em `ModulosSistemaCatalog` e `MODULOS_SISTEMA`. Dono recebe full; demais perfis normalizam a linha com `false` se ausente.

Atalho na listagem de Funcionários: exige Visualizar em `afastamentos` (independente de Editar em `funcionarios`).

---

## 5. Backend

### 5.1 Camadas

- Domain: entidade/params/results, enum `TipoAfastamento`, helpers de overlap e “ativo hoje”
- `IFuncionarioAfastamentoRepository` / `FuncionarioAfastamentoRepository` (Dapper, search_path tenant)
- `IFuncionarioAfastamentoService` / `FuncionarioAfastamentoService` (validação + recalc situação)
- DTOs + AutoMapper
- `AfastamentosController` → `api/afastamentos`
- Ajustes em `FuncionarioService` / repository / DTOs (remoção de motivo; regras de situação; campo de leitura do tipo ativo)

### 5.2 Endpoints

| Método | Rota | Permissão | Comportamento |
|--------|------|-----------|---------------|
| GET | `/api/afastamentos` | Visualizar | Lista (filtros: `funcionarioId`, `tipo`, `dataInicio`, `dataFim`) |
| GET | `/api/afastamentos/{id}` | Visualizar | Detalhe |
| POST | `/api/afastamentos` | Criar | Cria; valida overlap; recalc `situacao` |
| PUT | `/api/afastamentos/{id}` | Editar | Atualiza; valida overlap; recalc |
| DELETE | `/api/afastamentos/{id}` | Excluir | Remove; recalc |

Filtro de período na listagem (quando `dataInicio` e/ou `dataFim` do filtro forem informados): retorna registros cujo intervalo **intersecta** o filtro, tratando fim nulo do registro (e do filtro, se omitido) como aberto. Interseção = mesma regra da seção 3.3.

### 5.3 Mensagens (proposta PT)

| Situação | Mensagem |
|----------|----------|
| Sobreposição | `Já existe um afastamento neste período para o funcionário.` |
| Fim &lt; início | `A data fim deve ser maior ou igual à data início.` |
| Funcionário não encontrado | `Funcionário não encontrado.` |
| Afastamento não encontrado | `Afastamento não encontrado.` |
| Situação inválida no save do funcionário | `Situação inválida.` |
| Ativo com afastamento aberto | `Existe afastamento ativo; encerre ou exclua o período antes de marcar como ativo.` |
| Sem permissão | Padrão atual (`Sem permissão para esta operação.`) |

---

## 6. Frontend

### 6.1 Estrutura

- `features/afastamentos/afastamento-list` + `afastamento-form`
- `core/api/afastamento.models.ts` + `afastamento.service.ts`
- Rotas: `/afastamentos`, `/afastamentos/novo`, `/afastamentos/:id/editar`
- Guards: `tenantGuard` + `permissaoGuard('afastamentos')`

### 6.2 Listagem de afastamentos

- Colunas: Funcionário (nome), Tipo, Data início, Data fim (“Em aberto” se nula), Observação (resumo)
- Filtros: funcionário, tipo, período
- Ações: Novo | Editar | Excluir (conforme permissão; Excluir com confirmação)
- Query `?funcionarioId=` pré-aplica o filtro (atalho)

### 6.3 Formulário

- Funcionário (obrigatório; se veio com `funcionarioId` na navegação de “novo”, pré-selecionado)
- Tipo (dropdown do catálogo)
- Data início, Data fim (opcional), Observação (opcional)
- Salvar / Cancelar; Excluir apenas na edição com permissão

### 6.4 Ajustes em Funcionários

**Listagem:** Nome, CPF, Cargo, Situação, **Motivo** (label do tipo ativo ou —), Jornada; ações Editar + ícone/botão **Afastamentos/Férias** → `/afastamentos?funcionarioId={id}` (se Visualizar em `afastamentos`).

**Formulário:** situação só `Ativo` / `Desligado`; sem campo `motivoAfastamento`; se situação atual for `afastado`, exibir como leitura (ou chip) e link para o histórico filtrado.

### 6.5 Permissões na UI

- Sem Criar: oculta Novo  
- Sem Editar: formulário somente leitura / sem Salvar  
- Sem Excluir: oculta Excluir  
- Sem Visualizar: menu e rota bloqueados  

---

## 7. Fluxos principais

```text
[Empresa criada]
    → TenantProvisioner cria funcionario_afastamentos (+ drop legado motivo se aplicável via garantia)

[Startup / garantia]
    → Tenants sem tabela → CREATE IF NOT EXISTS
    → DROP COLUMN motivo_afastamento se ainda existir

[RH via menu]
    → Lista / filtra → Novo ou Editar → salva
    → Overlap ou datas inválidas → erro PT
    → Após salvar/excluir → situacao do funcionário recalc

[RH via Funcionários]
    → Afastamentos/Férias → /afastamentos?funcionarioId=…
    → Histórico daquele colaborador; CRUD conforme permissão

[Motor futuro]
    → Consulta funcionario_afastamentos; dias cobertos saem do cálculo
```

---

## 8. Testes

| Camada | Cobrir |
|--------|--------|
| Domain / Application | Overlap (incl. fim NULL); ativo hoje; recalc ativo↔afastado; desligado intocado; rejeitar situacao=afastado no funcionário; impedir ativo com período aberto |
| Infrastructure | Persistência FK; filtros; provisionamento/garantia |
| API | 403 por ação; CRUD; mensagens de conflito |
| Front | `ng build`; smoke menu → listagem; atalho com `funcionarioId` |

Cobertura backend: meta do projeto (≥ 80% onde aplicável).

---

## 9. Fora da v1

- API/serviço de “dias a excluir do cálculo” / integração ao motor  
- Sincronização com `calendario_dias` (férias coletivas)  
- Anexos / atestados  
- Workflow de aprovação  
- Admin cross-tenant  
- Soft delete de afastamentos  

---

## 10. Critérios de aceite

1. Menu **Afastamentos/Férias** com listagem + formulário e permissões no perfil.  
2. Atalho na listagem de Funcionários abre listagem filtrada por `funcionarioId`.  
3. Tipos do catálogo; data fim opcional; sobreposição bloqueada com mensagem clara.  
4. `situacao` deriva dos períodos ativos; `motivo_afastamento` removido; coluna Motivo na listagem.  
5. Form funcionário só Ativo/Desligado; impedir marcar Ativo com afastamento ativo.  
6. DELETE de registro de afastamento com permissão Excluir.  
7. Tabela provisionada em tenants novos e existentes (idempotente).  
8. UTF-8 com BOM; padrões alinhados a Funcionários/Dias úteis.

---

## 11. Riscos e mitigação

| Risco | Mitigação |
|-------|-----------|
| Situação dessincronizada se migração antiga tiver `afastado` sem períodos | Na garantia de tenants: **recalc em lote** da `situacao` de todos os funcionários (respeitando `desligado`) — simples e idempotente |
| Relógio / fuso “hoje” | Usar data de negócio consistente (mesmo padrão de datas do restante do tenant; preferir `DateOnly` / data local documentada nos testes) |
| Matriz de perfil sem a linha nova | `NormalizarPermissoes` + seed Dono |
| Drop de `motivo_afastamento` com dados | Aceitável: campo era livre e será substituído pelo histórico; sem migração de texto para linhas |
