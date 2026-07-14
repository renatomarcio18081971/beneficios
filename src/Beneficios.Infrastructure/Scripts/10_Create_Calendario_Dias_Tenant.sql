-- Referência / replay manual: calendário de dias úteis no schema do tenant.
-- O provisionamento automático está em TenantSchemaSql + TenantProvisioner.
-- Substitua {schema} pelo nome do schema (ex.: tenant_acme).

CREATE TABLE IF NOT EXISTS "{schema}".calendario_dias (
    id UUID PRIMARY KEY,
    data DATE NOT NULL,
    eh_dia_util BOOLEAN NOT NULL,
    tipo_excecao VARCHAR(40) NULL,
    origem VARCHAR(20) NOT NULL,
    observacao TEXT NULL,
    data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_alteracao TIMESTAMP NULL,
    usuario_alteracao_id UUID NULL,
    CONSTRAINT uq_calendario_dias_data UNIQUE (data)
);

CREATE INDEX IF NOT EXISTS idx_calendario_dias_data
    ON "{schema}".calendario_dias(data);
