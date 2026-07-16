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
        new("calendario", "Calendário", "/calendario", AcaoPermissao.Todas),
        new("funcionarios", "Funcionários", "/funcionarios", AcaoPermissao.Todas),
        new("afastamentos", "Afastamentos/Férias", "/afastamentos", AcaoPermissao.Todas),
        new("linhas_onibus", "Linhas de Ônibus", "/linhas-onibus", AcaoPermissao.Todas),
        new("funcionario_linhas", "Funcionário × Linhas", "/funcionario-linhas", AcaoPermissao.Todas),
    ];

    public static ModuloSistema? ObterPorCodigo(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
