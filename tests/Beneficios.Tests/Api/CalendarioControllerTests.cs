using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Api.Controllers;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Beneficios.Tests.Api;

public class CalendarioControllerTests
{
    private readonly Mock<ICalendarioDiaService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private CalendarioController CreateController(bool adminTenant = false)
    {
        var controller = new CalendarioController(
            _service.Object,
            _permissao.Object,
            NullLogger<CalendarioController>.Instance);

        var http = new DefaultHttpContext();
        http.Request.Headers["X-Tenant"] = adminTenant ? "admin" : "empresa1";
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
        ], "test"));
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return controller;
    }

    [Fact]
    public async Task GerarAno_QuandoJaExiste_DeveRetornarConflict()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.GerarAnoAsync(2026))
            .ThrowsAsync(new InvalidOperationException("Este ano já está cadastrado."));

        var result = await CreateController().GerarAno(new GerarAnoCalendarioDto(2026));
        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task ObterPorMes_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().ObterPorMes(2026, 3);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task ObterPorMes_Valido_DeveRetornarOk()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorMesAsync(2026, 7)).ReturnsAsync([]);

        var result = await CreateController().ObterPorMes(2026, 7);
        Assert.IsType<OkObjectResult>(result);
        _permissao.Verify(
            x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar),
            Times.Once);
    }

    [Theory]
    [InlineData(1899, 1)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public async Task ObterPorMes_AnoMesInvalido_DeveRetornarBadRequest(int ano, int mes)
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().ObterPorMes(ano, mes);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((CalendarioDiaDto?)null);

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_Valido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new CalendarioDiaDto(
            id, new DateOnly(2026, 7, 1), true, null, OrigemCalendarioDia.Geracao, null));

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_Valido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Atualizar(id, new CalendarioDiaAtualizarDto(true, null, null));
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<CalendarioDiaAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Dia não encontrado."));

        var result = await CreateController().Atualizar(id, new CalendarioDiaAtualizarDto(true, null, null));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GerarAno_Valido_DeveRetornarOk()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().GerarAno(new GerarAnoCalendarioDto(2027));
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GerarAno_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "calendario", AcaoPermissao.Criar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().GerarAno(new GerarAnoCalendarioDto(2027));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }
}
