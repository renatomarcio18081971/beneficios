using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;

namespace Beneficios.Domain;

public static class ModulosSistemaCatalog
{
    public static IReadOnlyList<ModuloSistema> Todos { get; } =
    [
        new("dashboard", "Dashboard", "/dashboard", AcaoPermissao.Visualizar),
        new("usuarios", "Usuários", "/usuarios", AcaoPermissao.Todas),
        new("perfis", "Perfis", "/perfis", AcaoPermissao.Todas),
    ];

    public static ModuloSistema? ObterPorCodigo(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
