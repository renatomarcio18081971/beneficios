using Npgsql;
using System.Data;

namespace Beneficios.Infrastructure.Configurations;

public class DatabaseConfiguration
{
    public static IDbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }
}
