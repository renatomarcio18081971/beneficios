using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public sealed record ModuloSistema(
    string Codigo,
    string NomeExibicao,
    string Rota,
    AcaoPermissao AcoesSuportadas);
