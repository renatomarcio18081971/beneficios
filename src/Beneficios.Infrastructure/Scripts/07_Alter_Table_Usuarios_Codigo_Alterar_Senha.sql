-- Adiciona coluna codigo_alterar_senha na tabela de usuários do catálogo e nos schemas de tenant.
-- Execute em ambientes com dados existentes após 06_Migrate_Existing_Tenant_Schemas.sql.

ALTER TABLE beneficios.usuarios
    ADD COLUMN IF NOT EXISTS codigo_alterar_senha VARCHAR(20) NULL;

DO $$
DECLARE
    schema_record RECORD;
BEGIN
    FOR schema_record IN
        SELECT schema_name
        FROM information_schema.schemata
        WHERE schema_name LIKE 'tenant_%'
    LOOP
        EXECUTE format(
            'ALTER TABLE %I.usuarios ADD COLUMN IF NOT EXISTS codigo_alterar_senha VARCHAR(20) NULL',
            schema_record.schema_name);
    END LOOP;
END $$;
