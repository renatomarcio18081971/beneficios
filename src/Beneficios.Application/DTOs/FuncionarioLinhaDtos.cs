namespace Beneficios.Application.DTOs;

public record FuncionarioLinhaDto(
    Guid Id, Guid FuncionarioId, string FuncionarioNome,
    Guid LinhaOnibusId, string LinhaDescricao,
    int Quantidade, DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaSalvarDto(
    Guid FuncionarioId, Guid LinhaOnibusId, int Quantidade,
    DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaAtualizarDto(
    Guid LinhaOnibusId, int Quantidade, DateOnly DataInicio, DateOnly? DataFim);
