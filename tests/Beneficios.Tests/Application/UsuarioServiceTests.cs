using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IMapper _mapper;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<UsuarioProfile>()).CreateMapper();
        _service = new UsuarioService(
            _repositoryMock.Object,
            _mapper,
            _tokenServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task SalvarAsync_DeveCriarUsuarioComSucesso()
    {
        var dto = new UsuarioSalvarDto("Joao Silva", "senha123", "joao@example.com", Guid.NewGuid());

        _repositoryMock.Setup(x => x.SalvarAsync(It.IsAny<UsuarioSalvarParams>()))
            .ReturnsAsync((UsuarioSalvarParams p) => p.Id);

        var result = await _service.SalvarAsync(dto);

        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(x => x.SalvarAsync(It.Is<UsuarioSalvarParams>(p =>
            p.Nome == dto.Nome && p.Email == dto.Email && p.EmpresaId == dto.EmpresaId)), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarUsuarioComSucesso()
    {
        var id = Guid.NewGuid();
        var dto = new UsuarioAtualizarDto("Joao Silva", "joao@example.com", Guid.NewGuid());

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new UsuarioQueryResult
        {
            Id = id,
            Email = "joao@example.com",
        });
        _repositoryMock.Setup(x => x.AtualizarAsync(It.IsAny<UsuarioAtualizarParams>()))
            .ReturnsAsync(true);

        var result = await _service.AtualizarAsync(id, dto, null);

        Assert.True(result);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.Is<UsuarioAtualizarParams>(p =>
            p.Id == id && p.Nome == dto.Nome && p.Email == dto.Email)), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_UsuarioPadrao_NaoDeveAtualizar()
    {
        var id = Guid.NewGuid();
        var dto = new UsuarioAtualizarDto("Outro Nome", "outro@example.com", Guid.NewGuid());

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new UsuarioQueryResult
        {
            Id = id,
            Email = TenantDefaultUser.Email,
        });

        var result = await _service.AtualizarAsync(id, dto, null);

        Assert.False(result);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<UsuarioAtualizarParams>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeveDeletarUsuarioComSucesso()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new UsuarioQueryResult
        {
            Id = id,
            Email = "joao@example.com",
        });
        _repositoryMock.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UsuarioPadrao_NaoDeveExcluir()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new UsuarioQueryResult
        {
            Id = id,
            Email = TenantDefaultUser.Email,
        });

        var result = await _service.DeleteAsync(id);

        Assert.False(result);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ObterUmAsync_DeveRetornarUsuarioQuandoEncontrado()
    {
        var id = Guid.NewGuid();
        var queryResult = new UsuarioQueryResult
        {
            Id = id,
            Nome = "Joao",
            Email = "joao@example.com",
            EmpresaId = Guid.NewGuid(),
            EmpresaNome = "Empresa Teste"
        };

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(queryResult);

        var result = await _service.ObterUmAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Joao", result.Nome);
        Assert.Equal("Empresa Teste", result.EmpresaNome);
    }

    [Fact]
    public async Task ObterUmAsync_DeveRetornarNullQuandoNaoEncontrado()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync((UsuarioQueryResult?)null);

        var result = await _service.ObterUmAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaDeUsuarios()
    {
        var usuarios = new[]
        {
            new UsuarioQueryResult { Id = Guid.NewGuid(), Nome = "Joao", Email = "joao@example.com" },
            new UsuarioQueryResult { Id = Guid.NewGuid(), Nome = "Maria", Email = "maria@example.com" }
        };

        _repositoryMock.Setup(x => x.ObterTodosAsync()).ReturnsAsync(usuarios);

        var result = await _service.ObterTodosAsync();

        Assert.Equal(2, result.Length);
        Assert.Equal("Joao", result[0].Nome);
        Assert.Equal("Maria", result[1].Nome);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarTokenQuandoCredenciaisValidas()
    {
        var usuarioId = Guid.NewGuid();
        var authResult = new UsuarioAuthResult
        {
            Id = usuarioId,
            Nome = "Joao",
            Email = "joao@example.com",
            Senha = Criptografia.Encrypt("senha123"),
            EmpresaId = Guid.NewGuid(),
            Perfil = UsuarioPerfil.Empresa,
            EmpresaDominio = "exemplo"
        };

        _repositoryMock.Setup(x => x.GetByEmailAsync("joao@example.com")).ReturnsAsync(authResult);
        _tokenServiceMock.Setup(x => x.GenerateToken(usuarioId, "joao@example.com", UsuarioPerfil.Empresa, authResult.EmpresaId)).Returns("jwt-token");
        _repositoryMock.Setup(x => x.UpdateTokenAsync(usuarioId, "jwt-token")).ReturnsAsync(true);

        var result = await _service.LoginAsync(new LoginDto("joao@example.com", "senha123"), "exemplo");

        Assert.NotNull(result);
        Assert.Equal("jwt-token", result!.Token);
        Assert.Equal(usuarioId, result.UsuarioId);
        Assert.Equal("Joao", result.Nome);
        _repositoryMock.Verify(x => x.UpdateTokenAsync(usuarioId, "jwt-token"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_EmAdmin_ComPerfilEmpresa_DeveRetornarNull()
    {
        var authResult = new UsuarioAuthResult
        {
            Id = Guid.NewGuid(),
            Email = "u@test.com",
            Senha = Criptografia.Encrypt("senha123"),
            Perfil = UsuarioPerfil.Empresa,
            EmpresaDominio = "exemplo"
        };
        _repositoryMock.Setup(x => x.GetByEmailAsync("u@test.com")).ReturnsAsync(authResult);

        var result = await _service.LoginAsync(new LoginDto("u@test.com", "senha123"), "admin");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_EmTenant_ComDominioDiferente_DeveRetornarNull()
    {
        var authResult = new UsuarioAuthResult
        {
            Id = Guid.NewGuid(),
            Email = "u@test.com",
            Senha = Criptografia.Encrypt("senha123"),
            Perfil = UsuarioPerfil.Empresa,
            EmpresaDominio = "exemplo"
        };
        _repositoryMock.Setup(x => x.GetByEmailAsync("u@test.com")).ReturnsAsync(authResult);

        var result = await _service.LoginAsync(new LoginDto("u@test.com", "senha123"), "outra");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_EmAdmin_ComPerfilAdmin_DeveRetornarToken()
    {
        var authResult = new UsuarioAuthResult
        {
            Id = Guid.NewGuid(),
            Nome = "Admin",
            Email = "admin@test.com",
            Senha = Criptografia.Encrypt("admin123"),
            Perfil = UsuarioPerfil.Admin
        };
        _repositoryMock.Setup(x => x.GetByEmailAsync("admin@test.com")).ReturnsAsync(authResult);
        _tokenServiceMock.Setup(x => x.GenerateToken(authResult.Id, authResult.Email, UsuarioPerfil.Admin, null))
            .Returns("jwt-token");
        _repositoryMock.Setup(x => x.UpdateTokenAsync(authResult.Id, "jwt-token")).ReturnsAsync(true);

        var result = await _service.LoginAsync(new LoginDto("admin@test.com", "admin123"), "admin");

        Assert.NotNull(result);
        Assert.Equal("jwt-token", result!.Token);
        Assert.Null(result.EmpresaId);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarNullQuandoUsuarioNaoExiste()
    {
        _repositoryMock.Setup(x => x.GetByEmailAsync("inexistente@example.com"))
            .ReturnsAsync((UsuarioAuthResult?)null);

        var result = await _service.LoginAsync(new LoginDto("inexistente@example.com", "senha123"), "exemplo");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarNullQuandoSenhaInvalida()
    {
        var authResult = new UsuarioAuthResult
        {
            Id = Guid.NewGuid(),
            Email = "joao@example.com",
            Senha = Criptografia.Encrypt("senha123")
        };

        _repositoryMock.Setup(x => x.GetByEmailAsync("joao@example.com")).ReturnsAsync(authResult);

        var result = await _service.LoginAsync(new LoginDto("joao@example.com", "senhaErrada"), "exemplo");

        Assert.Null(result);
        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<UsuarioPerfil>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveRetornarTrueQuandoUsuarioExisteNoTenant()
    {
        _repositoryMock.Setup(x => x.GetByEmailAsync("admin@example.com"))
            .ReturnsAsync(new UsuarioAuthResult { Email = "admin@example.com", Perfil = UsuarioPerfil.Admin });

        var result = await _service.EmailExisteAsync("admin@example.com", "admin");

        Assert.True(result);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveRetornarFalseQuandoUsuarioDeOutroTenant()
    {
        _repositoryMock.Setup(x => x.GetByEmailAsync("joao@example.com"))
            .ReturnsAsync(new UsuarioAuthResult
            {
                Email = "joao@example.com",
                Perfil = UsuarioPerfil.Empresa,
                EmpresaDominio = "empresa1",
            });

        var result = await _service.EmailExisteAsync("joao@example.com", "empresa2");

        Assert.False(result);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveRetornarFalseQuandoUsuarioNaoExiste()
    {
        _repositoryMock.Setup(x => x.GetByEmailAsync("inexistente@example.com"))
            .ReturnsAsync((UsuarioAuthResult?)null);

        var result = await _service.EmailExisteAsync("inexistente@example.com", "admin");

        Assert.False(result);
    }

    [Fact]
    public async Task SolicitarAlteracaoSenhaAsync_DeveEnviarEmailQuandoUsuarioPertenceAoTenant()
    {
        var dto = new SolicitarAlteracaoSenhaDto("joao@example.com");
        var usuarioId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new UsuarioAuthResult
            {
                Id = usuarioId,
                Email = dto.Email,
                Perfil = UsuarioPerfil.Empresa,
                EmpresaDominio = "empresa1",
            });
        _repositoryMock.Setup(x => x.UpdateCodigoAlterarSenhaAsync(usuarioId, It.IsAny<string>()))
            .ReturnsAsync(true);

        await _service.SolicitarAlteracaoSenhaAsync(dto, "empresa1");

        _repositoryMock.Verify(
            x => x.UpdateCodigoAlterarSenhaAsync(
                usuarioId,
                It.Is<string>(c => c.Length == 6 && c.All(char.IsDigit))),
            Times.Once);
        _emailServiceMock.Verify(
            x => x.EnviarAsync(
                dto.Email,
                "Redefinição de senha",
                It.Is<string>(m => m.Contains("Olá, você solicitou redefinição de senha. Informe este código quando solicitado."))),
            Times.Once);
    }

    [Fact]
    public async Task SolicitarAlteracaoSenhaAsync_NaoDeveEnviarEmailQuandoUsuarioDeOutroTenant()
    {
        var dto = new SolicitarAlteracaoSenhaDto("joao@example.com");
        _repositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new UsuarioAuthResult
            {
                Email = dto.Email,
                Perfil = UsuarioPerfil.Empresa,
                EmpresaDominio = "empresa1",
            });

        await _service.SolicitarAlteracaoSenhaAsync(dto, "empresa2");

        _emailServiceMock.Verify(
            x => x.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _repositoryMock.Verify(
            x => x.UpdateCodigoAlterarSenhaAsync(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SolicitarAlteracaoSenhaAsync_NaoDeveEnviarEmailQuandoUsuarioNaoExiste()
    {
        var dto = new SolicitarAlteracaoSenhaDto("inexistente@example.com");
        _repositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((UsuarioAuthResult?)null);

        await _service.SolicitarAlteracaoSenhaAsync(dto, "empresa1");

        _emailServiceMock.Verify(
            x => x.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _repositoryMock.Verify(
            x => x.UpdateCodigoAlterarSenhaAsync(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveRetornarTrueQuandoCodigoValido()
    {
        var usuarioId = Guid.NewGuid();
        var dto = new AlterarSenhaDto("123456", "novaSenha123", "novaSenha123");
        _repositoryMock.Setup(x => x.GetByCodigoAlterarSenhaAsync(dto.Codigo))
            .ReturnsAsync(new UsuarioAuthResult
            {
                Id = usuarioId,
                Perfil = UsuarioPerfil.Admin,
            });
        _repositoryMock.Setup(x => x.AtualizarSenhaAsync(usuarioId, It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _service.AlterarSenhaAsync(dto, "admin");

        Assert.True(result);
        _repositoryMock.Verify(
            x => x.AtualizarSenhaAsync(
                usuarioId,
                It.Is<string>(s => s == Criptografia.Encrypt("novaSenha123"))),
            Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveRetornarFalseQuandoCodigoInexistente()
    {
        var dto = new AlterarSenhaDto("999999", "novaSenha123", "novaSenha123");
        _repositoryMock.Setup(x => x.GetByCodigoAlterarSenhaAsync(dto.Codigo))
            .ReturnsAsync((UsuarioAuthResult?)null);

        var result = await _service.AlterarSenhaAsync(dto, "admin");

        Assert.False(result);
        _repositoryMock.Verify(x => x.AtualizarSenhaAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveRetornarFalseQuandoSenhasNaoCoincidem()
    {
        var dto = new AlterarSenhaDto("123456", "novaSenha123", "outraSenha");

        var result = await _service.AlterarSenhaAsync(dto, "admin");

        Assert.False(result);
        _repositoryMock.Verify(x => x.GetByCodigoAlterarSenhaAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AlterarSenhaAsync_DeveRetornarFalseQuandoUsuarioDeOutroTenant()
    {
        var dto = new AlterarSenhaDto("123456", "novaSenha123", "novaSenha123");
        _repositoryMock.Setup(x => x.GetByCodigoAlterarSenhaAsync(dto.Codigo))
            .ReturnsAsync(new UsuarioAuthResult
            {
                Id = Guid.NewGuid(),
                Perfil = UsuarioPerfil.Empresa,
                EmpresaDominio = "empresa1",
            });

        var result = await _service.AlterarSenhaAsync(dto, "empresa2");

        Assert.False(result);
        _repositoryMock.Verify(x => x.AtualizarSenhaAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }
}
