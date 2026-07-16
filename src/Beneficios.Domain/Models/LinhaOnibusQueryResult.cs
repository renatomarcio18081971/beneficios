namespace Beneficios.Domain.Models;

public sealed class LinhaOnibusQueryResult
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal ValorTarifa { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
