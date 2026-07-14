using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class FuncionarioService : IFuncionarioService
{
    public const string MensagemCpfInvalido = "CPF inválido.";
    public const string MensagemCpfDuplicado = "CPF já cadastrado.";
    public const string MensagemMatriculaDuplicada = "Matrícula já cadastrada.";
    public const string MensagemNaoEncontrado = "Funcionário não encontrado.";
    public const string MensagemDesligamento = "Informe a data de desligamento.";
    public const string MensagemJornadaEspecial = "Informe o detalhe da jornada especial.";
    public const string MensagemBeneficioNaoSuportado = "Benefício não suportado na v1.";
    public const string MensagemSituacaoInvalida = "Situação inválida.";
    public const string MensagemAtivoComAfastamento = "Existe afastamento ativo; encerre ou exclua o período antes de marcar como ativo.";
    public const string CodigoValeTransporte = "vale_transporte";

    private readonly IFuncionarioRepository _repository;
    private readonly IFuncionarioAfastamentoRepository _afastamentoRepository;
    private readonly IMapper _mapper;

    public FuncionarioService(
        IFuncionarioRepository repository,
        IFuncionarioAfastamentoRepository afastamentoRepository,
        IMapper mapper)
    {
        _repository = repository;
        _afastamentoRepository = afastamentoRepository;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(FuncionarioSalvarDto dto, Guid? usuarioAlteracaoId)
    {
        var cpf = ValidarENormalizarCpf(dto.Cpf);
        var matricula = NormalizarMatricula(dto.Matricula);
        ValidarRegras(dto.Situacao, dto.DataDesligamento, dto.Jornada, dto.JornadaDetalhe);
        await ValidarAtivoSemAfastamentoAsync(dto.Situacao, null);
        var beneficios = NormalizarBeneficios(dto.Beneficios);

        if (await _repository.CpfExisteAsync(cpf))
            throw new InvalidOperationException(MensagemCpfDuplicado);
        if (matricula is not null && await _repository.MatriculaExisteAsync(matricula))
            throw new InvalidOperationException(MensagemMatriculaDuplicada);

        var id = Guid.NewGuid();
        await _repository.SalvarAsync(MapearSalvar(dto, id, cpf, matricula), beneficios);
        return id;
    }

    public async Task AtualizarAsync(Guid id, FuncionarioAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        _ = await _repository.ObterPorIdAsync(id)
            ?? throw new InvalidOperationException(MensagemNaoEncontrado);

        var cpf = ValidarENormalizarCpf(dto.Cpf);
        var matricula = NormalizarMatricula(dto.Matricula);
        ValidarRegras(dto.Situacao, dto.DataDesligamento, dto.Jornada, dto.JornadaDetalhe);
        await ValidarAtivoSemAfastamentoAsync(dto.Situacao, id);
        var beneficios = NormalizarBeneficios(dto.Beneficios);

        if (await _repository.CpfExisteAsync(cpf, id))
            throw new InvalidOperationException(MensagemCpfDuplicado);
        if (matricula is not null && await _repository.MatriculaExisteAsync(matricula, id))
            throw new InvalidOperationException(MensagemMatriculaDuplicada);

        await _repository.AtualizarAsync(MapearAtualizar(dto, id, cpf, matricula, usuarioAlteracaoId), beneficios);
    }

    public async Task<FuncionarioDto?> ObterPorIdAsync(Guid id)
    {
        var entity = await _repository.ObterPorIdAsync(id);
        if (entity is null)
            return null;

        var dto = _mapper.Map<FuncionarioDto>(entity);
        var beneficios = await _repository.ObterBeneficiosAsync(id);
        return dto with { Beneficios = _mapper.Map<List<FuncionarioBeneficioDto>>(beneficios) };
    }

    public async Task<IReadOnlyList<FuncionarioDto>> FiltrarAsync(FuncionarioFiltroDto filtro)
    {
        var lista = await _repository.FiltrarAsync(new FuncionarioFiltroParams
        {
            Nome = filtro.Nome,
            Cpf = filtro.Cpf,
            Matricula = filtro.Matricula,
            Situacao = filtro.Situacao,
        });

        return lista.Select(f => _mapper.Map<FuncionarioDto>(f) with { Beneficios = Array.Empty<FuncionarioBeneficioDto>() }).ToArray();
    }

    private static string ValidarENormalizarCpf(string cpfRaw)
    {
        var cpf = CpfUtil.Normalizar(cpfRaw)
            ?? throw new InvalidOperationException(MensagemCpfInvalido);
        if (!CpfUtil.EhValido(cpf))
            throw new InvalidOperationException(MensagemCpfInvalido);
        return cpf;
    }

    private static string? NormalizarMatricula(string? matricula)
        => string.IsNullOrWhiteSpace(matricula) ? null : matricula.Trim();

    private static void ValidarRegras(
        SituacaoFuncionario situacao,
        DateOnly? dataDesligamento,
        JornadaTrabalho jornada,
        string? jornadaDetalhe)
    {
        if (situacao == SituacaoFuncionario.Afastado)
            throw new InvalidOperationException(MensagemSituacaoInvalida);

        if (situacao == SituacaoFuncionario.Desligado && dataDesligamento is null)
            throw new InvalidOperationException(MensagemDesligamento);

        if (JornadaTrabalhoCatalog.ExigeDetalhe(jornada))
        {
            if (string.IsNullOrWhiteSpace(jornadaDetalhe))
                throw new InvalidOperationException(MensagemJornadaEspecial);
        }
    }

    private async Task ValidarAtivoSemAfastamentoAsync(SituacaoFuncionario situacao, Guid? funcionarioId)
    {
        if (situacao != SituacaoFuncionario.Ativo || funcionarioId is null)
            return;

        var ativo = await _afastamentoRepository.ObterAtivoEmAsync(
            funcionarioId.Value, DateOnly.FromDateTime(DateTime.UtcNow));
        if (ativo is not null)
            throw new InvalidOperationException(MensagemAtivoComAfastamento);
    }

    private static IReadOnlyList<FuncionarioBeneficioSalvarParams> NormalizarBeneficios(
        IReadOnlyList<FuncionarioBeneficioSalvarDto>? beneficios)
    {
        if (beneficios is null || beneficios.Count == 0)
            return Array.Empty<FuncionarioBeneficioSalvarParams>();

        var lista = new List<FuncionarioBeneficioSalvarParams>();
        foreach (var b in beneficios)
        {
            if (!string.Equals(b.CodigoBeneficio, CodigoValeTransporte, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(MensagemBeneficioNaoSuportado);

            lista.Add(new FuncionarioBeneficioSalvarParams
            {
                Id = Guid.NewGuid(),
                CodigoBeneficio = CodigoValeTransporte,
                Ativo = b.Ativo,
                DataInicio = b.DataInicio,
                DataFim = b.DataFim,
                OptIn = b.OptIn,
            });
        }

        return lista;
    }

    private static FuncionarioSalvarParams MapearSalvar(
        FuncionarioSalvarDto dto,
        Guid id,
        string cpf,
        string? matricula) => new()
    {
        Id = id,
        Nome = dto.Nome.Trim(),
        Cpf = cpf,
        Matricula = matricula,
        DataAdmissao = dto.DataAdmissao,
        DataDesligamento = dto.Situacao == SituacaoFuncionario.Desligado ? dto.DataDesligamento : dto.DataDesligamento,
        Cargo = dto.Cargo.Trim(),
        SalarioBase = dto.SalarioBase,
        TipoContrato = dto.TipoContrato,
        CentroCusto = string.IsNullOrWhiteSpace(dto.CentroCusto) ? null : dto.CentroCusto.Trim(),
        ResCep = FuncionarioConversao.SomenteDigitosOuNulo(dto.ResCep),
        ResLogradouro = NullIfWhite(dto.ResLogradouro),
        ResNumero = NullIfWhite(dto.ResNumero),
        ResComplemento = NullIfWhite(dto.ResComplemento),
        ResBairro = NullIfWhite(dto.ResBairro),
        ResCidade = NullIfWhite(dto.ResCidade),
        ResUf = NullIfWhite(dto.ResUf)?.ToUpperInvariant(),
        TrabNomeLocal = NullIfWhite(dto.TrabNomeLocal),
        TrabCep = FuncionarioConversao.SomenteDigitosOuNulo(dto.TrabCep),
        TrabLogradouro = NullIfWhite(dto.TrabLogradouro),
        TrabNumero = NullIfWhite(dto.TrabNumero),
        TrabComplemento = NullIfWhite(dto.TrabComplemento),
        TrabBairro = NullIfWhite(dto.TrabBairro),
        TrabCidade = NullIfWhite(dto.TrabCidade),
        TrabUf = NullIfWhite(dto.TrabUf)?.ToUpperInvariant(),
        Situacao = dto.Situacao,
        Jornada = dto.Jornada,
        JornadaDetalhe = JornadaTrabalhoCatalog.ExigeDetalhe(dto.Jornada)
            ? dto.JornadaDetalhe!.Trim()
            : null,
        DataInclusao = DateTime.UtcNow,
    };

    private static FuncionarioAtualizarParams MapearAtualizar(
        FuncionarioAtualizarDto dto,
        Guid id,
        string cpf,
        string? matricula,
        Guid? usuarioAlteracaoId) => new()
    {
        Id = id,
        Nome = dto.Nome.Trim(),
        Cpf = cpf,
        Matricula = matricula,
        DataAdmissao = dto.DataAdmissao,
        DataDesligamento = dto.DataDesligamento,
        Cargo = dto.Cargo.Trim(),
        SalarioBase = dto.SalarioBase,
        TipoContrato = dto.TipoContrato,
        CentroCusto = string.IsNullOrWhiteSpace(dto.CentroCusto) ? null : dto.CentroCusto.Trim(),
        ResCep = FuncionarioConversao.SomenteDigitosOuNulo(dto.ResCep),
        ResLogradouro = NullIfWhite(dto.ResLogradouro),
        ResNumero = NullIfWhite(dto.ResNumero),
        ResComplemento = NullIfWhite(dto.ResComplemento),
        ResBairro = NullIfWhite(dto.ResBairro),
        ResCidade = NullIfWhite(dto.ResCidade),
        ResUf = NullIfWhite(dto.ResUf)?.ToUpperInvariant(),
        TrabNomeLocal = NullIfWhite(dto.TrabNomeLocal),
        TrabCep = FuncionarioConversao.SomenteDigitosOuNulo(dto.TrabCep),
        TrabLogradouro = NullIfWhite(dto.TrabLogradouro),
        TrabNumero = NullIfWhite(dto.TrabNumero),
        TrabComplemento = NullIfWhite(dto.TrabComplemento),
        TrabBairro = NullIfWhite(dto.TrabBairro),
        TrabCidade = NullIfWhite(dto.TrabCidade),
        TrabUf = NullIfWhite(dto.TrabUf)?.ToUpperInvariant(),
        Situacao = dto.Situacao,
        Jornada = dto.Jornada,
        JornadaDetalhe = JornadaTrabalhoCatalog.ExigeDetalhe(dto.Jornada)
            ? dto.JornadaDetalhe!.Trim()
            : null,
        UsuarioAlteracaoId = usuarioAlteracaoId,
    };

    private static string? NullIfWhite(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
