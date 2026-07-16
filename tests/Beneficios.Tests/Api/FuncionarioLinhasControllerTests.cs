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

public class FuncionarioLinhasControllerTests
{
    private readonly Mock<IFuncionarioLinhaService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private FuncionarioLinhasController CreateController()
    {
        var controller = new FuncionarioLinhasController(
            _service.Object,
            _permissao.Object,
            NullLogger<FuncionarioLinhasController>.Instance);

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
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Filtrar(null, null);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Filtrar_Valido_DeveRetornarOk()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.FiltrarAsync(null, null))
            .ReturnsAsync(Array.Empty<FuncionarioLinhaDto>());

        var result = await CreateController().Filtrar(null, null);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Criar_Valido_DeveRetornarCreated()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(id);

        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FuncionarioLinhasController.ObterPorId), created.ActionName);
    }

    [Fact]
    public async Task Criar_VtInativo_DeveRetornarBadRequest()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException(
                "Funcionário sem vale transporte ativo; não é possível vincular linhas."));

        var result = await CreateController().Criar(new FuncionarioLinhaSalvarDto(
            Guid.NewGuid(), Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaDto?)null);

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_Valido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        var funcionarioId = Guid.NewGuid();
        var linhaId = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new FuncionarioLinhaDto(
            id, funcionarioId, "Ana", linhaId, "Linha 100", 1, new DateOnly(2026, 1, 1), null));

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_Valido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Atualizar(id, new FuncionarioLinhaAtualizarDto(
            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<FuncionarioLinhaAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Vínculo não encontrado."));

        var result = await CreateController().Atualizar(id, new FuncionarioLinhaAtualizarDto(
            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionario_linhas", AcaoPermissao.Editar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Atualizar(Guid.NewGuid(), new FuncionarioLinhaAtualizarDto(
            Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), null));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }
}
