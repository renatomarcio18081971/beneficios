using Beneficios.Domain.Enums;

namespace Beneficios.Application.Interfaces;

public interface IPermissaoService
{
    Task<bool> PossuiAsync(Guid usuarioId, string codigoMenu, AcaoPermissao acao);
    Task GarantirPermissaoAsync(Guid usuarioId, string codigoMenu, AcaoPermissao acao);
}
