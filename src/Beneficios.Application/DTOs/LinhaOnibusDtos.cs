namespace Beneficios.Application.DTOs;

public record LinhaOnibusDto(Guid Id, string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusSalvarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusAtualizarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);