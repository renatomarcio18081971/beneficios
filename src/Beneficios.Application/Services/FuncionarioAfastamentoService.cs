using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class FuncionarioAfastamentoService : IFuncionarioAfastamentoService
{
    public const string MensagemSobreposicao = "Já existe um afastamento neste período para o funcionário.";
    public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
    public const string MensagemFuncionarioNaoEncontrado = "Funcionário não encontrado.";
    public const string MensagemAfastamentoNaoEncontrado = "Afastamento não encontrado.";

    private readonly IFuncionarioAfastamentoRepository _afastamentoRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IMapper _mapper;

    public FuncionarioAfastamentoService(
        IFuncionarioAfastamentoRepository afastamentoRepository,
        IFuncionarioRepository funcionarioRepository,
        IMapper mapper)
    {
        _afastamentoRepository = afastamentoRepository;
        _funcionarioRepository = funcionarioRepository;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(AfastamentoSalvarDto dto, Guid? usuarioAlteracaoId)
    {
        ValidarDatas(dto.DataInicio, dto.DataFim);

        _ = await _funcionarioRepository.ObterPorIdAsync(dto.FuncionarioId)
            ?? throw new InvalidOperationException(MensagemFuncionarioNaoEncontrado);

        if (await _afastamentoRepository.ExisteSobreposicaoAsync(dto.FuncionarioId, dto.DataInicio, dto.DataFim))
            throw new InvalidOperationException(MensagemSobreposicao);

        var id = Guid.NewGuid();
        await _afastamentoRepository.SalvarAsync(new FuncionarioAfastamentoSalvarParams
        {
            Id = id,
            FuncionarioId = dto.FuncionarioId,
            Tipo = TipoAfastamentoCatalog.ParaBanco(dto.Tipo),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Observacao = NullIfWhite(dto.Observacao),
            DataInclusao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });

        await RecalcularSituacaoAsync(dto.FuncionarioId);
        return id;
    }

    public async Task AtualizarAsync(Guid id, AfastamentoAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        ValidarDatas(dto.DataInicio, dto.DataFim);

        var existente = await _afastamentoRepository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemAfastamentoNaoEncontrado);

        if (await _afastamentoRepository.ExisteSobreposicaoAsync(
                existente.FuncionarioId, dto.DataInicio, dto.DataFim, id))
            throw new InvalidOperationException(MensagemSobreposicao);

        await _afastamentoRepository.AtualizarAsync(new FuncionarioAfastamentoAtualizarParams
        {
            Id = id,
            FuncionarioId = existente.FuncionarioId,
            Tipo = TipoAfastamentoCatalog.ParaBanco(dto.Tipo),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Observacao = NullIfWhite(dto.Observacao),
            DataAlteracao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });

        await RecalcularSituacaoAsync(existente.FuncionarioId);
    }

    public async Task ExcluirAsync(Guid id)
    {
        var existente = await _afastamentoRepository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemAfastamentoNaoEncontrado);

        await _afastamentoRepository.ExcluirAsync(id);
        await RecalcularSituacaoAsync(existente.FuncionarioId);
    }

    public async Task<AfastamentoDto?> ObterPorIdAsync(Guid id)
    {
        var entity = await _afastamentoRepository.ObterPorIdAsync(id);
        return entity is null ? null : _mapper.Map<AfastamentoDto>(entity);
    }

    public async Task<IReadOnlyList<AfastamentoDto>> FiltrarAsync(
        Guid? funcionarioId,
        TipoAfastamento? tipo,
        DateOnly? dataInicio,
        DateOnly? dataFim)
    {
        var lista = await _afastamentoRepository.FiltrarAsync(new FuncionarioAfastamentoFiltroParams
        {
            FuncionarioId = funcionarioId,
            Tipo = tipo is null ? null : TipoAfastamentoCatalog.ParaBanco(tipo.Value),
            DataInicio = dataInicio,
            DataFim = dataFim,
        });
        return _mapper.Map<List<AfastamentoDto>>(lista);
    }

    private async Task RecalcularSituacaoAsync(Guid funcionarioId)
    {
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(funcionarioId)
            ?? throw new InvalidOperationException(MensagemFuncionarioNaoEncontrado);

        if (funcionario.Situacao == SituacaoFuncionario.Desligado)
            return;

        var hoje = ObterHoje();
        var ativo = await _afastamentoRepository.ObterAtivoEmAsync(funcionarioId, hoje);
        var situacao = ativo is null ? "ativo" : "afastado";
        await _funcionarioRepository.AtualizarSituacaoAsync(funcionarioId, situacao);
    }

    private static void ValidarDatas(DateOnly dataInicio, DateOnly? dataFim)
    {
        if (dataFim is not null && dataFim < dataInicio)
            throw new InvalidOperationException(MensagemDataFim);
    }

    private static DateOnly ObterHoje() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static string? NullIfWhite(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
