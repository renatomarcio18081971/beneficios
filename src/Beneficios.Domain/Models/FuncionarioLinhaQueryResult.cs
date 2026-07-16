namespace Beneficios.Domain.Models;

public sealed class FuncionarioLinhaQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public Guid LinhaOnibusId { get; set; }
    public string LinhaDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
