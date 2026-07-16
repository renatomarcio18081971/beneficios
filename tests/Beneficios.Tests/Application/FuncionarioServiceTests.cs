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
    private readonly Mock<IFuncionarioAfastamentoRepository> _afastRepo = new();
    private readonly FuncionarioService _sut;

    public FuncionarioServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioProfile>()).CreateMapper();
        _sut = new FuncionarioService(_repo.Object, _afastRepo.Object, mapper);
    }

    [Fact]
    public async Task SalvarAsync_SituacaoAfastado_DeveFalhar()
    {
        var dto = CriarDto(situacao: SituacaoFuncionario.Afastado);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemSituacaoInvalida, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_AtivoComAfastamentoAtivo_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
        {
            Id = id,
            Nome = "Ana",
            Cpf = "52998224725",
            Cargo = "Analista",
            SalarioBase = 1,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Afastado,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataAdmissao = new DateOnly(2024, 1, 1),
            DataInclusao = DateTime.UtcNow,
        });
        _afastRepo.Setup(r => r.ObterAtivoEmAsync(id, It.IsAny<DateOnly>()))
            .ReturnsAsync(new FuncionarioAfastamentoQueryResult
            {
                Id = Guid.NewGuid(),
                FuncionarioId = id,
                Tipo = "ferias",
                DataInicio = new DateOnly(2026, 1, 1),
            });

        var dto = CriarAtualizarDto(SituacaoFuncionario.Ativo);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AtualizarAsync(id, dto, null));
        Assert.Equal(FuncionarioService.MensagemAtivoComAfastamento, ex.Message);
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

    [Fact]
    public async Task SalvarAsync_CpfDuplicado_DeveFalhar()
    {
        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(CriarDto(), null));
        Assert.Equal(FuncionarioService.MensagemCpfDuplicado, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_MatriculaDuplicada_DeveFalhar()
    {
        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        _repo.Setup(r => r.MatriculaExisteAsync("M1", null)).ReturnsAsync(true);

        var dto = CriarDto() with { Matricula = "M1" };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemMatriculaDuplicada, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_BeneficioNaoSuportado_DeveFalhar()
    {
        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
        var dto = CriarDto() with
        {
            Beneficios =
            [
                new FuncionarioBeneficioSalvarDto("vale_refeicao", true, new DateOnly(2024, 1, 10), null, true),
            ],
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioService.MensagemBeneficioNaoSuportado, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_NaoEncontrado_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioQueryResult?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.AtualizarAsync(id, CriarAtualizarDto(SituacaoFuncionario.Ativo), null));
        Assert.Equal(FuncionarioService.MensagemNaoEncontrado, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_Valido_DevePersistir()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
        {
            Id = id,
            Nome = "Ana",
            Cpf = "52998224725",
            Cargo = "Analista",
            SalarioBase = 1,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataAdmissao = new DateOnly(2024, 1, 1),
            DataInclusao = DateTime.UtcNow,
        });
        _afastRepo.Setup(r => r.ObterAtivoEmAsync(id, It.IsAny<DateOnly>()))
            .ReturnsAsync((FuncionarioAfastamentoQueryResult?)null);
        _repo.Setup(r => r.CpfExisteAsync(It.IsAny<string>(), id)).ReturnsAsync(false);

        await _sut.AtualizarAsync(id, CriarAtualizarDto(SituacaoFuncionario.Ativo), null);

        _repo.Verify(r => r.AtualizarAsync(
            It.Is<FuncionarioAtualizarParams>(p => p.Id == id),
            It.IsAny<IReadOnlyList<FuncionarioBeneficioSalvarParams>>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveCarregarBeneficios()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioQueryResult
        {
            Id = id,
            Nome = "Ana",
            Cpf = "52998224725",
            Cargo = "Analista",
            SalarioBase = 1,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataAdmissao = new DateOnly(2024, 1, 1),
            DataInclusao = DateTime.UtcNow,
        });
        _repo.Setup(r => r.ObterBeneficiosAsync(id)).ReturnsAsync(
        [
            new FuncionarioBeneficioQueryResult
            {
                Id = Guid.NewGuid(),
                FuncionarioId = id,
                CodigoBeneficio = "vale_transporte",
                Ativo = true,
                OptIn = true,
            },
        ]);

        var dto = await _sut.ObterPorIdAsync(id);
        Assert.NotNull(dto);
        Assert.Single(dto!.Beneficios);
        Assert.Equal("vale_transporte", dto.Beneficios[0].CodigoBeneficio);
    }

    [Fact]
    public async Task FiltrarAsync_DeveZerarBeneficios()
    {
        _repo.Setup(r => r.FiltrarAsync(It.IsAny<FuncionarioFiltroParams>())).ReturnsAsync(
        [
            new FuncionarioQueryResult
            {
                Id = Guid.NewGuid(),
                Nome = "Ana",
                Cpf = "52998224725",
                Cargo = "Analista",
                SalarioBase = 1,
                TipoContrato = TipoContrato.Clt,
                Situacao = SituacaoFuncionario.Ativo,
                MotivoAfastamentoAtivo = "ferias",
                Jornada = JornadaTrabalho.QuarentaHorasSegSex,
                DataAdmissao = new DateOnly(2024, 1, 1),
                DataInclusao = DateTime.UtcNow,
            },
        ]);

        var lista = await _sut.FiltrarAsync(new FuncionarioFiltroDto(null, null, null, null));
        Assert.Single(lista);
        Assert.Empty(lista[0].Beneficios);
        Assert.Equal("ferias", lista[0].MotivoAfastamentoAtivo);
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

    private static FuncionarioAtualizarDto CriarAtualizarDto(SituacaoFuncionario situacao) => new(
        Nome: "Ana Silva",
        Cpf: "529.982.247-25",
        Matricula: null,
        DataAdmissao: new DateOnly(2024, 1, 10),
        DataDesligamento: null,
        Cargo: "Analista",
        SalarioBase: 3500m,
        TipoContrato: TipoContrato.Clt,
        CentroCusto: null,
        ResCep: null, ResLogradouro: null, ResNumero: null, ResComplemento: null,
        ResBairro: null, ResCidade: null, ResUf: null,
        TrabNomeLocal: null, TrabCep: null, TrabLogradouro: null, TrabNumero: null,
        TrabComplemento: null, TrabBairro: null, TrabCidade: null, TrabUf: null,
        Situacao: situacao,
        Jornada: JornadaTrabalho.QuarentaHorasSegSex,
        JornadaDetalhe: null,
        Beneficios: null);
}
