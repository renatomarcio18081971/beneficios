using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _mapperMock = new Mock<IMapper>();
        _tokenServiceMock = new Mock<ITokenService>();
        _service = new UsuarioService(_repositoryMock.Object, _mapperMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_DeveCriarUsuarioComSucesso()
    {
        var dto = new UsuarioCreateDto("João Silva", "senha123", "joao@example.com", Guid.NewGuid());

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<UsuarioCreateParams>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result);
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<UsuarioCreateParams>(p =>
            p.Nome == dto.Nome && p.Email == dto.Email && p.EmpresaId == dto.EmpresaId)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizarUsuarioComSucesso()
    {
        var id = Guid.NewGuid();
        var dto = new UsuarioUpdateDto("João Silva", "joao@example.com", Guid.NewGuid());

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<UsuarioUpdateParams>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(id, dto, null);

        Assert.True(result);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<UsuarioUpdateParams>(p =>
            p.Id == id && p.Nome == dto.Nome && p.Email == dto.Email)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveDeletarUsuarioComSucesso()
    {
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        Assert.True(result);
        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }
}
