# Linhas de Ônibus e Funcionário × Linhas (tenant) — Design Spec

**Data:** 2026-07-16  
**Status:** Aprovado em brainstorming  
**Escopo:** CRUD de linhas de ônibus (catálogo com vigência e tarifa) e vínculos N:N funcionário×linha (quantidade de passagens/dia), com módulos de perfil separados, atalho em Funcionários, integração com Vale Transporte ativo, provisionamento tenant e cobertura de testes ≥ 80% (Sonar) — base para o motor futuro de VT

**Relacionado:** `2026-07-14-funcionarios-design.md` (benefício `vale_transporte` em `funcionario_beneficios`)

---

## 1. Contexto

O cadastro de funcionários já vincula o benefício Vale Transporte (`funcionario_beneficios` / `vale_transporte`), mas não há catálogo de linhas nem registro de quais linhas o colaborador utiliza nem quantas passagens por dia. Sem isso, o motor futuro de VT não tem base para calcular valor a partir de tarifas.

Esta feature cria:

1. **Linhas de ônibus** — catálogo tenant com descrição, vigência e valor da tarifa  
2. **Funcionário × Linhas** — vínculo N:N com quantidade de utilizações (passagens/dia naquela linha) e vigência própria  

UX híbrida no espírito de Afastamentos: menu próprio para o catálogo e para os vínculos, mais atalho na listagem/formulário de Funcionários.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Escopo tenant | Somente schema **tenant**; admin fora da v1 |
| Cardinalidade | N:N — funcionário pode ter várias linhas; **mesmo par funcionário+linha uma única vez** |
| Relação com VT | Condicionado — vincular só se `vale_transporte` **ativo** |
| Desativar VT | **Encerra automaticamente** vínculos abertos (`data_fim = hoje`); motor futuro não contempla funcionário sem VT ativo |
| UX | **Híbrido:** menus **Linhas de Ônibus** e **Funcionário × Linhas** + atalho em Funcionários |
| Permissões | **Dois módulos:** `linhas_onibus` e `funcionario_linhas` (Visualizar/Criar/Editar/Excluir no catálogo) |
| Excluir (ação perfil) | Reservada no catálogo; **sem** botão e **sem** endpoint DELETE — encerramento por `data_fim` |
| Encerrar vínculo | `data_inicio` / `data_fim` no vínculo (`data_fim` NULL = vigente) |
| Quantidade | Passagens/dia **naquela linha**; inteiro ≥ 1 (ex.: 1 = só ida/volta em linhas distintas; 2 = ida e volta na mesma) |
| Vigência da linha | `data_fim` **opcional**; ao vincular, a linha deve estar **vigente** na `data_inicio` do vínculo |
| Encerrar linha | **Bloqueado** se existir vínculo com `data_fim` NULL nessa linha |
| Auditoria | Padrão atual: `data_inclusao`, `data_alteracao`, `usuario_alteracao_id` (create e update) |
| Layout UI | Reutilizar layout/padrão das telas já existentes (listagens e forms Material do sistema) |
| Encoding | UTF-8 **com BOM** em arquivos novos/alterados |
| Docs / práticas | Skill `angular-developer` + Context7 (front Angular 22 e back) na implementação |
| Testes / Sonar | Cobertura unitária **≥ 80%** no escopo da mudança |
| Motor | **Fora da v1** — só cadastro/CRUD |
| Provisionamento | Tabelas em empresa nova + garantia idempotente em tenants existentes |
| Padrões | Mesmos de Usuários/Perfis/Funcionários/Afastamentos/Calendário |

---

## 2. Separação de conceitos

| Conceito | Papel na v1 |
|----------|-------------|
| Linha de ônibus | Catálogo de tarifas/vigência no tenant |
| Vínculo funcionário×linha | Quais linhas o colaborador usa e quantas passagens/dia em cada uma |
| Vale Transporte (`funcionario_beneficios`) | Gate: sem VT ativo não há vínculo novo; desativar VT fecha vínculos abertos |
| Motor de benefícios | **Fora da v1** — consumirá linhas + vínculos + calendário no futuro |

---

## 3. Arquitetura de dados

### 3.1 Tabela `linhas_onibus` (schema tenant)

```text
linhas_onibus
  id                     UUID PK
  descricao              VARCHAR NOT NULL
  data_inicio            DATE NOT NULL
  data_fim               DATE NULL              -- NULL = vigente
  valor_tarifa           NUMERIC(18,2) NOT NULL
  data_inclusao          TIMESTAMP NOT NULL
  data_alteracao         TIMESTAMP NULL
  usuario_alteracao_id   UUID NULL

  INDEX (descricao)
  INDEX (data_inicio, data_fim)
```

### 3.2 Tabela `funcionario_linhas` (schema tenant)

```text
funcionario_linhas
  id                     UUID PK
  funcionario_id         UUID NOT NULL FK → funcionarios(id)
  linha_onibus_id        UUID NOT NULL FK → linhas_onibus(id)
  quantidade             INT NOT NULL           -- ≥ 1
  data_inicio            DATE NOT NULL
  data_fim               DATE NULL              -- NULL = vigente
  data_inclusao          TIMESTAMP NOT NULL
  data_alteracao         TIMESTAMP NULL
  usuario_alteracao_id   UUID NULL

  UNIQUE (funcionario_id, linha_onibus_id)
  INDEX (funcionario_id)
  INDEX (linha_onibus_id)
```

### 3.3 Regras de vigência e negócio

| Regra | Detalhe |
|-------|---------|
| Fim ≥ início | Se `data_fim` informada, deve ser ≥ `data_inicio` (linha e vínculo) |
| Linha vigente | `data_inicio ≤ ref` e (`data_fim` IS NULL ou `data_fim ≥ ref`); `ref` = `data_inicio` do vínculo |
| VT ativo | Existe `funcionario_beneficios` com `codigo_beneficio = vale_transporte` e `ativo = true` |
| Unicidade | No máximo um registro por (`funcionario_id`, `linha_onibus_id`) — alterações de quantidade/vigência editam o mesmo registro |
| Quantidade | Inteiro ≥ 1; sem teto na v1 |
| Sem DELETE físico | Encerrar = preencher `data_fim` |
| Desativar VT | Ao persistir VT inativo no save do funcionário: `UPDATE funcionario_linhas SET data_fim = hoje` onde `data_fim IS NULL` e `funcionario_id = …` |
| Encerrar linha | PUT que define `data_fim` na linha falha se existir vínculo com `linha_onibus_id` e `data_fim IS NULL` |

### 3.4 DDL / migração

- Inclusão em `TenantSchemaSql` + scripts de referência em `Scripts/` (ex.: `14_Create_Linhas_Onibus_Tenant.sql`, `15_Create_Funcionario_Linhas_Tenant.sql`)
- `TenantProvisioner`: criar ambas as tabelas em empresa nova
- Garantia idempotente para tenants existentes (startup / método no espírito de calendário/afastamentos)

---

## 4. Integração com perfil de acesso

### 4.1 Módulo `linhas_onibus`

| Campo | Valor |
|-------|-------|
| Código | `linhas_onibus` |
| Nome | Linhas de Ônibus |
| Rota | `/linhas-onibus` |
| Ícone sugerido | `directions_bus` |
| Ações | Visualizar \| Criar \| Editar \| Excluir (`AcaoPermissao.Todas`) |

| Ação | Comportamento UI/API |
|------|----------------------|
| Visualizar | Menu + listagem + formulário em leitura se sem Editar |
| Criar | Nova linha + POST |
| Editar | Salvar / encerrar vigência + PUT |
| Excluir | **Sem operação na v1** (reservado no catálogo) |

### 4.2 Módulo `funcionario_linhas`

| Campo | Valor |
|-------|-------|
| Código | `funcionario_linhas` |
| Nome | Funcionário × Linhas |
| Rota | `/funcionario-linhas` |
| Ícone sugerido | `commute` |
| Ações | Visualizar \| Criar \| Editar \| Excluir (`AcaoPermissao.Todas`) |

| Ação | Comportamento UI/API |
|------|----------------------|
| Visualizar | Menu + listagem + formulário em leitura se sem Editar; **atalho em Funcionários** |
| Criar | Novo vínculo + POST |
| Editar | Salvar / encerrar vigência + PUT |
| Excluir | **Sem operação na v1** (reservado no catálogo) |

Incluir em `ModulosSistemaCatalog` e `MODULOS_SISTEMA`. Dono recebe full; demais perfis normalizam a linha com `false` se ausente.

Atalho na listagem/form de Funcionários: exige **Visualizar** em `funcionario_linhas` (independente de Editar em `funcionarios`), no mesmo espírito do atalho de afastamentos.

---

## 5. Backend

### 5.1 Camadas

- Domain: params/results, helpers de vigência (linha vigente em data; fim ≥ início)
- `ILinhaOnibusRepository` / `LinhaOnibusRepository` (Dapper, search_path tenant)
- `IFuncionarioLinhaRepository` / `FuncionarioLinhaRepository`
- `ILinhaOnibusService` / `LinhaOnibusService`
- `IFuncionarioLinhaService` / `FuncionarioLinhaService` (gate VT; unicidade; validação de linha vigente)
- Ajuste em `FuncionarioService` (ou ponto de save de benefícios): ao desativar VT, encerrar vínculos abertos
- DTOs + AutoMapper profiles
- Controllers: `LinhasOnibusController` → `api/linhas-onibus`; `FuncionarioLinhasController` → `api/funcionario-linhas`

### 5.2 Endpoints

| Método | Rota | Permissão | Comportamento |
|--------|------|-----------|---------------|
| GET | `/api/linhas-onibus` | `linhas_onibus` Visualizar | Lista (filtros: descrição, vigência) |
| GET | `/api/linhas-onibus/{id}` | `linhas_onibus` Visualizar | Detalhe |
| POST | `/api/linhas-onibus` | `linhas_onibus` Criar | Cria |
| PUT | `/api/linhas-onibus/{id}` | `linhas_onibus` Editar | Atualiza; bloqueia encerrar se vínculo aberto |
| GET | `/api/funcionario-linhas` | `funcionario_linhas` Visualizar | Lista (filtros: `funcionarioId`, vigência) |
| GET | `/api/funcionario-linhas/{id}` | `funcionario_linhas` Visualizar | Detalhe |
| POST | `/api/funcionario-linhas` | `funcionario_linhas` Criar | Cria; valida VT, unicidade, linha vigente, quantidade |
| PUT | `/api/funcionario-linhas/{id}` | `funcionario_linhas` Editar | Atualiza; mesmas validações aplicáveis |

**Sem DELETE** em ambos os recursos.

### 5.3 Mensagens (PT)

| Situação | Mensagem |
|----------|----------|
| VT inativo | `Funcionário sem vale transporte ativo; não é possível vincular linhas.` |
| Linha não vigente | `A linha selecionada não está vigente na data de início do vínculo.` |
| Par duplicado | `Esta linha já está vinculada a este funcionário.` |
| Quantidade &lt; 1 | `A quantidade de utilizações deve ser maior ou igual a 1.` |
| Fim &lt; início | `A data fim deve ser maior ou igual à data início.` |
| Encerrar linha com vínculo aberto | `Existem vínculos em aberto para esta linha; encerre-os antes de finalizar a vigência.` |
| Linha não encontrada | `Linha não encontrada.` |
| Vínculo não encontrado | `Vínculo não encontrado.` |
| Funcionário não encontrado | `Funcionário não encontrado.` |
| Sem permissão | Padrão atual (`Sem permissão para esta operação.`) |

---

## 6. Frontend

### 6.1 Estrutura e layout

- Reutilizar o **mesmo layout/padrão** das telas já criadas (listagem com filtros, tabela Material, form com seções, botões Novo/Editar/Cancelar)
- `features/linhas-onibus/` — `linha-onibus-list` + `linha-onibus-form`
- `features/funcionario-linhas/` — `funcionario-linha-list` + `funcionario-linha-form`
- `core/api/linha-onibus.models.ts` + `linha-onibus.service.ts`
- `core/api/funcionario-linha.models.ts` + `funcionario-linha.service.ts`
- Rotas: `/linhas-onibus`, `/linhas-onibus/nova`, `/linhas-onibus/:id/editar`
- Rotas: `/funcionario-linhas`, `/funcionario-linhas/novo`, `/funcionario-linhas/:id/editar`
- Guards: `tenantGuard` + `permissaoGuard('linhas_onibus' | 'funcionario_linhas')`
- Menu no shell conforme catálogo de módulos
- Forms: alinhar à estratégia **já usada** no app (reactive forms nos cadastros atuais); skill `angular-developer` + Context7 na implementação
- Encoding UTF-8 **com BOM**

### 6.2 Listagem de linhas

- Colunas: Descrição, Data início, Data fim (“Em aberto” se nula), Valor da tarifa
- Filtros: descrição, vigência (ex.: só vigentes)
- Ações: Novo | Editar (conforme permissão); **sem** Excluir

### 6.3 Formulário de linha

- Descrição (obrigatória)
- Data início (obrigatória)
- Data fim (opcional)
- Valor da tarifa (obrigatório, decimal ≥ 0)
- Encerrar vigência = preencher data fim (sujeito à regra de vínculos abertos)

### 6.4 Listagem de vínculos

- Colunas: Funcionário, Linha, Quantidade, Data início, Data fim (“Em aberto” se nula)
- Filtros: funcionário, vigência
- Query `?funcionarioId=` pré-aplica o filtro (atalho)
- Ações: Novo | Editar; **sem** Excluir

### 6.5 Formulário de vínculo

- Funcionário (obrigatório; pré-selecionado se veio do atalho)
- Linha (obrigatória; dropdown só linhas vigentes na data de início)
- Quantidade (inteiro ≥ 1)
- Data início / Data fim (fim opcional)
- Bloquear submit se funcionário sem VT ativo (mensagem alinhada à API)

### 6.6 Atalho em Funcionários

- Listagem: botão/link “Linhas” (ou equivalente) com `routerLink` + `queryParams: { funcionarioId }`, visível se `possuiPermissao('funcionario_linhas', 'visualizar')`
- Form (edição): link “Ver linhas de ônibus” com o mesmo filtro, no espírito do link de afastamentos

---

## 7. Testes e qualidade

| Camada | Foco |
|--------|------|
| Domain | Helpers de vigência; fim ≥ início |
| Application | Gate VT; auto-encerra ao desativar VT; bloqueio ao encerrar linha; unicidade; quantidade; mensagens |
| API | Permissões, status codes, contratos dos cenários acima |
| Front | Permissão do atalho; smoke dos forms conforme padrão do repo |

**Aceite Sonar:** cobertura de testes unitários **igual ou maior que 80%** no escopo da mudança (não regredir a métrica do projeto).

---

## 8. Fora da v1

- Motor de cálculo de VT / benefícios
- DELETE físico de linha ou vínculo
- `usuario_inclusao_id` separado (usa-se só `usuario_alteracao_id`)
- Admin global (fora do tenant)
- Teto máximo de quantidade (além de ≥ 1)

---

## 9. Ordem sugerida de implementação

1. Domain (helpers + models + interfaces) + testes  
2. Módulos no catálogo de permissões (back + front)  
3. DDL / `TenantSchemaSql` / provisionamento  
4. Repositories + services + API + testes (≥ 80%)  
5. Ajuste save funcionário (desativar VT → encerrar vínculos)  
6. Front: linhas + vínculos + atalho em funcionários (layout existente)  
7. E2E smoke se o repo já cobrir fluxos similares  
