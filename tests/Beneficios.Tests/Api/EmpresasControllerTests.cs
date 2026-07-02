using Beneficios.Api.Controllers;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beneficios.Tests.Api;

public class EmpresasControllerTests
{
    private readonly Mock<IEmpresaService> _serviceMock;
    private readonly Mock<ILogger<EmpresasController>> _loggerMock;
    private readonly EmpresasController _controller;

    public EmpresasControllerTests()
    {
        _serviceMock = new Mock<IEmpresaService>();
        _loggerMock = new Mock<ILogger<EmpresasController>>();
        _controller = new EmpresasController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComListaDeEmpresas()
    {
        var empresas = new[]
        {
            new EmpresaDto { Id = Guid.NewGuid(), RazaoSocial = "Empresa 1", Dominio = "empresa1" },
            new EmpresaDto { Id = Guid.NewGuid(), RazaoSocial = "Empresa 2", Dominio = "empresa2" }
        };

        _serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(empresas);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<EmpresaDto[]>(okResult.Value);
        Assert.Equal(2, returnValue.Length);
    }

    [Fact]
    public async Task GetById_DeveRetornarOkComEmpresa()
    {
        var id = Guid.NewGuid();
        var empresa = new EmpresaDto { Id = id, RazaoSocial = "Empresa Teste", Dominio = "empresateste" };

        _serviceMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(empresa);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<EmpresaDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFoundQuandoEmpresaNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((EmpresaDto?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContentQuandoSucesso()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFoundQuandoEmpresaNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
