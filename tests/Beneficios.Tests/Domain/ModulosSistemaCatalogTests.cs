using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class ModulosSistemaCatalogTests
{
    [Fact]
    public void Todos_DeveConterCodigosDaV1()
    {
        var codigos = ModulosSistemaCatalog.Todos.Select(m => m.Codigo).ToArray();
        Assert.Contains("dashboard", codigos);
        Assert.Contains("usuarios", codigos);
        Assert.Contains("perfis", codigos);
    }

    [Fact]
    public void Dashboard_DeveSuportarSomenteVisualizar()
    {
        var dashboard = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == "dashboard");
        Assert.Equal(AcaoPermissao.Visualizar, dashboard.AcoesSuportadas);
    }
}
