using Beneficios.Domain;
using Beneficios.Infrastructure.Tenancy;
using Dapper;
using Npgsql;
using System.Data;
using Testcontainers.PostgreSql;

namespace Beneficios.Tests.Infrastructure;

public class PostgresFixture : IDisposable
{
    private PostgreSqlContainer? _container;
    public IDbConnection? Connection { get; }
    public string? CatalogConnectionString { get; }
    public bool Disponivel { get; }

    public PostgresFixture()
    {
        var connectionString = ResolveConnectionString();

        if (connectionString is null)
        {
            Disponivel = false;
            Connection = null;
            CatalogConnectionString = null;
            return;
        }

        CatalogConnectionString = connectionString;

        try
        {
            Connection = new NpgsqlConnection(connectionString);
            Connection.Open();
            Disponivel = true;
            InitializeSchemaAsync(Connection).GetAwaiter().GetResult();
        }
        catch
        {
            Disponivel = false;
            Connection?.Dispose();
            Connection = null;
        }
    }

    private string? ResolveConnectionString()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("BENEFICIOS_TEST_CONNECTION"),
            "Host=192.168.18.70;Port=5433;Database=postgres;Username=postgres;Password=senha123;Search Path=beneficios"
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            if (TryOpenConnection(EnsureSearchPath(candidate)))
                return EnsureSearchPath(candidate);
        }

        return TryStartTestContainer();
    }

    private static string EnsureSearchPath(string connectionString)
    {
        if (connectionString.Contains("Search Path", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        return connectionString.TrimEnd(';') + ";Search Path=beneficios";
    }

    private static bool TryOpenConnection(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? TryStartTestContainer()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            _container.StartAsync().GetAwaiter().GetResult();

            var connectionString = _container.GetConnectionString() + ";Search Path=beneficios";
            return TryOpenConnection(connectionString) ? connectionString : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task ResetDataAsync()
    {
        if (!Disponivel || Connection is null)
            return;

        if (Connection.State != ConnectionState.Open)
            Connection.Open();

        await Connection.ExecuteAsync("""
            DO $$
            DECLARE schema_record RECORD;
            BEGIN
                FOR schema_record IN
                    SELECT schema_name
                    FROM information_schema.schemata
                    WHERE schema_name LIKE 'tenant_%'
                LOOP
                    EXECUTE format('DROP SCHEMA IF EXISTS %I CASCADE', schema_record.schema_name);
                END LOOP;
            END $$;
            """);

        await Connection.ExecuteAsync("TRUNCATE TABLE beneficios.usuarios, beneficios.empresas CASCADE;");
    }

    public async Task<NpgsqlConnection> CreateTenantConnectionAsync(string razaoSocial)
    {
        if (!Disponivel || Connection is null || CatalogConnectionString is null)
            throw new InvalidOperationException("PostgreSQL indisponível para testes.");

        await ProvisionTenantSchemaAsync(Connection, razaoSocial);

        var builder = new NpgsqlConnectionStringBuilder(CatalogConnectionString)
        {
            SearchPath = TenantSchemaNames.ObterSearchPathTenant(razaoSocial),
        };

        var tenantConnection = new NpgsqlConnection(builder.ConnectionString);
        await tenantConnection.OpenAsync();
        return tenantConnection;
    }

    public static async Task ProvisionTenantSchemaAsync(IDbConnection connection, string razaoSocial)
    {
        var schemaName = TenantSchemaSql.MontarNomeSchema(razaoSocial);
        await connection.ExecuteAsync(TenantSchemaSql.CriarSchema(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaUsuarios(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaPerfis(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaPerfilPermissoes(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.AlterarUsuariosAdicionarPerfilId(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaCalendarioDias(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarios(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarioBeneficios(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.CriarTabelaFuncionarioAfastamentos(schemaName));
        await connection.ExecuteAsync(TenantSchemaSql.DropColunaMotivoAfastamento(schemaName));
    }

    public void Dispose()
    {
        Connection?.Dispose();
        _container?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private static async Task InitializeSchemaAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync("DROP TABLE IF EXISTS beneficios.usuarios CASCADE;");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS beneficios.empresas CASCADE;");

        var scriptsPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Beneficios.Infrastructure", "Scripts"));

        var schemaScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "00_Create_Schema.sql"));
        var empresasScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "01_Create_Table_Empresas.sql"));
        var usuariosScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "02_Create_Table_Usuarios.sql"));

        await connection.ExecuteAsync(schemaScript);
        await connection.ExecuteAsync(empresasScript);
        await connection.ExecuteAsync(usuariosScript);
    }
}
