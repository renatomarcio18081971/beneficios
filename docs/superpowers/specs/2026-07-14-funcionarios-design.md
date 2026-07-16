# Cadastro de Funcionários (tenant) — Design Spec

**Data:** 2026-07-14  
**Status:** Aprovado em brainstorming  
**Escopo:** CRUD de funcionários por empresa (schema tenant), com identificação, dados contratuais, localização, situação, benefício vinculado (somente Vale transporte na v1), jornada via dropdown fixo, provisionamento de tabelas em empresas novas/existentes e módulo no perfil de acesso

---

## 1. Contexto

O sistema Beneficios calcula benefícios (vale transporte, vale refeição, etc.) com base em calendário da empresa, jornada e vínculos do colaborador. O módulo **Calendário** (`dias_uteis`) cobre o calendário compartilhado da empresa. Este módulo cobre o **cadastro RH do funcionário**, incluindo a **jornada individual** (dropdown padronizado) e o vínculo com benefícios — na v1, apenas Vale transporte, em estrutura de lista preparada para o futuro cadastro de regras do motor.

### Decisões consolidadas

| Tópico | Decisão |
|--------|---------|
| Escopo tenant | Somente schema **tenant**; admin fora da v1 |
| Relação com usuário | **Distintos** — `funcionarios` ≠ `usuarios`; vínculo opcional fora da v1 |
| Arquitetura | CRUD clássico: tabela principal + filha de benefícios; formulário único com seções |
| Delete | **Não existe** (nem físico nem soft) — histórico permanente via **situação** |
| Permissão Excluir | Reservada no catálogo; **sem** botão e **sem** endpoint |
| Cargo / centro de custo / local | **Texto livre** na v1 |
| Endereços | **Estruturados** (residência e local de trabalho); CEP com **máscara**, **sem** API externa |
| Benefícios | Tabela filha; v1 só `vale_transporte` (UI de lista com 1 item fixo) |
| Jornada | Dropdown **fixo** obrigatório; se especial → campo detalhe obrigatório |
| Unicidade | CPF obrigatório + único; matrícula **opcional**, única **se** informada |
| Situação desligado | Data de desligamento **obrigatória** |
| Motivo afastamento | Opcional |
| Provisionamento | Tabelas criadas em **toda empresa nova** + garantia idempotente em tenants existentes |
| Perfil | Módulo `funcionarios` (Visualizar/Criar/Editar/Excluir) |
| Encoding | UTF-8 **com BOM** em arquivos novos/alterados |
| Padrões | Mesmos de Usuários/Perfis/Calendário (camadas, DTOs, Repository, controller enxuto, front standalone) |

---

## 2. Separação de conceitos

| Conceito | Papel na v1 |
|----------|-------------|
| Funcionário | Cadastro RH no tenant; base para motor futuro |
| Usuário do sistema | Login/permissões — entidade separada |
| Calendário da empresa | Já existe em `calendario_dias` — não faz parte deste módulo |
| Jornada do funcionário | Enum/dropdown fixo + detalhe se especial |
| Benefício vinculado | Linha em `funcionario_beneficios`; v1 só VT |
| Cadastro de regras do motor | **Fora da v1** — este módulo só prepara o vínculo |

---

## 3. Arquitetura de dados

### 3.1 Tabela `funcionarios` (schema tenant)

```text
funcionarios
  id                          UUID PK
  nome                        VARCHAR NOT NULL
  cpf                         VARCHAR(11) NOT NULL   -- somente dígitos
  matricula                   VARCHAR NULL           -- unique parcial se NOT NULL
  data_admissao               DATE NOT NULL
  data_desligamento           DATE NULL

  cargo                       VARCHAR NOT NULL
  salario_base                NUMERIC(18,2) NOT NULL
  tipo_contrato               VARCHAR(20) NOT NULL   -- clt | estagio | terceirizado | pj
  centro_custo                VARCHAR NULL

  -- Endereço residencial
  res_cep                     VARCHAR(8) NULL        -- somente dígitos; UI máscara 00000-000
  res_logradouro              VARCHAR NULL
  res_numero                  VARCHAR NULL
  res_complemento             VARCHAR NULL
  res_bairro                  VARCHAR NULL
  res_cidade                  VARCHAR NULL
  res_uf                      VARCHAR(2) NULL

  -- Local de trabalho (endereço estruturado + nome livre empresa/filial)
  trab_nome_local             VARCHAR NULL           -- texto livre (empresa/filial)
  trab_cep                    VARCHAR(8) NULL
  trab_logradouro             VARCHAR NULL
  trab_numero                 VARCHAR NULL
  trab_complemento            VARCHAR NULL
  trab_bairro                 VARCHAR NULL
  trab_cidade                 VARCHAR NULL
  trab_uf                     VARCHAR(2) NULL

  situacao                    VARCHAR(20) NOT NULL   -- ativo | afastado | desligado
  motivo_afastamento          TEXT NULL

  jornada                     VARCHAR(40) NOT NULL   -- códigos do catálogo fixo
  jornada_detalhe             VARCHAR NULL           -- obrigatório se especial_categoria

  data_inclusao               TIMESTAMP NOT NULL
  data_alteracao              TIMESTAMP NULL
  usuario_alteracao_id        UUID NULL

  UNIQUE (cpf)
  UNIQUE (matricula) WHERE matricula IS NOT NULL   -- índice único parcial
  INDEX (nome)
  INDEX (situacao)
```

### 3.2 Tabela `funcionario_beneficios`

```text
funcionario_beneficios
  id                    UUID PK
  funcionario_id        UUID NOT NULL FK → funcionarios(id)
  codigo_beneficio      VARCHAR(40) NOT NULL   -- v1: vale_transporte
  ativo                 BOOLEAN NOT NULL
  data_inicio           DATE NULL
  data_fim              DATE NULL
  opt_in                BOOLEAN NOT NULL DEFAULT FALSE
  data_inclusao         TIMESTAMP NOT NULL
  data_alteracao        TIMESTAMP NULL

  UNIQUE (funcionario_id, codigo_beneficio)
```

Na v1 a API/UI trabalha com **no máximo uma** linha `vale_transporte` por funcionário (upsert no save).

### 3.3 Catálogo fixo de jornada (código)

| Código | Label |
|--------|-------|
| `44h_clt` | 44h semanais (CLT padrão) |
| `40h_seg_sex` | 40h semanais (segunda a sexta) |
| `36h` | 36h semanais (turnos reduzidos) |
| `12x36` | 12x36 (12h trabalho / 36h descanso) |
| `6x1` | 6x1 (trabalha 6 dias, folga 1) |
| `5x1` | 5x1 (trabalha 5 dias, folga 1) |
| `5x2` | 5x2 (trabalha 5 dias, folga 2) |
| `tempo_parcial` | Tempo parcial (até 30h semanais) |
| `especial_categoria` | Jornada especial por categoria |

Se `jornada = especial_categoria` → `jornada_detalhe` obrigatório; caso contrário `jornada_detalhe` deve ser nulo/vazio.

### 3.4 Outros enums (código)

**Tipo de contrato:** `clt` | `estagio` | `terceirizado` | `pj`  
**Situação:** `ativo` | `afastado` | `desligado`  
**Código benefício v1:** `vale_transporte`

### 3.5 DDL / migração

- Inclusão em `TenantSchemaSql` + script(s) de referência em `Scripts/`
- `TenantProvisioner.ProvisionarAsync`: criar `funcionarios` e `funcionario_beneficios` ao provisionar empresa nova
- Garantia idempotente para tenants existentes (startup / método no espírito de `GarantirPerfisEmTenantsExistentesAsync` / calendário)

---

## 4. Integração com perfil de acesso

Código do módulo: **`funcionarios`**  
Nome de exibição: **Funcionários**  
Rota: **`/funcionarios`**  
Ícone sugerido: `badge`  
Ações: Visualizar | Criar | Editar | Excluir (`AcaoPermissao.Todas`)

| Ação | Comportamento UI/API |
|------|----------------------|
| Visualizar | Menu + listagem + formulário em leitura se sem Editar |
| Criar | Novo funcionário + POST |
| Editar | Salvar alterações + PUT |
| Excluir | **Sem operação na v1** (reservado no catálogo) |

Incluir em `ModulosSistemaCatalog` e `MODULOS_SISTEMA`. Dono recebe full; demais perfis normalizam a linha com `false` se ausente.

---

## 5. Backend

### 5.1 Camadas

- Domain: entidade/params/results, enums (`TipoContrato`, `SituacaoFuncionario`, `JornadaTrabalho`), catálogo de labels de jornada
- `IFuncionarioRepository` / `FuncionarioRepository` (Dapper, search_path tenant)
- `IFuncionarioService` / `FuncionarioService`
- DTOs + AutoMapper `FuncionarioProfile`
- `FuncionariosController` → `api/funcionarios`

### 5.2 Endpoints

| Método | Rota | Permissão | Comportamento |
|--------|------|-----------|---------------|
| GET | `/api/funcionarios` | Visualizar | Lista (filtros: nome, cpf, matricula, situacao) |
| GET | `/api/funcionarios/{id}` | Visualizar | Detalhe + benefícios |
| POST | `/api/funcionarios` | Criar | Cria funcionário + sync benefícios (VT) |
| PUT | `/api/funcionarios/{id}` | Editar | Atualiza + sync benefícios |

Sem DELETE.

### 5.3 Regras de validação

| Regra | Mensagem (proposta PT) |
|-------|-------------------------|
| CPF inválido (dígitos) | `CPF inválido.` |
| CPF duplicado | `CPF já cadastrado.` |
| Matrícula duplicada | `Matrícula já cadastrada.` |
| Não encontrado | `Funcionário não encontrado.` |
| Desligado sem data desligamento | `Informe a data de desligamento.` |
| Jornada especial sem detalhe | `Informe o detalhe da jornada especial.` |
| Sem permissão | Padrão atual (`Sem permissão para esta operação.`) |

Demais campos obrigatórios de identificação/contrato/jornada: validação de formulário + BadRequest amigável.

**Benefícios (sync no save):** payload inclui lista (v1: 0 ou 1 item `vale_transporte`). Se ativo, datas/opt-in conforme UI; upsert por `(funcionario_id, codigo_beneficio)`.

### 5.4 Provisionamento

Após criar demais tabelas do tenant, executar DDL de `funcionarios` e `funcionario_beneficios`. Em tenants legados, criar tabelas se não existirem (idempotente).

---

## 6. Frontend

### 6.1 Estrutura

- `features/funcionarios/funcionario-list` + `funcionario-form`
- `core/api/funcionario.models.ts` + `funcionario.service.ts`
- Rotas: `/funcionarios`, `/funcionarios/novo`, `/funcionarios/:id/editar`
- Guards: `tenantGuard` + `permissaoGuard('funcionarios')`

### 6.2 UX

**Listagem:** filtros (nome, CPF, matrícula, situação); Novo; Editar; sem Excluir.

**Formulário (seções na mesma página):**
1. Identificação  
2. Dados contratuais  
3. Localização (residência + local de trabalho; CEP mascarado)  
4. Situação  
5. Benefícios — card **Vale transporte** (ativo, início, fim, opt-in)  
6. Jornada — select; detalhe condicional se especial  

Máscaras: CPF, CEP (`00000-000`), salário (BRL). Confirmação de salvar no padrão Usuários/Perfis se aplicável.

### 6.3 Permissões na UI

- Sem Criar: oculta Novo  
- Sem Editar: formulário somente leitura / sem Salvar  
- Sem Visualizar: menu e rota bloqueados  

---

## 7. Fluxos principais

```text
[Empresa criada]
    → TenantProvisioner cria funcionarios + funcionario_beneficios

[Startup / garantia]
    → Tenants existentes sem tabelas → CREATE IF NOT EXISTS

[RH tenant]
    → Lista / filtra funcionários
    → Novo ou Editar → preenche seções → salva
    → Situação desligado exige data_desligamento
    → Jornada especial exige detalhe
    → VT opcional via lista filha (1 código fixo)
```

---

## 8. Testes

- Repository / Service / Controller: unicidade, jornada especial, desligamento, sync VT, 403 permissões  
- Provisioner (ou garantia): tabelas existem após provisionar  
- Front: `ng build`; E2E smoke menu → listagem  
- Cobertura backend ≥ 80% (`scripts/check-coverage.ps1`)

---

## 9. Fora da v1

- Vínculo funcionário ↔ usuário de login  
- ViaCEP / qualquer API externa de endereço  
- Cadastros auxiliares (cargos, departamentos, filiais)  
- Outros benefícios além de Vale transporte  
- Cadastro de regras do motor / cálculo de benefícios  
- DELETE de funcionário  
- Admin cross-tenant  
- Remapeamento jornada → dias úteis no motor (pode ser v1.1+)

---

## 10. Encoding e padrões

- UTF-8 **com BOM** em todo arquivo novo ou alterado  
- Métodos/tipos novos em português claro (`SalvarAsync`, `ObterPorIdAsync`, etc.)  
- Dashboard permanece fora do catálogo de permissões  
