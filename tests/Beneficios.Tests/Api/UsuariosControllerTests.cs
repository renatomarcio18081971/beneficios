using Beneficios.Api.Controllers;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beneficios.Tests.Api;

public class UsuariosControllerTests
{
    private readonly Mock<IUsuarioService> _serviceMock;
    private readonly Mock<ILogger<UsuariosController>> _loggerMock;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _serviceMock = new Mock<IUsuarioService>();
        _loggerMock = new Mock<ILogger<UsuariosController>>();
        _controller = new UsuariosController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComListaDeUsuarios()
    {
        var usuarios = new[]
        {
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "João", Email = "joao@example.com" },
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "Maria", Email = "maria@example.com" }
        };

        _serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(usuarios);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<UsuarioDto[]>(okResult.Value);
        Assert.Equal(2, returnValue.Length);
    }

    [Fact]
    public async Task GetById_DeveRetornarOkComUsuario()
    {
        var id = Guid.NewGuid();
        var usuario = new UsuarioDto { Id = id, Nome = "João", Email = "joao@example.com" };

        _serviceMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(usuario);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<UsuarioDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFoundQuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((UsuarioDto?)null);

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
    public async Task Delete_DeveRetornarNotFoundQuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
