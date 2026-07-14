using Beneficios.Application.Interfaces;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;

namespace Beneficios.Application.Services;

public class PermissaoService : IPermissaoService
{
    private readonly IPerfilRepository _perfilRepository;

    public PermissaoService(IPerfilRepository perfilRepository)
    {
        _perfilRepository = perfilRepository;
    }

    public async Task<bool> PossuiAsync(Guid usuarioId, string codigoMenu, AcaoPermissao acao)
    {
        var permissoes = await _perfilRepository.ObterPermissoesPorUsuarioAsync(usuarioId);
        var linha = permissoes.FirstOrDefault(p =>
            p.CodigoMenu.Equals(codigoMenu, StringComparison.OrdinalIgnoreCase));

        if (linha is null)
            return false;

        return acao switch
        {
            AcaoPermissao.Visualizar => linha.Visualizar,
            AcaoPermissao.Criar => linha.Criar,
            AcaoPermissao.Editar => linha.Editar,
            AcaoPermissao.Excluir => linha.Excluir,
            _ => false,
        };
    }

    public async Task GarantirPermissaoAsync(Guid usuarioId, string codigoMenu, AcaoPermissao acao)
    {
        if (!await PossuiAsync(usuarioId, codigoMenu, acao))
            throw new UnauthorizedAccessException("Sem permissão para esta operação.");
    }
}
