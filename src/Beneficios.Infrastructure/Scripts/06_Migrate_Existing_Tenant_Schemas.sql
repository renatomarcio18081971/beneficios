-- Provisiona schemas de tenant para empresas já cadastradas e migra usuários operacionais
-- do catálogo (beneficios.usuarios) para o schema do respectivo tenant.
-- O nome do schema segue a razão social: tenant_{razao_social_sanitizada}.
-- Execute após 05_Drop_Empresa_Db_Credentials.sql em ambientes com dados existentes.

CREATE OR REPLACE FUNCTION beneficios.tenant_schema_name(razao_social TEXT)
RETURNS TEXT
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT 'tenant_' || regexp_replace(lower(trim(razao_social)), '[^a-z0-9]', '_', 'g');
$$;

DO $$
DECLARE
    empresa_record RECORD;
    schema_name TEXT;
    index_prefix TEXT;
BEGIN
    FOR empresa_record IN
        SELECT id, razao_social
        FROM beneficios.empresas
        ORDER BY razao_social
    LOOP
        schema_name := beneficios.tenant_schema_name(empresa_record.razao_social);
        index_prefix := replace(schema_name, '-', '_');

        EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', schema_name);

        EXECUTE format($sql$
            CREATE TABLE IF NOT EXISTS %I.usuarios (
                id UUID PRIMARY KEY,
                nome VARCHAR(50) NOT NULL,
                senha VARCHAR(50) NOT NULL,
                email VARCHAR(256) NOT NULL,
                perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa',
                empresa_id UUID NOT NULL,
                token TEXT NULL,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                usuario_alteracao_id UUID NULL,
                CONSTRAINT fk_%s_usuarios_empresa
                    FOREIGN KEY (empresa_id) REFERENCES beneficios.empresas(id) ON DELETE CASCADE
            )
        $sql$, schema_name, index_prefix);

        EXECUTE format(
            'CREATE UNIQUE INDEX IF NOT EXISTS idx_%s_usuarios_email ON %I.usuarios(email)',
            index_prefix,
            schema_name);
        EXECUTE format(
            'CREATE INDEX IF NOT EXISTS idx_%s_usuarios_empresa_id ON %I.usuarios(empresa_id)',
            index_prefix,
            schema_name);
        EXECUTE format(
            'CREATE INDEX IF NOT EXISTS idx_%s_usuarios_nome ON %I.usuarios(nome)',
            index_prefix,
            schema_name);

        EXECUTE format($sql$
            INSERT INTO %I.usuarios (
                id, nome, senha, email, perfil, empresa_id, token,
                data_inclusao, data_alteracao, usuario_alteracao_id
            )
            SELECT
                u.id, u.nome, u.senha, u.email, u.perfil, u.empresa_id, u.token,
                u.data_inclusao, u.data_alteracao, u.usuario_alteracao_id
            FROM beneficios.usuarios u
            WHERE u.empresa_id = %L
              AND u.perfil <> 'Admin'
            ON CONFLICT (id) DO NOTHING
        $sql$, schema_name, empresa_record.id);

        DELETE FROM beneficios.usuarios u
        WHERE u.empresa_id = empresa_record.id
          AND u.perfil <> 'Admin';
    END LOOP;
END $$;

DROP FUNCTION IF EXISTS beneficios.tenant_schema_name(TEXT);
