using Beneficios.Api.Controllers;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Beneficios.Tests.Api;

public class LinhasOnibusControllerTests
{
    private readonly Mock<ILinhaOnibusService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private LinhasOnibusController CreateController()
    {
        var controller = new LinhasOnibusController(
            _service.Object,
            _permissao.Object,
            NullLogger<LinhasOnibusController>.Instance);

        var http = new DefaultHttpContext();
        http.Request.Headers["X-Tenant"] = "empresa1";
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
        ], "test"));
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return controller;
    }

    [Fact]
    public async Task Filtrar_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Filtrar(null, null);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Filtrar_Valido_DeveRetornarOk()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.FiltrarAsync(null, null))
            .ReturnsAsync(Array.Empty<LinhaOnibusDto>());

        var result = await CreateController().Filtrar(null, null);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Criar_Valido_DeveRetornarCreated()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<LinhaOnibusSalvarDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(id);

        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(LinhasOnibusController.ObterPorId), created.ActionName);
    }

    [Fact]
    public async Task Criar_TarifaInvalida_DeveRetornarBadRequest()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<LinhaOnibusSalvarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("O valor da tarifa deve ser maior ou igual a zero."));

        var result = await CreateController().Criar(new LinhaOnibusSalvarDto(
            "Linha 100", new DateOnly(2026, 1, 1), null, -1m));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusDto?)null);

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_Valido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new LinhaOnibusDto(
            id, "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_Valido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<LinhaOnibusAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Linha não encontrada."));

        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_VinculosAbertos_DeveRetornarBadRequest()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<LinhaOnibusAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException(
                "Existem vínculos em aberto para esta linha; encerre-os antes de finalizar a vigência."));

        var result = await CreateController().Atualizar(id, new LinhaOnibusAtualizarDto(
            "Linha 100", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), 4.50m));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "linhas_onibus", AcaoPermissao.Editar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Atualizar(Guid.NewGuid(), new LinhaOnibusAtualizarDto(
            "Linha 100", new DateOnly(2026, 1, 1), null, 4.50m));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }
}
