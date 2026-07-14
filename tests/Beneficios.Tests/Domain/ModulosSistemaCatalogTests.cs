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
    }

    [Fact]
    public void UsuariosEPerfis_DevemSuportarTodasAsAcoes()
    {
        foreach (var codigo in new[] { "usuarios", "perfis" })
        {
            var modulo = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == codigo);
            Assert.Equal(AcaoPermissao.Todas, modulo.AcoesSuportadas);
        }
    }
}
