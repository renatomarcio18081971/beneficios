using Beneficios.Domain;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public static class TenantSchemaSql
{
    public static string CriarSchema(string schemaName) =>
        $"CREATE SCHEMA IF NOT EXISTS {CitarIdentificador(schemaName)};";

    public static string CriarTabelaUsuarios(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
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

    public static string CriarTabelaPerfis(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.perfis (
                id UUID PRIMARY KEY,
                nome VARCHAR(100) NOT NULL,
                eh_sistema BOOLEAN NOT NULL DEFAULT FALSE,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                usuario_alteracao_id UUID NULL
            );

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_perfis_nome
                ON {quotedSchema}.perfis(nome);
            """;
    }

    public static string CriarTabelaPerfilPermissoes(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.perfil_permissoes (
                id UUID PRIMARY KEY,
                perfil_id UUID NOT NULL,
                codigo_menu VARCHAR(50) NOT NULL,
                visualizar BOOLEAN NOT NULL DEFAULT FALSE,
                criar BOOLEAN NOT NULL DEFAULT FALSE,
                editar BOOLEAN NOT NULL DEFAULT FALSE,
                excluir BOOLEAN NOT NULL DEFAULT FALSE,
                CONSTRAINT fk_{indexPrefix}_perfil_permissoes_perfil
                    FOREIGN KEY (perfil_id) REFERENCES {quotedSchema}.perfis(id) ON DELETE CASCADE,
                CONSTRAINT uq_{indexPrefix}_perfil_permissoes_menu
                    UNIQUE (perfil_id, codigo_menu)
            );

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_perfil_permissoes_perfil_id
                ON {quotedSchema}.perfil_permissoes(perfil_id);
            """;
    }

    public static string AlterarUsuariosAdicionarPerfilId(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            ALTER TABLE {quotedSchema}.usuarios
                ADD COLUMN IF NOT EXISTS perfil_id UUID NULL;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint
                    WHERE conname = 'fk_{indexPrefix}_usuarios_perfil'
                ) THEN
                    ALTER TABLE {quotedSchema}.usuarios
                        ADD CONSTRAINT fk_{indexPrefix}_usuarios_perfil
                        FOREIGN KEY (perfil_id) REFERENCES {quotedSchema}.perfis(id);
                END IF;
            END $$;

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_usuarios_perfil_id
                ON {quotedSchema}.usuarios(perfil_id);
            """;
    }

    public static string CriarTabelaCalendarioDias(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.calendario_dias (
                id UUID PRIMARY KEY,
                data DATE NOT NULL,
                eh_dia_util BOOLEAN NOT NULL,
                tipo_excecao VARCHAR(40) NULL,
                origem VARCHAR(20) NOT NULL,
                observacao TEXT NULL,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                usuario_alteracao_id UUID NULL,
                CONSTRAINT uq_{indexPrefix}_calendario_dias_data UNIQUE (data)
            );

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_calendario_dias_data
                ON {quotedSchema}.calendario_dias(data);
            """;
    }

    public static string CriarTabelaFuncionarios(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.funcionarios (
                id UUID PRIMARY KEY,
                nome VARCHAR(200) NOT NULL,
                cpf VARCHAR(11) NOT NULL,
                matricula VARCHAR(50) NULL,
                data_admissao DATE NOT NULL,
                data_desligamento DATE NULL,
                cargo VARCHAR(200) NOT NULL,
                salario_base NUMERIC(18,2) NOT NULL,
                tipo_contrato VARCHAR(20) NOT NULL,
                centro_custo VARCHAR(200) NULL,
                res_cep VARCHAR(8) NULL,
                res_logradouro VARCHAR(200) NULL,
                res_numero VARCHAR(30) NULL,
                res_complemento VARCHAR(100) NULL,
                res_bairro VARCHAR(100) NULL,
                res_cidade VARCHAR(100) NULL,
                res_uf VARCHAR(2) NULL,
                trab_nome_local VARCHAR(200) NULL,
                trab_cep VARCHAR(8) NULL,
                trab_logradouro VARCHAR(200) NULL,
                trab_numero VARCHAR(30) NULL,
                trab_complemento VARCHAR(100) NULL,
                trab_bairro VARCHAR(100) NULL,
                trab_cidade VARCHAR(100) NULL,
                trab_uf VARCHAR(2) NULL,
                situacao VARCHAR(20) NOT NULL,
                motivo_afastamento TEXT NULL,
                jornada VARCHAR(40) NOT NULL,
                jornada_detalhe VARCHAR(200) NULL,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                usuario_alteracao_id UUID NULL,
                CONSTRAINT uq_{indexPrefix}_funcionarios_cpf UNIQUE (cpf)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS uq_{indexPrefix}_funcionarios_matricula
                ON {quotedSchema}.funcionarios(matricula)
                WHERE matricula IS NOT NULL;

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_funcionarios_nome
                ON {quotedSchema}.funcionarios(nome);

            CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_funcionarios_situacao
                ON {quotedSchema}.funcionarios(situacao);
            """;
    }

    public static string CriarTabelaFuncionarioBeneficios(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);
        var indexPrefix = schemaName.Replace('-', '_');

        return $"""
            CREATE TABLE IF NOT EXISTS {quotedSchema}.funcionario_beneficios (
                id UUID PRIMARY KEY,
                funcionario_id UUID NOT NULL,
                codigo_beneficio VARCHAR(40) NOT NULL,
                ativo BOOLEAN NOT NULL,
                data_inicio DATE NULL,
                data_fim DATE NULL,
                opt_in BOOLEAN NOT NULL DEFAULT FALSE,
                data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
                data_alteracao TIMESTAMP NULL,
                CONSTRAINT fk_{indexPrefix}_func_benef_funcionario
                    FOREIGN KEY (funcionario_id) REFERENCES {quotedSchema}.funcionarios(id),
                CONSTRAINT uq_{indexPrefix}_func_benef_codigo
                    UNIQUE (funcionario_id, codigo_beneficio)
            );
            """;
    }

    public static string InserirPerfilDono(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);

        return $"""
            INSERT INTO {quotedSchema}.perfis
                (id, nome, eh_sistema, data_inclusao)
            VALUES
                (@Id, @Nome, TRUE, @DataInclusao)
            """;
    }

    public static string InserirPerfilPermissao(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);

        return $"""
            INSERT INTO {quotedSchema}.perfil_permissoes
                (id, perfil_id, codigo_menu, visualizar, criar, editar, excluir)
            VALUES
                (@Id, @PerfilId, @CodigoMenu, @Visualizar, @Criar, @Editar, @Excluir)
            """;
    }

    public static string InserirUsuarioPadrao(string schemaName)
    {
        var quotedSchema = CitarIdentificador(schemaName);

        return $"""
            INSERT INTO {quotedSchema}.usuarios
                (id, nome, senha, email, perfil, empresa_id, perfil_id, data_inclusao)
            VALUES
                (@Id, @Nome, @Senha, @Email, 'Empresa', @EmpresaId, @PerfilId, @DataInclusao)
            """;
    }

    public static string MontarNomeSchema(string razaoSocial) =>
        TenantSchemaNames.FromRazaoSocial(razaoSocial);

    public static string CitarIdentificador(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
