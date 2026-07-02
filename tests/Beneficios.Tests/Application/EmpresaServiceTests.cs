using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class EmpresaServiceTests
{
    private readonly Mock<IEmpresaRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly EmpresaService _service;

    public EmpresaServiceTests()
    {
        _repositoryMock = new Mock<IEmpresaRepository>();
        _mapperMock = new Mock<IMapper>();
        _service = new EmpresaService(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DeveCriarEmpresaComSucesso()
    {
        var dto = new EmpresaCreateDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<EmpresaCreateParams>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<EmpresaCreateParams>(p =>
            p.RazaoSocial == dto.RazaoSocial && p.Dominio == dto.Dominio)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarEmpresaComSucesso()
    {
        var id = Guid.NewGuid();
        var dto = new EmpresaUpdateDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<EmpresaUpdateParams>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(id, dto, null);

        Assert.True(result);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<EmpresaUpdateParams>(p =>
            p.Id == id && p.RazaoSocial == dto.RazaoSocial)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveDeletarEmpresaComSucesso()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }
}
