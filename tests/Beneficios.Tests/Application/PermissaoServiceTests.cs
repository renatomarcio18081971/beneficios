using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class PermissaoServiceTests
{
    private readonly Mock<IPerfilRepository> _repositoryMock;
    private readonly PermissaoService _service;

    public PermissaoServiceTests()
    {
        _repositoryMock = new Mock<IPerfilRepository>();
        _service = new PermissaoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task PossuiAsync_QuandoSemPermissao_DeveRetornarFalse()
    {
        var usuarioId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ObterPermissoesPorUsuarioAsync(usuarioId))
            .ReturnsAsync([
                new PerfilPermissaoParams
                {
                    CodigoMenu = "usuarios",
                    Visualizar = true,
                    Criar = false,
                }
            ]);

        var possui = await _service.PossuiAsync(usuarioId, "usuarios", AcaoPermissao.Criar);

        Assert.False(possui);
    }

    [Fact]
    public async Task EnsureAsync_QuandoSemPermissao_DeveLancarUnauthorizedAccess()
    {
        var usuarioId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ObterPermissoesPorUsuarioAsync(usuarioId))
            .ReturnsAsync([]);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.EnsureAsync(usuarioId, "usuarios", AcaoPermissao.Visualizar));
    }

    [Fact]
    public async Task EnsureAsync_QuandoTemPermissao_NaoDeveLancar()
    {
        var usuarioId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ObterPermissoesPorUsuarioAsync(usuarioId))
            .ReturnsAsync([
                new PerfilPermissaoParams
                {
                    CodigoMenu = "usuarios",
                    Visualizar = true,
                }
            ]);

        await _service.EnsureAsync(usuarioId, "usuarios", AcaoPermissao.Visualizar);
    }
}
