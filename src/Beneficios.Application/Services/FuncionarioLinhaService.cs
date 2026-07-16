using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class FuncionarioLinhaService : IFuncionarioLinhaService
{
    public const string MensagemVtInativo = "Funcionário sem vale transporte ativo; não é possível vincular linhas.";
    public const string MensagemLinhaNaoVigente = "A linha selecionada não está vigente na data de início do vínculo.";
    public const string MensagemParDuplicado = "Esta linha já está vinculada a este funcionário.";
    public const string MensagemQuantidade = "A quantidade de utilizações deve ser maior ou igual a 1.";
    public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
    public const string MensagemVinculoNaoEncontrado = "Vínculo não encontrado.";
    public const string MensagemFuncionarioNaoEncontrado = "Funcionário não encontrado.";
    public const string MensagemLinhaNaoEncontrada = "Linha não encontrada.";
    public const string CodigoValeTransporte = "vale_transporte";

    private readonly IFuncionarioLinhaRepository _vinculoRepository;
    private readonly ILinhaOnibusRepository _linhaRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IMapper _mapper;

    public FuncionarioLinhaService(
        IFuncionarioLinhaRepository vinculoRepository,
        ILinhaOnibusRepository linhaRepository,
        IFuncionarioRepository funcionarioRepository,
        IMapper mapper)
    {
        _vinculoRepository = vinculoRepository;
        _linhaRepository = linhaRepository;
        _funcionarioRepository = funcionarioRepository;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId)
    {
        Validar(dto.DataInicio, dto.DataFim, dto.Quantidade);
        await GarantirFuncionarioComVtAsync(dto.FuncionarioId);
        await GarantirLinhaVigenteAsync(dto.LinhaOnibusId, dto.DataInicio);

        if (await _vinculoRepository.ExisteParAsync(dto.FuncionarioId, dto.LinhaOnibusId))
            throw new InvalidOperationException(MensagemParDuplicado);

        var id = Guid.NewGuid();
        await _vinculoRepository.SalvarAsync(new FuncionarioLinhaSalvarParams
        {
            Id = id,
            FuncionarioId = dto.FuncionarioId,
            LinhaOnibusId = dto.LinhaOnibusId,
            Quantidade = dto.Quantidade,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            DataInclusao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });
        return id;
    }

    public async Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        Validar(dto.DataInicio, dto.DataFim, dto.Quantidade);

        var existente = await _vinculoRepository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemVinculoNaoEncontrado);

        // VT só é exigido se o vínculo permanecer aberto após o save (criar/reabrir/manter vigente).
        // Encerrar (dataFim no passado ou hoje) deve ser permitido sem VT ativo.
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var permaneceAberto = dto.DataFim is null || dto.DataFim >= hoje;
        if (permaneceAberto)
            await GarantirFuncionarioComVtAsync(existente.FuncionarioId);

        await GarantirLinhaVigenteAsync(dto.LinhaOnibusId, dto.DataInicio);

        if (await _vinculoRepository.ExisteParAsync(existente.FuncionarioId, dto.LinhaOnibusId, id))
            throw new InvalidOperationException(MensagemParDuplicado);

        await _vinculoRepository.AtualizarAsync(new FuncionarioLinhaAtualizarParams
        {
            Id = id,
            LinhaOnibusId = dto.LinhaOnibusId,
            Quantidade = dto.Quantidade,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            DataAlteracao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });
    }

    public async Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id)
    {
        var entity = await _vinculoRepository.ObterPorIdAsync(id);
        return entity is null ? null : _mapper.Map<FuncionarioLinhaDto>(entity);
    }

    public async Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes)
    {
        var lista = await _vinculoRepository.FiltrarAsync(new FuncionarioLinhaFiltroParams
        {
            FuncionarioId = funcionarioId,
            SomenteVigentes = somenteVigentes,
            Referencia = somenteVigentes == true ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
        });

        IEnumerable<FuncionarioLinhaQueryResult> filtrada = lista;
        if (somenteVigentes == true)
        {
            var referencia = DateOnly.FromDateTime(DateTime.UtcNow);
            filtrada = lista.Where(v => AfastamentoPeriodo.EstaAtivoEm(v.DataInicio, v.DataFim, referencia));
        }

        return _mapper.Map<List<FuncionarioLinhaDto>>(filtrada.ToList());
    }

    public Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId)
        => _vinculoRepository.EncerrarAbertosPorFuncionarioAsync(
            funcionarioId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTime.UtcNow,
            usuarioAlteracaoId);

    private async Task GarantirFuncionarioComVtAsync(Guid funcionarioId)
    {
        _ = await _funcionarioRepository.ObterPorIdAsync(funcionarioId)
            ?? throw new InvalidOperationException(MensagemFuncionarioNaoEncontrado);

        var beneficios = await _funcionarioRepository.ObterBeneficiosAsync(funcionarioId);
        var vtAtivo = beneficios.Any(b =>
            string.Equals(b.CodigoBeneficio, CodigoValeTransporte, StringComparison.OrdinalIgnoreCase)
            && b.Ativo);

        if (!vtAtivo)
            throw new InvalidOperationException(MensagemVtInativo);
    }

    private async Task GarantirLinhaVigenteAsync(Guid linhaOnibusId, DateOnly dataInicioVinculo)
    {
        var linha = await _linhaRepository.ObterPorIdAsync(linhaOnibusId)
            ?? throw new InvalidOperationException(MensagemLinhaNaoEncontrada);

        if (!AfastamentoPeriodo.EstaAtivoEm(linha.DataInicio, linha.DataFim, dataInicioVinculo))
            throw new InvalidOperationException(MensagemLinhaNaoVigente);
    }

    private static void Validar(DateOnly dataInicio, DateOnly? dataFim, int quantidade)
    {
        if (quantidade < 1)
            throw new InvalidOperationException(MensagemQuantidade);
        if (dataFim is not null && dataFim < dataInicio)
            throw new InvalidOperationException(MensagemDataFim);
    }
}
