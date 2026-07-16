using Beneficios.Api.Controllers;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Beneficios.Tests.Api;

public class FuncionariosControllerTests
{
    private readonly Mock<IFuncionarioService> _service = new();
    private readonly Mock<IPermissaoService> _permissao = new();

    private FuncionariosController CreateController()
    {
        var controller = new FuncionariosController(
            _service.Object,
            _permissao.Object,
            NullLogger<FuncionariosController>.Instance);

        var http = new DefaultHttpContext();
        http.Request.Headers["X-Tenant"] = "empresa1";
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
        ], "test"));
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return controller;
    }

    private void Permitir(AcaoPermissao acao) =>
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionarios", acao))
            .Returns(Task.CompletedTask);

    private void Negar(AcaoPermissao acao) =>
        _permissao
            .Setup(x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionarios", acao))
            .ThrowsAsync(new UnauthorizedAccessException("Sem permissão para esta operação."));

    [Fact]
    public async Task Filtrar_SemPermissao_DeveRetornar403()
    {
        Negar(AcaoPermissao.Visualizar);
        var result = await CreateController().Filtrar(new FuncionarioFiltroDto(null, null, null, null));
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Filtrar_Valido_DeveRetornarOk()
    {
        Permitir(AcaoPermissao.Visualizar);
        _service
            .Setup(x => x.FiltrarAsync(It.IsAny<FuncionarioFiltroDto>()))
            .ReturnsAsync(Array.Empty<FuncionarioDto>());

        var result = await CreateController().Filtrar(new FuncionarioFiltroDto(null, null, null, null));
        Assert.IsType<OkObjectResult>(result);
        _permissao.Verify(
            x => x.GarantirPermissaoAsync(It.IsAny<Guid>(), "funcionarios", AcaoPermissao.Visualizar),
            Times.Once);
    }

    [Fact]
    public async Task ObterPorId_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Visualizar);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioDto?)null);

        var result = await CreateController().ObterPorId(id);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(FuncionarioService.MensagemNaoEncontrado, notFound.Value!.ToString());
    }

    [Fact]
    public async Task ObterPorId_Valido_DeveRetornarOk()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Visualizar);
        _service.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(CriarDto(id));

        var result = await CreateController().ObterPorId(id);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ObterPorId_SemPermissao_DeveRetornar403()
    {
        Negar(AcaoPermissao.Visualizar);
        var result = await CreateController().ObterPorId(Guid.NewGuid());
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Criar_Valido_DeveRetornarCreated()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Criar);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioSalvarDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(id);

        var result = await CreateController().Criar(CriarSalvarDto());
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FuncionariosController.ObterPorId), created.ActionName);
    }

    [Fact]
    public async Task Criar_Invalido_DeveRetornarBadRequest()
    {
        Permitir(AcaoPermissao.Criar);
        _service
            .Setup(x => x.SalvarAsync(It.IsAny<FuncionarioSalvarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException(FuncionarioService.MensagemCpfDuplicado));

        var result = await CreateController().Criar(CriarSalvarDto());
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Criar_SemPermissao_DeveRetornar403()
    {
        Negar(AcaoPermissao.Criar);
        var result = await CreateController().Criar(CriarSalvarDto());
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, obj.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Valido_DeveRetornarNoContent()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Editar);
        var result = await CreateController().Atualizar(id, CriarAtualizarDto());
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Atualizar_NaoEncontrado_DeveRetornar404()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Editar);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<FuncionarioAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException(FuncionarioService.MensagemNaoEncontrado));

        var result = await CreateController().Atualizar(id, CriarAtualizarDto());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Atualizar_RegraNegocio_DeveRetornarBadRequest()
    {
        var id = Guid.NewGuid();
        Permitir(AcaoPermissao.Editar);
        _service
            .Setup(x => x.AtualizarAsync(id, It.IsAny<FuncionarioAtualizarDto>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException(FuncionarioService.MensagemSituacaoInvalida));

        var result = await CreateController().Atualizar(id, CriarAtualizarDto());
        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static FuncionarioDto CriarDto(Guid id) => new(
        id, "Ana", "52998224725", null, new DateOnly(2024, 1, 1), null,
        "Analista", 3500m, TipoContrato.Clt, null,
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null,
        SituacaoFuncionario.Ativo, null, JornadaTrabalho.QuarentaHorasSegSex, null,
        DateTime.UtcNow, null, Array.Empty<FuncionarioBeneficioDto>());

    private static FuncionarioSalvarDto CriarSalvarDto() => new(
        "Ana", "529.982.247-25", null, new DateOnly(2024, 1, 1), null,
        "Analista", 3500m, TipoContrato.Clt, null,
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null,
        SituacaoFuncionario.Ativo, JornadaTrabalho.QuarentaHorasSegSex, null, null);

    private static FuncionarioAtualizarDto CriarAtualizarDto() => new(
        "Ana", "529.982.247-25", null, new DateOnly(2024, 1, 1), null,
        "Analista", 3500m, TipoContrato.Clt, null,
        null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null,
        SituacaoFuncionario.Ativo, JornadaTrabalho.QuarentaHorasSegSex, null, null);
}
