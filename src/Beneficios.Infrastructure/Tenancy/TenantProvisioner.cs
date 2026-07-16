using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Configurations;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public class TenantProvisioner : ITenantProvisioner
{
    private readonly string _catalogConnectionString;

    public TenantProvisioner(IConfiguration configuration)
    {
        _catalogConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task ProvisionarAsync(
        string razaoSocial,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var schemaName = TenantSchemaSql.MontarNomeSchema(razaoSocial);
        var agora = DateTime.UtcNow;

        using IDbConnection connection = DatabaseConfiguration.CreateConnection(_catalogConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarSchema(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaUsuarios(schemaName),
            cancellationToken: cancellationToken));

        var perfilId = await GarantirPerfisNoSchemaAsync(connection, schemaName, agora, cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaCalendarioDias(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarios(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioBeneficios(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaLinhasOnibus(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioLinhas(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.DropColunaMotivoAfastamento(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.RecalcularSituacoesPorAfastamento(schemaName),
            cancellationToken: cancellationToken));

        await CalendarioAnoGerador.GerarSeAusenteAsync(
            connection,
            schemaName,
            DateTime.UtcNow.Year,
            cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.InserirUsuarioPadrao(schemaName),
            new
            {
                Id = Guid.NewGuid(),
                Nome = TenantDefaultUser.Nome,
                Senha = Criptografia.Encrypt(TenantDefaultUser.Senha),
                Email = TenantDefaultUser.Email,
                EmpresaId = empresaId,
                PerfilId = perfilId,
                DataInclusao = agora,
            },
            cancellationToken: cancellationToken));
    }

    public async Task GarantirPerfisEmTenantsExistentesAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = DatabaseConfiguration.CreateConnection(_catalogConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var schemas = (await connection.QueryAsync<string>(new CommandDefinition(
            """
            SELECT schema_name
            FROM information_schema.schemata
            WHERE schema_name LIKE 'tenant_%'
            ORDER BY schema_name
            """,
            cancellationToken: cancellationToken))).ToArray();

        var agora = DateTime.UtcNow;
        foreach (var schemaName in schemas)
        {
            await GarantirPerfisNoSchemaAsync(connection, schemaName, agora, cancellationToken);
            await VincularUsuarioPadraoAoDonoAsync(connection, schemaName, cancellationToken);
        }
    }

    private static async Task<Guid> GarantirPerfisNoSchemaAsync(
        IDbConnection connection,
        string schemaName,
        DateTime agora,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaPerfis(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaPerfilPermissoes(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.AlterarUsuariosAdicionarPerfilId(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaCalendarioDias(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarios(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioBeneficios(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaLinhasOnibus(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CriarTabelaFuncionarioLinhas(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.DropColunaMotivoAfastamento(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.RecalcularSituacoesPorAfastamento(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.RenomearCodigoMenuDiasUteisParaCalendario(schemaName),
            cancellationToken: cancellationToken));

        var quoted = TenantSchemaSql.CitarIdentificador(schemaName);
        var donoId = await connection.ExecuteScalarAsync<Guid?>(new CommandDefinition(
            $"""
            SELECT id FROM {quoted}.perfis WHERE eh_sistema = TRUE LIMIT 1
            """,
            cancellationToken: cancellationToken));

        if (donoId is Guid existente)
        {
            foreach (var modulo in ModulosSistemaCatalog.Todos)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    TenantSchemaSql.InserirPerfilPermissaoSeAusente(schemaName),
                    new
                    {
                        Id = Guid.NewGuid(),
                        PerfilId = existente,
                        CodigoMenu = modulo.Codigo,
                        Visualizar = true,
                        Criar = true,
                        Editar = true,
                        Excluir = true,
                    },
                    cancellationToken: cancellationToken));
            }

            return existente;
        }

        var perfilId = Guid.NewGuid();
        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.InserirPerfilDono(schemaName),
            new
            {
                Id = perfilId,
                Nome = "Dono",
                DataInclusao = agora,
            },
            cancellationToken: cancellationToken));

        foreach (var modulo in ModulosSistemaCatalog.Todos)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                TenantSchemaSql.InserirPerfilPermissao(schemaName),
                new
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = modulo.Codigo,
                    Visualizar = true,
                    Criar = true,
                    Editar = true,
                    Excluir = true,
                },
                cancellationToken: cancellationToken));
        }

        return perfilId;
    }

    private static async Task VincularUsuarioPadraoAoDonoAsync(
        IDbConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var quoted = TenantSchemaSql.CitarIdentificador(schemaName);
        await connection.ExecuteAsync(new CommandDefinition(
            $"""
            UPDATE {quoted}.usuarios u
            SET perfil_id = p.id
            FROM {quoted}.perfis p
            WHERE p.eh_sistema = TRUE
              AND LOWER(u.email) = LOWER(@Email)
              AND u.perfil_id IS NULL
            """,
            new { Email = TenantDefaultUser.Email },
            cancellationToken: cancellationToken));
    }
}
