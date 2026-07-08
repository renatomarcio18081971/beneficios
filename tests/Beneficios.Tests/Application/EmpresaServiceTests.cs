using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class EmpresaServiceTests
{
    private readonly Mock<IEmpresaRepository> _repositoryMock;
    private readonly Mock<ITenantProvisioner> _provisionerMock;
    private readonly IMapper _mapper;
    private readonly EmpresaService _service;

    public EmpresaServiceTests()
    {
        _repositoryMock = new Mock<IEmpresaRepository>();
        _provisionerMock = new Mock<ITenantProvisioner>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<EmpresaProfile>()).CreateMapper();
        _service = new EmpresaService(_repositoryMock.Object, _provisionerMock.Object, _mapper);
    }

    [Fact]
    public async Task SalvarAsync_DeveCriarEmpresaEProvisionarSchemaComUsuarioPadrao()
    {
        var dto = new EmpresaSalvarDto("Empresa Teste LTDA", "empresateste");

        _repositoryMock.Setup(x => x.SalvarAsync(It.IsAny<EmpresaSalvarParams>()))
            .ReturnsAsync((EmpresaSalvarParams p) => p.Id);
        _provisionerMock
            .Setup(x => x.ProvisionAsync(dto.RazaoSocial, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.SalvarAsync(dto);

        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(x => x.SalvarAsync(It.Is<EmpresaSalvarParams>(p =>
            p.RazaoSocial == dto.RazaoSocial && p.Dominio == dto.Dominio)), Times.Once);
        _provisionerMock.Verify(
            x => x.ProvisionAsync(dto.RazaoSocial, result, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SalvarAsync_QuandoProvisionamentoFalhar_DeveRemoverEmpresaCriada()
    {
        var dto = new EmpresaSalvarDto("Empresa Teste LTDA", "empresateste");

        _repositoryMock.Setup(x => x.SalvarAsync(It.IsAny<EmpresaSalvarParams>()))
            .ReturnsAsync((EmpresaSalvarParams p) => p.Id);
        _provisionerMock
            .Setup(x => x.ProvisionAsync(dto.RazaoSocial, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha no schema"));
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SalvarAsync(dto));

        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarEmpresaComSucesso()
    {
        var id = Guid.NewGuid();
        var dto = new EmpresaAtualizarDto("Empresa Teste LTDA", "empresateste");

        _repositoryMock.Setup(x => x.AtualizarAsync(It.IsAny<EmpresaAtualizarParams>()))
            .ReturnsAsync(true);

        var result = await _service.AtualizarAsync(id, dto, null);

        Assert.True(result);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.Is<EmpresaAtualizarParams>(p =>
            p.Id == id && p.RazaoSocial == dto.RazaoSocial)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveRemoverEmpresaSemExcluirSchema()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
        _repositoryMock.Verify(x => x.ObterUmAsync(It.IsAny<Guid>()), Times.Never);
        _provisionerMock.Verify(
            x => x.ProvisionAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarFalseQuandoEmpresaNaoExiste()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(id);

        Assert.False(result);
    }

    [Fact]
    public async Task ObterUmAsync_DeveRetornarEmpresaQuandoEncontrada()
    {
        var id = Guid.NewGuid();
        var queryResult = new EmpresaQueryResult
        {
            Id = id,
            RazaoSocial = "Empresa Teste LTDA",
            Dominio = "empresateste",
            DataInclusao = DateTime.UtcNow
        };

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(queryResult);

        var result = await _service.ObterUmAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Empresa Teste LTDA", result.RazaoSocial);
    }

    [Fact]
    public async Task ObterUmAsync_DeveRetornarNullQuandoNaoEncontrada()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync((EmpresaQueryResult?)null);

        var result = await _service.ObterUmAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaDeEmpresas()
    {
        var empresas = new[]
        {
            new EmpresaQueryResult { Id = Guid.NewGuid(), RazaoSocial = "Alpha", Dominio = "alpha" },
            new EmpresaQueryResult { Id = Guid.NewGuid(), RazaoSocial = "Beta", Dominio = "beta" }
        };

        _repositoryMock.Setup(x => x.ObterTodosAsync()).ReturnsAsync(empresas);

        var result = await _service.ObterTodosAsync();

        Assert.Equal(2, result.Length);
        Assert.Equal("Alpha", result[0].RazaoSocial);
        Assert.Equal("Beta", result[1].RazaoSocial);
    }
}
