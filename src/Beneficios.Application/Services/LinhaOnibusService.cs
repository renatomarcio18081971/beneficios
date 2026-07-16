using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class LinhaOnibusService : ILinhaOnibusService
{
    public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
    public const string MensagemNaoEncontrada = "Linha não encontrada.";
    public const string MensagemVinculosAbertos = "Existem vínculos em aberto para esta linha; encerre-os antes de finalizar a vigência.";
    public const string MensagemTarifa = "O valor da tarifa deve ser maior ou igual a zero.";

    private readonly ILinhaOnibusRepository _linhaRepository;
    private readonly IFuncionarioLinhaRepository _vinculoRepository;
    private readonly IMapper _mapper;

    public LinhaOnibusService(
        ILinhaOnibusRepository linhaRepository,
        IFuncionarioLinhaRepository vinculoRepository,
        IMapper mapper)
    {
        _linhaRepository = linhaRepository;
        _vinculoRepository = vinculoRepository;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId)
    {
        Validar(dto.DataInicio, dto.DataFim, dto.ValorTarifa);

        var id = Guid.NewGuid();
        await _linhaRepository.SalvarAsync(new LinhaOnibusSalvarParams
        {
            Id = id,
            Descricao = dto.Descricao.Trim(),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            ValorTarifa = dto.ValorTarifa,
            DataInclusao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });
        return id;
    }

    public async Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        Validar(dto.DataInicio, dto.DataFim, dto.ValorTarifa);

        _ = await _linhaRepository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemNaoEncontrada);

        if (dto.DataFim is not null && await _vinculoRepository.ExisteVinculoAbertoPorLinhaAsync(id))
            throw new InvalidOperationException(MensagemVinculosAbertos);

        await _linhaRepository.AtualizarAsync(new LinhaOnibusAtualizarParams
        {
            Id = id,
            Descricao = dto.Descricao.Trim(),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            ValorTarifa = dto.ValorTarifa,
            DataAlteracao = DateTime.UtcNow,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });
    }

    public async Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id)
    {
        var entity = await _linhaRepository.ObterPorIdAsync(id);
        return entity is null ? null : _mapper.Map<LinhaOnibusDto>(entity);
    }

    public async Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(
        string? descricao,
        bool? somenteVigentes,
        DateOnly? referencia = null)
    {
        var dataRef = referencia ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var lista = await _linhaRepository.FiltrarAsync(new LinhaOnibusFiltroParams
        {
            Descricao = descricao,
            SomenteVigentes = somenteVigentes,
            Referencia = somenteVigentes == true ? dataRef : null,
        });

        return _mapper.Map<List<LinhaOnibusDto>>(lista);
    }

    private static void Validar(DateOnly dataInicio, DateOnly? dataFim, decimal valorTarifa)
    {
        if (dataFim is not null && dataFim < dataInicio)
            throw new InvalidOperationException(MensagemDataFim);
        if (valorTarifa < 0)
            throw new InvalidOperationException(MensagemTarifa);
    }
}