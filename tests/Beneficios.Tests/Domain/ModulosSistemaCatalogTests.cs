using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class ModulosSistemaCatalogTests
{
    [Fact]
    public void Todos_DeveConterSomenteModulosComValidacaoDePerfil()
    {
        var codigos = ModulosSistemaCatalog.Todos.Select(m => m.Codigo).ToArray();
        Assert.DoesNotContain("dashboard", codigos);
        Assert.Contains("usuarios", codigos);
        Assert.Contains("perfis", codigos);
        Assert.Contains("dias_uteis", codigos);
        Assert.Contains("funcionarios", codigos);
        Assert.Contains("afastamentos", codigos);
    }

    [Fact]
    public void Todos_DeveConterDiasUteis()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "dias_uteis" && m.Rota == "/dias-uteis");
    }

    [Fact]
    public void Todos_DeveConterFuncionarios()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "funcionarios" && m.Rota == "/funcionarios");
    }

    [Fact]
    public void Todos_DeveConterAfastamentos()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "afastamentos" && m.Rota == "/afastamentos");
    }

    [Fact]
    public void ModulosDePerfil_DevemSuportarTodasAsAcoes()
    {
        foreach (var codigo in new[] { "usuarios", "perfis", "dias_uteis", "funcionarios", "afastamentos" })
        {
            var modulo = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == codigo);
            Assert.Equal(AcaoPermissao.Todas, modulo.AcoesSuportadas);
        }
    }
}
