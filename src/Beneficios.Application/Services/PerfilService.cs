using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly IMapper _mapper;

    public PerfilService(IPerfilRepository perfilRepository, IMapper mapper)
    {
        _perfilRepository = perfilRepository;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(PerfilSalvarDto dto)
    {
        var perfilId = Guid.NewGuid();
        var permissoes = NormalizarPermissoes(perfilId, dto.Permissoes);

        await _perfilRepository.SalvarAsync(new PerfilSalvarParams
        {
            Id = perfilId,
            Nome = dto.Nome.Trim(),
            EhSistema = false,
            Permissoes = permissoes,
        });

        return perfilId;
    }

    public async Task AtualizarAsync(Guid id, PerfilAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        var existente = await _perfilRepository.ObterUmAsync(id)
            ?? throw new InvalidOperationException("Perfil não encontrado.");

        if (existente.EhSistema)
            throw new InvalidOperationException("Perfil de sistema não pode ser alterado.");

        var ok = await _perfilRepository.AtualizarAsync(new PerfilAtualizarParams
        {
            Id = id,
            Nome = dto.Nome.Trim(),
            UsuarioAlteracaoId = usuarioAlteracaoId,
            Permissoes = NormalizarPermissoes(id, dto.Permissoes),
        });

        if (!ok)
            throw new InvalidOperationException("Perfil não encontrado.");
    }

    public async Task DeleteAsync(Guid id)
    {
        var existente = await _perfilRepository.ObterUmAsync(id)
            ?? throw new InvalidOperationException("Perfil não encontrado.");

        if (existente.EhSistema)
            throw new InvalidOperationException("Perfil de sistema não pode ser excluído.");

        if (await _perfilRepository.EstaEmUsoAsync(id))
            throw new InvalidOperationException("Perfil em uso por usuários e não pode ser excluído.");

        var ok = await _perfilRepository.DeleteAsync(id);
        if (!ok)
            throw new InvalidOperationException("Perfil não encontrado.");
    }

    public async Task<PerfilDto?> ObterUmAsync(Guid id)
    {
        var perfil = await _perfilRepository.ObterUmAsync(id);
        return perfil is null ? null : _mapper.Map<PerfilDto>(perfil);
    }

    public async Task<PerfilDto[]> ObterTodosAsync()
    {
        var perfis = await _perfilRepository.ObterTodosAsync();
        return _mapper.Map<PerfilDto[]>(perfis);
    }

    public async Task<PerfilDto[]> FiltrarAsync(PerfilFiltroDto filtro)
    {
        var perfis = await _perfilRepository.FiltrarAsync(_mapper.Map<PerfilFiltroParams>(filtro));
        return _mapper.Map<PerfilDto[]>(perfis);
    }

    private static IReadOnlyList<PerfilPermissaoParams> NormalizarPermissoes(
        Guid perfilId,
        IReadOnlyList<PermissaoMenuDto>? incoming)
    {
        var byCodigo = (incoming ?? [])
            .Where(p => ModulosSistemaCatalog.ObterPorCodigo(p.CodigoMenu) is not null)
            .GroupBy(p => p.CodigoMenu, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);

        return ModulosSistemaCatalog.Todos
            .Select(modulo =>
            {
                byCodigo.TryGetValue(modulo.Codigo, out var dto);
                return new PerfilPermissaoParams
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = modulo.Codigo,
                    Visualizar = dto?.Visualizar ?? false,
                    Criar = dto?.Criar ?? false,
                    Editar = dto?.Editar ?? false,
                    Excluir = dto?.Excluir ?? false,
                };
            })
            .ToList();
    }
}
