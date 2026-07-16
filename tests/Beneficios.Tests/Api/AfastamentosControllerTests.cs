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

public class AfastamentosControllerTests
{
    private readonly Mock<IFuncionarioAfastamentoService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private AfastamentosController CreateController()
    {
        var controller = new AfastamentosController(
            _service.Object,
            _permissao.Object,
            NullLogger<AfastamentosController>.Instance);

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
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Visualizar))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Filtrar(null, null, null, null);
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Filtrar_Valido_DeveRetornarOk()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.FiltrarAsync(null, null, null, null))
            .ReturnsAsync(Array.Empty<AfastamentoDto>());

        var result = await CreateController().Filtrar(null, null, null, null);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Criar_Valido_DeveRetornarCreated()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<AfastamentoSalvarDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(id);

        var result = await CreateController().Criar(new AfastamentoSalvarDto(
            Guid.NewGuid(), TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), null, null));

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AfastamentosController.ObterPorId), created.ActionName);
    }

    [Fact]
    public async Task Criar_Sobreposicao_DeveRetornarBadRequest()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<AfastamentoSalvarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Já existe um afastamento neste período para o funcionário."));

        var result = await CreateController().Criar(new AfastamentoSalvarDto(
            Guid.NewGuid(), TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), null, null));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((AfastamentoDto?)null);

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_Valido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Visualizar))
            .Returns(Task.CompletedTask);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new AfastamentoDto(
            id, Guid.NewGuid(), "Ana", TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), null, null));

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_Valido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Atualizar(id, new AfastamentoAtualizarDto(
            TipoAfastamento.Ferias, new DateOnly(2026, 1, 1), null, null));
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Editar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<AfastamentoAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Afastamento não encontrado."));

        var result = await CreateController().Atualizar(id, new AfastamentoAtualizarDto(
            TipoAfastamento.Ferias, new DateOnly(2026, 1, 1), null, null));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Excluir_DeveChamarService()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Excluir))
            .Returns(Task.CompletedTask);

        var result = await CreateController().Excluir(id);
        Assert.IsType<NoContentResult>(result);
        _service.Verify(x => x.ExcluirAsync(id), Times.Once);
    }

    [Fact]
    public async Task Criar_FuncionarioNaoEncontrado_DeveRetornar404()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Criar))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<AfastamentoSalvarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("Funcionário não encontrado."));

        var result = await CreateController().Criar(new AfastamentoSalvarDto(
            Guid.NewGuid(), TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), null, null));
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Excluir_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Excluir))
            .Returns(Task.CompletedTask);
        _service
            .Setup(x => x.ExcluirAsync(id))
            .ThrowsAsync(new InvalidOperationException("Afastamento não encontrado."));

        var result = await CreateController().Excluir(id);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Excluir_SemPermissao_DeveRetornar403()
    {
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "afastamentos", AcaoPermissao.Excluir))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

        var result = await CreateController().Excluir(Guid.NewGuid());
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }
}