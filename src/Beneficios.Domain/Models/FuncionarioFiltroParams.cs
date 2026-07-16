using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class FuncionarioFiltroParams
{
    public string? Nome { get; init; }
    public string? Cpf { get; init; }
    public string? Matricula { get; init; }
    public SituacaoFuncionario? Situacao { get; init; }
}
