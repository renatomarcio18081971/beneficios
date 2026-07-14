using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class FuncionarioQueryResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Matricula { get; set; }
    public DateOnly DataAdmissao { get; set; }
    public DateOnly? DataDesligamento { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
    public TipoContrato TipoContrato { get; set; }
    public string? CentroCusto { get; set; }
    public string? ResCep { get; set; }
    public string? ResLogradouro { get; set; }
    public string? ResNumero { get; set; }
    public string? ResComplemento { get; set; }
    public string? ResBairro { get; set; }
    public string? ResCidade { get; set; }
    public string? ResUf { get; set; }
    public string? TrabNomeLocal { get; set; }
    public string? TrabCep { get; set; }
    public string? TrabLogradouro { get; set; }
    public string? TrabNumero { get; set; }
    public string? TrabComplemento { get; set; }
    public string? TrabBairro { get; set; }
    public string? TrabCidade { get; set; }
    public string? TrabUf { get; set; }
    public SituacaoFuncionario Situacao { get; set; }
    public string? MotivoAfastamento { get; set; }
    public JornadaTrabalho Jornada { get; set; }
    public string? JornadaDetalhe { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
