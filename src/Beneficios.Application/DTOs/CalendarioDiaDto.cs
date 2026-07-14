using Beneficios.Domain.Enums;

namespace Beneficios.Application.DTOs;

public record CalendarioDiaDto(
    Guid Id,
    DateOnly Data,
    bool EhDiaUtil,
    TipoExcecaoCalendario? TipoExcecao,
    OrigemCalendarioDia Origem,
    string? Observacao);

public record CalendarioDiaAtualizarDto(
    bool EhDiaUtil,
    TipoExcecaoCalendario? TipoExcecao,
    string? Observacao);

public record GerarAnoCalendarioDto(int Ano);
