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

        _serviceMock.Setup(x => x.ObterTodosAsync()).ReturnsAsync(empresas);

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

        _serviceMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(empresa);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<EmpresaDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFoundQuandoEmpresaNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync((EmpresaDto?)null);

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

    [Fact]
    public async Task GetById_DeveRetornar500QuandoExcecao()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.ObterUmAsync(id)).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.GetById(id);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_DeveRetornar500QuandoExcecao()
    {
        _serviceMock.Setup(x => x.ObterTodosAsync()).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.GetAll();

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Delete_DeveRetornar500QuandoExcecao()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(id)).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.Delete(id);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Create_DeveRetornarCreatedQuandoSucesso()
    {
        var dto = new EmpresaSalvarDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.SalvarAsync(dto)).ReturnsAsync(id);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EmpresasController.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Create_DeveRetornar500QuandoExcecao()
    {
        var dto = new EmpresaSalvarDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        _serviceMock.Setup(x => x.SalvarAsync(dto)).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.Create(dto);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Update_DeveRetornarNoContentQuandoSucesso()
    {
        var id = Guid.NewGuid();
        var usuarioAlteracaoId = Guid.NewGuid();
        var dto = new EmpresaAtualizarDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        ControllerTestHelper.SetAuthenticatedUser(_controller, usuarioAlteracaoId);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, usuarioAlteracaoId)).ReturnsAsync(true);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_DeveRetornarNotFoundQuandoEmpresaNaoExiste()
    {
        var id = Guid.NewGuid();
        var dto = new EmpresaAtualizarDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        ControllerTestHelper.SetAuthenticatedUser(_controller);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, It.IsAny<Guid?>())).ReturnsAsync(false);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_DeveRetornar500QuandoExcecao()
    {
        var id = Guid.NewGuid();
        var dto = new EmpresaAtualizarDto("Empresa Teste LTDA", "empresateste", "dbname", "dbuser", "dbpass");

        ControllerTestHelper.SetAuthenticatedUser(_controller);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, It.IsAny<Guid?>())).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.Update(id, dto);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
