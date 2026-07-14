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

public class DiasUteisControllerTests
{
    private readonly Mock<ICalendarioDiaService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private DiasUteisController CreateController(bool adminTenant = false)
    {
        var controller = new DiasUteisController(
            _service.Object,
            _permissao.Object,
            NullLogger<DiasUteisController>.Instance);

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
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "dias_uteis", AcaoPermissao.Criar))
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
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "dias_uteis", AcaoPermissao.Visualizar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().ObterPorMes(2026, 3);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }
}
