using Beneficios.Api.Controllers;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Beneficios.Tests.Api;

public class PerfisControllerTests
{
    private readonly Mock<IPerfilService> _serviceMock;
    private readonly Mock<ILogger<PerfisController>> _loggerMock;
    private readonly PerfisController _controller;

    public PerfisControllerTests()
    {
        _serviceMock = new Mock<IPerfilService>();
        _loggerMock = new Mock<ILogger<PerfisController>>();
        _controller = new PerfisController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComLista()
    {
        var perfis = new[]
        {
            new PerfilDto { Id = Guid.NewGuid(), Nome = "Dono", EhSistema = true },
            new PerfilDto { Id = Guid.NewGuid(), Nome = "Operador" },
        };
        _serviceMock.Setup(x => x.ObterTodosAsync()).ReturnsAsync(perfis);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<PerfilDto[]>(ok.Value);
        Assert.Equal(2, value.Length);
    }

    [Fact]
    public async Task Create_DeveRetornarCreated()
    {
        var id = Guid.NewGuid();
        var dto = new PerfilSalvarDto("Operador", []);
        _serviceMock.Setup(x => x.SalvarAsync(dto)).ReturnsAsync(id);

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(id, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Delete_PerfilSistema_DeveRetornarBadRequest()
    {
        var id = Guid.NewGuid();
        _serviceMock
            .Setup(x => x.DeleteAsync(id))
            .ThrowsAsync(new InvalidOperationException("Perfil de sistema não pode ser excluído."));

        var result = await _controller.Delete(id);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(bad.Value);
    }
}
