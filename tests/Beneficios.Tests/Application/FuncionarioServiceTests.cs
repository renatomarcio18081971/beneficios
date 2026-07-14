using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class FuncionarioServiceTests
{
    private readonly Mock<IFuncionarioRepository> _repo = new();
    private readonly FuncionarioService _sut;

    public FuncionarioServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioProfile>()).CreateMapper();
        _sut = new FuncionarioService(_repo.Object, mapper);
    }

    [Fact]
    public async Task SalvarAsync_CpfInvalido_DeveFalhar()
    {
        var dto = CriarDto(cpf: "11111111111");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemCpfInvalido, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_DesligadoSemData_DeveFalhar()
    {
        var dto = CriarDto(situacao: SituacaoFuncionario.Desligado, dataDesligamento: null);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemDesligamento, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_JornadaEspecialSemDetalhe_DeveFalhar()
    {
        var dto = CriarDto(jornada: JornadaTrabalho.EspecialCategoria, jornadaDetalhe: null);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemJornadaEspecial, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_Valido_DevePersistir()
    {
        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _repo.Setup(r => r.SalvarAsync(It.IsAny<FuncionarioSalvarParams>(), It.IsAny<IReadOnlyList<FuncionarioBeneficioSalvarParams>>()))
            .ReturnsAsync((FuncionarioSalvarParams p, IReadOnlyList<FuncionarioBeneficioSalvarParams> _) => p.Id);

        var id = await _sut.SalvarAsync(CriarDto(), null);
        Assert.NotEqual(Guid.Empty, id);
        _repo.Verify(r => r.SalvarAsync(
            It.Is<FuncionarioSalvarParams>(p => p.Cpf == "52998224725"),
            It.IsAny<IReadOnlyList<FuncionarioBeneficioSalvarParams>>()), Times.Once);
    }

    private static FuncionarioSalvarDto CriarDto(
        string cpf = "529.982.247-25",
        SituacaoFuncionario situacao = SituacaoFuncionario.Ativo,
        DateOnly? dataDesligamento = null,
        JornadaTrabalho jornada = JornadaTrabalho.QuarentaHorasSegSex,
        string? jornadaDetalhe = null) => new(
        Nome: "Ana Silva",
        Cpf: cpf,
        Matricula: null,
        DataAdmissao: new DateOnly(2024, 1, 10),
        DataDesligamento: dataDesligamento,
        Cargo: "Analista",
        SalarioBase: 3500m,
        TipoContrato: TipoContrato.Clt,
        CentroCusto: null,
        ResCep: null, ResLogradouro: null, ResNumero: null, ResComplemento: null,
        ResBairro: null, ResCidade: null, ResUf: null,
        TrabNomeLocal: null, TrabCep: null, TrabLogradouro: null, TrabNumero: null,
        TrabComplemento: null, TrabBairro: null, TrabCidade: null, TrabUf: null,
        Situacao: situacao,
        Jornada: jornada,
        JornadaDetalhe: jornadaDetalhe,
        Beneficios: new[]
        {
            new FuncionarioBeneficioSalvarDto("vale_transporte", true, new DateOnly(2024, 1, 10), null, true),
        });
}
