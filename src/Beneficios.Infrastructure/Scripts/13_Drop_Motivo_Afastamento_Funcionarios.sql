-- Referência: remove motivo_afastamento de funcionarios
-- Aplicado via TenantSchemaSql.DropColunaMotivoAfastamento
ALTER TABLE funcionarios DROP COLUMN IF EXISTS motivo_afastamento;
