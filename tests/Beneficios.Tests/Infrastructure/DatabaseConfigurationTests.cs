using Beneficios.Infrastructure.Configurations;
using Npgsql;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

public class DatabaseConfigurationTests
{
    [Fact]
    public void CreateConnection_DeveRetornarNpgsqlConnection()
    {
        var connection = DatabaseConfiguration.CreateConnection("Host=localhost;Database=beneficios");

        Assert.IsType<NpgsqlConnection>(connection);
    }
}
