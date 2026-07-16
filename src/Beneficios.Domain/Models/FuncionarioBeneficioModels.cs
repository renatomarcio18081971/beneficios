namespace Beneficios.Domain.Models;

public class FuncionarioBeneficioSalvarParams
{
    public Guid Id { get; init; }
    public string CodigoBeneficio { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public DateOnly? DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public bool OptIn { get; init; }
}

public class FuncionarioBeneficioQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string CodigoBeneficio { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool OptIn { get; set; }
}
