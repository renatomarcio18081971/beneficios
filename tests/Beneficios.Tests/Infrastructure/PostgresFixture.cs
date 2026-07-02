using Dapper;
using Npgsql;
using System.Data;

namespace Beneficios.Tests.Infrastructure;

public class PostgresFixture : IDisposable
{
    public IDbConnection? Connection { get; }
    public bool Disponivel { get; }

    public PostgresFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable("BENEFICIOS_TEST_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=beneficios;Username=postgres;Password=postgres";

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

    public void Dispose()
    {
        Connection?.Dispose();
    }

    private static async Task InitializeSchemaAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync("DROP TABLE IF EXISTS usuarios CASCADE;");
        await connection.ExecuteAsync("DROP TABLE IF EXISTS empresas CASCADE;");

        var scriptsPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Beneficios.Infrastructure", "Scripts"));

        var empresasScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "01_Create_Table_Empresas.sql"));
        var usuariosScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "02_Create_Table_Usuarios.sql"));

        await connection.ExecuteAsync(empresasScript);
        await connection.ExecuteAsync(usuariosScript);
    }
}
