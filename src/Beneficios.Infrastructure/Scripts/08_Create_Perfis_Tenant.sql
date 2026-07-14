-- Referência / replay manual: tabelas de perfil no schema do tenant.
-- O provisionamento automático está em TenantSchemaSql + TenantProvisioner.
-- Substitua {schema} pelo nome do schema (ex.: tenant_acme).

CREATE TABLE IF NOT EXISTS "{schema}".perfis (
    id UUID PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    eh_sistema BOOLEAN NOT NULL DEFAULT FALSE,
    data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_alteracao TIMESTAMP NULL,
    usuario_alteracao_id UUID NULL
);

CREATE TABLE IF NOT EXISTS "{schema}".perfil_permissoes (
    id UUID PRIMARY KEY,
    perfil_id UUID NOT NULL REFERENCES "{schema}".perfis(id) ON DELETE CASCADE,
    codigo_menu VARCHAR(50) NOT NULL,
    visualizar BOOLEAN NOT NULL DEFAULT FALSE,
    criar BOOLEAN NOT NULL DEFAULT FALSE,
    editar BOOLEAN NOT NULL DEFAULT FALSE,
    excluir BOOLEAN NOT NULL DEFAULT FALSE,
    UNIQUE (perfil_id, codigo_menu)
);

ALTER TABLE "{schema}".usuarios
    ADD COLUMN IF NOT EXISTS perfil_id UUID NULL REFERENCES "{schema}".perfis(id);
