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
        Assert.Contains("calendario", codigos);
        Assert.Contains("funcionarios", codigos);
        Assert.Contains("afastamentos", codigos);
    }

    [Fact]
    public void Todos_DeveConterCalendario()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos, m => m.Codigo == "calendario" && m.Rota == "/calendario");
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
    public void Todos_DeveConterLinhasOnibus()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos,
            m => m.Codigo == "linhas_onibus" && m.Rota == "/linhas-onibus");
    }

    [Fact]
    public void Todos_DeveConterFuncionarioLinhas()
    {
        Assert.Contains(ModulosSistemaCatalog.Todos,
            m => m.Codigo == "funcionario_linhas" && m.Rota == "/funcionario-linhas");
    }

    [Fact]
    public void ModulosDePerfil_DevemSuportarTodasAsAcoes()
    {
        foreach (var codigo in new[] { "usuarios", "perfis", "calendario", "funcionarios", "afastamentos" })
        {
            var modulo = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == codigo);
            Assert.Equal(AcaoPermissao.Todas, modulo.AcoesSuportadas);
        }
    }

    [Fact]
    public void Todos_NaoDeveConterDiasUteis()
    {
        Assert.DoesNotContain(ModulosSistemaCatalog.Todos, m => m.Codigo == "dias_uteis");
        Assert.DoesNotContain(ModulosSistemaCatalog.Todos, m => m.Rota == "/dias-uteis");
    }

    [Fact]
    public void ObterPorCodigo_DeveSerCaseInsensitive()
    {
        Assert.NotNull(ModulosSistemaCatalog.ObterPorCodigo("CALENDARIO"));
        Assert.Null(ModulosSistemaCatalog.ObterPorCodigo("inexistente"));
    }
}
