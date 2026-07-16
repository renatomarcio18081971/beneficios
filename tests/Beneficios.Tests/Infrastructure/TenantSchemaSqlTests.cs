using Beneficios.Infrastructure.Tenancy;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

public class TenantSchemaSqlTests
{
    [Fact]
    public void RenomearCodigoMenuDiasUteisParaCalendario_DeveMigrarCodigo()
    {
        var sql = TenantSchemaSql.RenomearCodigoMenuDiasUteisParaCalendario("tenant_acme");
        Assert.Contains("dias_uteis", sql);
        Assert.Contains("calendario", sql);
        Assert.Contains("NOT EXISTS", sql);
        Assert.Contains("DELETE FROM", sql);
    }

    [Fact]
    public void InserirPerfilPermissaoSeAusente_DeveUsarWhereNotExists()
    {
        var sql = TenantSchemaSql.InserirPerfilPermissaoSeAusente("tenant_acme");
        Assert.Contains("INSERT INTO", sql);
        Assert.Contains("WHERE NOT EXISTS", sql);
        Assert.Contains("codigo_menu", sql);
    }

    [Fact]
    public void CriarTabelaFuncionarioAfastamentos_DeveCriarTabelaEIndices()
    {
        var sql = TenantSchemaSql.CriarTabelaFuncionarioAfastamentos("tenant_acme");
        Assert.Contains("funcionario_afastamentos", sql);
        Assert.Contains("CREATE INDEX IF NOT EXISTS", sql);
        Assert.Contains("FOREIGN KEY", sql);
    }

    [Fact]
    public void RecalcularSituacoesPorAfastamento_DeveUsarAfastadoEPreservarDesligado()
    {
        var sql = TenantSchemaSql.RecalcularSituacoesPorAfastamento("tenant_acme");
        Assert.Contains("'afastado'", sql);
        Assert.Contains("'ativo'", sql);
        Assert.Contains("desligado", sql);
    }
}
