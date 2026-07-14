using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class FuncionarioAtualizarParams
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string? Matricula { get; init; }
    public DateOnly DataAdmissao { get; init; }
    public DateOnly? DataDesligamento { get; init; }
    public string Cargo { get; init; } = string.Empty;
    public decimal SalarioBase { get; init; }
    public TipoContrato TipoContrato { get; init; }
    public string? CentroCusto { get; init; }
    public string? ResCep { get; init; }
    public string? ResLogradouro { get; init; }
    public string? ResNumero { get; init; }
    public string? ResComplemento { get; init; }
    public string? ResBairro { get; init; }
    public string? ResCidade { get; init; }
    public string? ResUf { get; init; }
    public string? TrabNomeLocal { get; init; }
    public string? TrabCep { get; init; }
    public string? TrabLogradouro { get; init; }
    public string? TrabNumero { get; init; }
    public string? TrabComplemento { get; init; }
    public string? TrabBairro { get; init; }
    public string? TrabCidade { get; init; }
    public string? TrabUf { get; init; }
    public SituacaoFuncionario Situacao { get; init; }
    public string? MotivoAfastamento { get; init; }
    public JornadaTrabalho Jornada { get; init; }
    public string? JornadaDetalhe { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
