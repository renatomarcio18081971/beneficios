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
    private readonly Mock<IPermissaoService> _permissaoMock;
    private readonly Mock<ILogger<UsuariosController>> _loggerMock;
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _serviceMock = new Mock<IUsuarioService>();
        _permissaoMock = new Mock<IPermissaoService>();
        _loggerMock = new Mock<ILogger<UsuariosController>>();
        _controller = new UsuariosController(
            _serviceMock.Object,
            _permissaoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_DeveRetornarOkComListaDeUsuarios()
    {
        var usuarios = new[]
        {
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "João", Email = "joao@example.com" },
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "Maria", Email = "maria@example.com" }
        };

        _serviceMock.Setup(x => x.ObterTodosAsync()).ReturnsAsync(usuarios);

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

        _serviceMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(usuario);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<UsuarioDto>(okResult.Value);
        Assert.Equal(id, returnValue.Id);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFoundQuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync((UsuarioDto?)null);

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
        var dto = new UsuarioSalvarDto("Joao Silva", "senha123", "joao@example.com", Guid.NewGuid(), Guid.NewGuid());
        var id = Guid.NewGuid();

        _serviceMock.Setup(x => x.SalvarAsync(dto)).ReturnsAsync(id);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsuariosController.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Create_DeveRetornar500QuandoExcecao()
    {
        var dto = new UsuarioSalvarDto("Joao Silva", "senha123", "joao@example.com", Guid.NewGuid(), Guid.NewGuid());

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
        var dto = new UsuarioAtualizarDto("Joao Silva", "joao@example.com", Guid.NewGuid(), Guid.NewGuid());

        ControllerTestHelper.SetAuthenticatedUser(_controller, usuarioAlteracaoId);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, usuarioAlteracaoId)).ReturnsAsync(true);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_DeveRetornarNotFoundQuandoUsuarioNaoExiste()
    {
        var id = Guid.NewGuid();
        var dto = new UsuarioAtualizarDto("Joao Silva", "joao@example.com", Guid.NewGuid(), Guid.NewGuid());

        ControllerTestHelper.SetAuthenticatedUser(_controller);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, It.IsAny<Guid?>())).ReturnsAsync(false);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_DeveRetornar500QuandoExcecao()
    {
        var id = Guid.NewGuid();
        var dto = new UsuarioAtualizarDto("Joao Silva", "joao@example.com", Guid.NewGuid(), Guid.NewGuid());

        ControllerTestHelper.SetAuthenticatedUser(_controller);
        _serviceMock.Setup(x => x.AtualizarAsync(id, dto, It.IsAny<Guid?>())).ThrowsAsync(new Exception("Erro"));

        var result = await _controller.Update(id, dto);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Login_DeveRetornarOkQuandoCredenciaisValidas()
    {
        var loginDto = new LoginDto("joao@example.com", "senha123");
        var response = new LoginResponseDto
        {
            Token = "jwt-token",
            UsuarioId = Guid.NewGuid(),
            Nome = "Joao",
            Email = "joao@example.com"
        };

        _serviceMock.Setup(x => x.LoginAsync(loginDto, "admin")).ReturnsAsync(response);
        ControllerTestHelper.SetRequestHost(_controller, "admin.localhost:5000");

        var result = await _controller.Login(loginDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.Equal("jwt-token", returnValue.Token);
    }

    [Fact]
    public async Task Login_DeveRetornarUnauthorizedQuandoCredenciaisInvalidas()
    {
        var loginDto = new LoginDto("joao@example.com", "senhaErrada");

        _serviceMock.Setup(x => x.LoginAsync(loginDto, "admin")).ReturnsAsync((LoginResponseDto?)null);
        ControllerTestHelper.SetRequestHost(_controller, "admin.localhost:5000");

        var result = await _controller.Login(loginDto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_DeveRetornar500QuandoExcecao()
    {
        var loginDto = new LoginDto("joao@example.com", "senha123");

        _serviceMock.Setup(x => x.LoginAsync(loginDto, "admin")).ThrowsAsync(new Exception("Erro"));
        ControllerTestHelper.SetRequestHost(_controller, "admin.localhost:5000");

        var result = await _controller.Login(loginDto);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Login_DeveUsarHeaderXTenantQuandoInformado()
    {
        var loginDto = new LoginDto("joao@example.com", "senha123");
        var response = new LoginResponseDto { Token = "jwt-token", Email = loginDto.Email };

        _serviceMock.Setup(x => x.LoginAsync(loginDto, "empresa1")).ReturnsAsync(response);
        ControllerTestHelper.SetRequestHost(_controller, "localhost:5000", "empresa1");

        var result = await _controller.Login(loginDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("jwt-token", Assert.IsType<LoginResponseDto>(okResult.Value).Token);
        _serviceMock.Verify(x => x.LoginAsync(loginDto, "empresa1"), Times.Once);
    }

    [Fact]
    public async Task SolicitarAlteracaoSenha_DeveRetornarNotFoundQuandoEmailNaoExiste()
    {
        var dto = new SolicitarAlteracaoSenhaDto("inexistente@example.com");

        _serviceMock.Setup(x => x.EmailExisteAsync(dto.Email, "empresa1")).ReturnsAsync(false);
        ControllerTestHelper.SetRequestHost(_controller, "localhost:5000", "empresa1");

        var result = await _controller.SolicitarAlteracaoSenha(dto);

        Assert.IsType<NotFoundObjectResult>(result);
        _serviceMock.Verify(x => x.SolicitarAlteracaoSenhaAsync(dto, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarAlteracaoSenha_DeveRetornarOkQuandoEmailExiste()
    {
        var dto = new SolicitarAlteracaoSenhaDto("joao@example.com");

        _serviceMock.Setup(x => x.EmailExisteAsync(dto.Email, "empresa1")).ReturnsAsync(true);
        _serviceMock.Setup(x => x.SolicitarAlteracaoSenhaAsync(dto, "empresa1")).Returns(Task.CompletedTask);
        ControllerTestHelper.SetRequestHost(_controller, "localhost:5000", "empresa1");

        var result = await _controller.SolicitarAlteracaoSenha(dto);

        Assert.IsType<OkResult>(result);
        _serviceMock.Verify(x => x.SolicitarAlteracaoSenhaAsync(dto, "empresa1"), Times.Once);
    }

    [Fact]
    public async Task AlterarSenha_DeveRetornarOk()
    {
        var dto = new AlterarSenhaDto("123456", "novaSenha123", "novaSenha123");

        _serviceMock.Setup(x => x.AlterarSenhaAsync(dto, "empresa1")).ReturnsAsync(true);
        ControllerTestHelper.SetRequestHost(_controller, "localhost:5000", "empresa1");

        var result = await _controller.AlterarSenha(dto);

        Assert.IsType<OkResult>(result);
        _serviceMock.Verify(x => x.AlterarSenhaAsync(dto, "empresa1"), Times.Once);
    }

    [Fact]
    public async Task AlterarSenha_DeveRetornarBadRequestQuandoCodigoInvalido()
    {
        var dto = new AlterarSenhaDto("999999", "novaSenha123", "novaSenha123");

        _serviceMock.Setup(x => x.AlterarSenhaAsync(dto, "empresa1")).ReturnsAsync(false);
        ControllerTestHelper.SetRequestHost(_controller, "localhost:5000", "empresa1");

        var result = await _controller.AlterarSenha(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Filtrar_DeveRetornarOkComFiltroPorNome()
    {
        var usuarios = new[]
        {
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "João", Email = "joao@example.com" },
        };

        _serviceMock.Setup(x => x.FiltrarAsync(It.Is<UsuarioFiltroDto>(f => f.Nome == "João" && f.Email == null)))
            .ReturnsAsync(usuarios);

        var result = await _controller.Filtrar("João", null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsAssignableFrom<UsuarioDto[]>(okResult.Value));
    }

    [Fact]
    public async Task Filtrar_DeveRetornarOkComFiltroPorEmail()
    {
        var usuarios = new[]
        {
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "João", Email = "joao@example.com" },
        };

        _serviceMock.Setup(x => x.FiltrarAsync(It.Is<UsuarioFiltroDto>(f => f.Nome == null && f.Email == "joao")))
            .ReturnsAsync(usuarios);

        var result = await _controller.Filtrar(null, "joao");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsAssignableFrom<UsuarioDto[]>(okResult.Value));
    }

    [Fact]
    public async Task Filtrar_DeveRetornarOkComFiltroCombinado()
    {
        var usuarios = new[]
        {
            new UsuarioDto { Id = Guid.NewGuid(), Nome = "João Silva", Email = "joao@example.com" },
        };

        _serviceMock.Setup(x => x.FiltrarAsync(It.Is<UsuarioFiltroDto>(f => f.Nome == "João" && f.Email == "joao")))
            .ReturnsAsync(usuarios);

        var result = await _controller.Filtrar("João", "joao");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Single(Assert.IsAssignableFrom<UsuarioDto[]>(okResult.Value));
    }

    [Fact]
    public async Task Filtrar_DeveRetornarListaVaziaQuandoNenhumResultado()
    {
        _serviceMock.Setup(x => x.FiltrarAsync(It.IsAny<UsuarioFiltroDto>()))
            .ReturnsAsync(Array.Empty<UsuarioDto>());

        var result = await _controller.Filtrar("Inexistente", null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<UsuarioDto[]>(okResult.Value));
    }

    [Fact]
    public async Task Filtrar_DeveRetornar500QuandoExcecao()
    {
        _serviceMock.Setup(x => x.FiltrarAsync(It.IsAny<UsuarioFiltroDto>()))
            .ThrowsAsync(new Exception("Erro"));

        var result = await _controller.Filtrar("João", null);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
