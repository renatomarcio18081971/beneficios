using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class PerfilServiceTests
{
    private readonly Mock<IPerfilRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly PerfilService _service;

    public PerfilServiceTests()
    {
        _repositoryMock = new Mock<IPerfilRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<PerfilProfile>()).CreateMapper();
        _service = new PerfilService(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task SalvarAsync_DeveCompletarPermissoesFaltantesDoCatalogo()
    {
        PerfilSalvarParams? salvo = null;
        _repositoryMock
            .Setup(x => x.SalvarAsync(It.IsAny<PerfilSalvarParams>()))
            .Callback<PerfilSalvarParams>(p => salvo = p)
            .ReturnsAsync((PerfilSalvarParams p) => p.Id);

        var id = await _service.SalvarAsync(new PerfilSalvarDto(
            "Operador",
            [new PermissaoMenuDto("usuarios", true, true, false, false)]));

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(salvo);
        Assert.Equal(ModulosSistemaCatalog.Todos.Count, salvo!.Permissoes.Count);
        Assert.Contains(salvo.Permissoes, p => p.CodigoMenu == "dashboard" && !p.Visualizar);
        Assert.Contains(salvo.Permissoes, p => p.CodigoMenu == "usuarios" && p.Criar);
        Assert.Contains(salvo.Permissoes, p => p.CodigoMenu == "perfis" && !p.Visualizar);
    }

    [Fact]
    public async Task DeleteAsync_PerfilSistema_DeveLancarInvalidOperation()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new PerfilQueryResult
        {
            Id = id,
            Nome = "Dono",
            EhSistema = true,
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id));
        Assert.Contains("sistema", ex.Message, StringComparison.OrdinalIgnoreCase);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_PerfilEmUso_DeveLancarInvalidOperation()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterUmAsync(id)).ReturnsAsync(new PerfilQueryResult
        {
            Id = id,
            Nome = "Operador",
            EhSistema = false,
        });
        _repositoryMock.Setup(x => x.EstaEmUsoAsync(id)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id));
        Assert.Contains("em uso", ex.Message, StringComparison.OrdinalIgnoreCase);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
