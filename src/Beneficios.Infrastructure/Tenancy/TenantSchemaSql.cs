using Beneficios.Domain;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public static class TenantSchemaSql
{
    public static string CreateSchema(string schemaName) =>
        $"CREATE SCHEMA IF NOT EXISTS {QuoteIdentifier(schemaName)};";

    public static string CreateUsuariosTable(string schemaName)
    {
        var quotedSchema = QuoteIdentifier(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.usuarios (
                id UUID PRIMARY KEY,
                nome VARCHAR(50) NOT NULL,
                senha VARCHAR(50) NOT NULL,
                email VARCHAR(256) NOT NULL,
                perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa',
                empresa_id UUID NOT NULL,
                token TEXT NULL,
                codigo_alterar_senha VARCHAR(20) NULL,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                usuario_alteracao_id UUID NULL,
                CONSTRAINT fk_{indexPrefix}_usuarios_empresa
                    FOREIGN KEY (empresa_id) REFERENCES beneficios.empresas(id) ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS idx_{indexPrefix}_usuarios_email
                ON {quotedSchema}.usuarios(email);
            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_usuarios_empresa_id
                ON {quotedSchema}.usuarios(empresa_id);
            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_usuarios_nome
                ON {quotedSchema}.usuarios(nome);
            """;
    }

    public static string InsertDefaultUsuario(string schemaName)
    {
        var quotedSchema = QuoteIdentifier(schemaName);

        return $"""
            INSERT INTO {quotedSchema}.usuarios
                (id, nome, senha, email, perfil, empresa_id, data_inclusao)
            VALUES
                (@Id, @Nome, @Senha, @Email, 'Empresa', @EmpresaId, @DataInclusao)
            """;
    }

    public static string BuildSchemaName(string razaoSocial) =>
        TenantSchemaNames.FromRazaoSocial(razaoSocial);

    public static string QuoteIdentifier(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
