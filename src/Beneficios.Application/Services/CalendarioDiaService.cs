using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class CalendarioDiaService : ICalendarioDiaService
{
    public const string MensagemAnoJaCadastrado = "Este ano já está cadastrado.";
    public const string MensagemDiaNaoEncontrado = "Dia não encontrado.";

    private readonly ICalendarioDiaRepository _repository;
    private readonly IMapper _mapper;

    public CalendarioDiaService(ICalendarioDiaRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task GerarAnoAsync(int ano)
    {
        if (await _repository.AnoExisteAsync(ano))
            throw new InvalidOperationException(MensagemAnoJaCadastrado);

        var dias = MontarDiasDoAno(ano);
        await _repository.InserirLoteAsync(dias);
    }

    public async Task<CalendarioDiaDto[]> ObterPorMesAsync(int ano, int mes)
    {
        var dias = await _repository.ObterPorMesAsync(ano, mes);
        return _mapper.Map<CalendarioDiaDto[]>(dias);
    }

    public async Task<CalendarioDiaDto?> ObterPorIdAsync(Guid id)
    {
        var dia = await _repository.ObterPorIdAsync(id);
        return dia is null ? null : _mapper.Map<CalendarioDiaDto>(dia);
    }

    public async Task AtualizarAsync(Guid id, CalendarioDiaAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        var existente = await _repository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemDiaNaoEncontrado);

        var ok = await _repository.AtualizarAsync(new CalendarioDiaAtualizarParams
        {
            Id = existente.Id,
            EhDiaUtil = dto.EhDiaUtil,
            TipoExcecao = dto.TipoExcecao,
            Origem = OrigemCalendarioDia.Manual,
            Observacao = dto.Observacao,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });

        if (!ok)
            throw new InvalidOperationException(MensagemDiaNaoEncontrado);
    }

    public static IReadOnlyList<CalendarioDiaSalvarParams> MontarDiasDoAno(int ano)
    {
        var inicio = new DateOnly(ano, 1, 1);
        var fim = new DateOnly(ano, 12, 31);
        var mapa = new Dictionary<DateOnly, CalendarioDiaSalvarParams>();

        for (var data = inicio; data <= fim; data = data.AddDays(1))
        {
            var ehUtil = data.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
            mapa[data] = new CalendarioDiaSalvarParams
            {
                Id = Guid.NewGuid(),
                Data = data,
                EhDiaUtil = ehUtil,
                TipoExcecao = null,
                Origem = OrigemCalendarioDia.Geracao,
                Observacao = null,
            };
        }

        foreach (var feriado in FeriadosNacionaisCatalog.ObterParaAno(ano))
        {
            if (!mapa.TryGetValue(feriado.Data, out var dia))
                continue;

            mapa[feriado.Data] = new CalendarioDiaSalvarParams
            {
                Id = dia.Id,
                Data = dia.Data,
                EhDiaUtil = false,
                TipoExcecao = TipoExcecaoCalendario.Nacional,
                Origem = OrigemCalendarioDia.Nacional,
                Observacao = feriado.Nome,
            };
        }

        return mapa.Values.OrderBy(d => d.Data).ToList();
    }
}
