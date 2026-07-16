using Beneficios.Domain.Enums;

namespace Beneficios.Application.DTOs;

public record AfastamentoDto(
    Guid Id,
    Guid FuncionarioId,
    string FuncionarioNome,
    TipoAfastamento Tipo,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string? Observacao);

public record AfastamentoSalvarDto(
    Guid FuncionarioId,
    TipoAfastamento Tipo,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string? Observacao);

public record AfastamentoAtualizarDto(
    TipoAfastamento Tipo,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string? Observacao);
