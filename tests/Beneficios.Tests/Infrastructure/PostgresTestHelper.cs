namespace Beneficios.Tests.Infrastructure;

internal static class PostgresTestHelper
{
    public static async Task PrepareAsync(PostgresFixture fixture)
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");
        await fixture.ResetDataAsync();
    }
}
