using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Xunit;

namespace Beneficios.Tests.Application;

public class FuncionarioProfileTests
{
    private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioProfile>()).CreateMapper();

    [Fact]
    public void MapearQueryResult_DevePreencherBeneficiosVazio()
    {
        var source = new FuncionarioQueryResult
        {
            Id = Guid.NewGuid(),
            Nome = "Ana",
            Cpf = "52998224725",
            Cargo = "Analista",
            SalarioBase = 1,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Afastado,
            MotivoAfastamentoAtivo = "ferias",
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataAdmissao = new DateOnly(2024, 1, 1),
            DataInclusao = DateTime.UtcNow,
        };

        var dto = _mapper.Map<FuncionarioDto>(source);

        Assert.Equal(source.Id, dto.Id);
        Assert.Equal("ferias", dto.MotivoAfastamentoAtivo);
        Assert.Empty(dto.Beneficios);
        Assert.Equal(SituacaoFuncionario.Afastado, dto.Situacao);
    }
}
