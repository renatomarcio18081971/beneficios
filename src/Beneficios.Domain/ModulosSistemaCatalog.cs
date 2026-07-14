using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;

namespace Beneficios.Domain;

public static class ModulosSistemaCatalog
{
    /// <summary>
    /// Módulos sujeitos a validação de perfil.
    /// Dashboard fica fora do catálogo — todos os usuários tenant podem visualizá-lo.
    /// </summary>
    public static IReadOnlyList<ModuloSistema> Todos { get; } =
    [
        new("usuarios", "Usuários", "/usuarios", AcaoPermissao.Todas),
        new("perfis", "Perfis", "/perfis", AcaoPermissao.Todas),
        new("dias_uteis", "Calendário", "/dias-uteis", AcaoPermissao.Todas),
    ];

    public static ModuloSistema? ObterPorCodigo(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
