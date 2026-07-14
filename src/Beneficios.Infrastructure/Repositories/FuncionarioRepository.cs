using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using System.Data;
using System.Text;

namespace Beneficios.Infrastructure.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly IDbConnection _dbConnection;

    public FuncionarioRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(
        FuncionarioSalvarParams parametros,
        IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios)
    {
        EnsureOpen();
        using var tx = _dbConnection.BeginTransaction();

        const string sql = """
            INSERT INTO funcionarios (
                id, nome, cpf, matricula, data_admissao, data_desligamento,
                cargo, salario_base, tipo_contrato, centro_custo,
                res_cep, res_logradouro, res_numero, res_complemento, res_bairro, res_cidade, res_uf,
                trab_nome_local, trab_cep, trab_logradouro, trab_numero, trab_complemento, trab_bairro, trab_cidade, trab_uf,
                situacao, motivo_afastamento, jornada, jornada_detalhe, data_inclusao)
            VALUES (
                @Id, @Nome, @Cpf, @Matricula, @DataAdmissao, @DataDesligamento,
                @Cargo, @SalarioBase, @TipoContrato, @CentroCusto,
                @ResCep, @ResLogradouro, @ResNumero, @ResComplemento, @ResBairro, @ResCidade, @ResUf,
                @TrabNomeLocal, @TrabCep, @TrabLogradouro, @TrabNumero, @TrabComplemento, @TrabBairro, @TrabCidade, @TrabUf,
                @Situacao, @MotivoAfastamento, @Jornada, @JornadaDetalhe, @DataInclusao)
            """;

        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros), tx);
        await SyncBeneficiosAsync(parametros.Id, beneficios, tx);
        tx.Commit();
        return parametros.Id;
    }

    public async Task AtualizarAsync(
        FuncionarioAtualizarParams parametros,
        IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios)
    {
        EnsureOpen();
        using var tx = _dbConnection.BeginTransaction();

        const string sql = """
            UPDATE funcionarios SET
                nome = @Nome,
                cpf = @Cpf,
                matricula = @Matricula,
                data_admissao = @DataAdmissao,
                data_desligamento = @DataDesligamento,
                cargo = @Cargo,
                salario_base = @SalarioBase,
                tipo_contrato = @TipoContrato,
                centro_custo = @CentroCusto,
                res_cep = @ResCep,
                res_logradouro = @ResLogradouro,
                res_numero = @ResNumero,
                res_complemento = @ResComplemento,
                res_bairro = @ResBairro,
                res_cidade = @ResCidade,
                res_uf = @ResUf,
                trab_nome_local = @TrabNomeLocal,
                trab_cep = @TrabCep,
                trab_logradouro = @TrabLogradouro,
                trab_numero = @TrabNumero,
                trab_complemento = @TrabComplemento,
                trab_bairro = @TrabBairro,
                trab_cidade = @TrabCidade,
                trab_uf = @TrabUf,
                situacao = @Situacao,
                motivo_afastamento = @MotivoAfastamento,
                jornada = @Jornada,
                jornada_detalhe = @JornadaDetalhe,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;

        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros), tx);
        if (rows == 0)
            throw new InvalidOperationException("Funcionário não encontrado.");

        await SyncBeneficiosAsync(parametros.Id, beneficios, tx);
        tx.Commit();
    }

    public async Task<FuncionarioQueryResult?> ObterPorIdAsync(Guid id)
    {
        const string sql = """
            SELECT id AS Id, nome AS Nome, cpf AS Cpf, matricula AS Matricula,
                data_admissao AS DataAdmissaoObj, data_desligamento AS DataDesligamentoObj,
                cargo AS Cargo, salario_base AS SalarioBase,
                tipo_contrato AS TipoContratoTexto, centro_custo AS CentroCusto,
                res_cep AS ResCep, res_logradouro AS ResLogradouro, res_numero AS ResNumero,
                res_complemento AS ResComplemento, res_bairro AS ResBairro, res_cidade AS ResCidade, res_uf AS ResUf,
                trab_nome_local AS TrabNomeLocal, trab_cep AS TrabCep, trab_logradouro AS TrabLogradouro,
                trab_numero AS TrabNumero, trab_complemento AS TrabComplemento, trab_bairro AS TrabBairro,
                trab_cidade AS TrabCidade, trab_uf AS TrabUf,
                situacao AS SituacaoTexto, motivo_afastamento AS MotivoAfastamento,
                jornada AS JornadaTexto, jornada_detalhe AS JornadaDetalhe,
                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
            FROM funcionarios WHERE id = @Id
            """;

        var row = await _dbConnection.QueryFirstOrDefaultAsync<FuncionarioRow>(sql, new { Id = id });
        return row is null ? null : Mapear(row);
    }

    public async Task<IReadOnlyList<FuncionarioBeneficioQueryResult>> ObterBeneficiosAsync(Guid funcionarioId)
    {
        const string sql = """
            SELECT id AS Id, funcionario_id AS FuncionarioId, codigo_beneficio AS CodigoBeneficio,
                ativo AS Ativo, data_inicio AS DataInicioObj, data_fim AS DataFimObj, opt_in AS OptIn
            FROM funcionario_beneficios
            WHERE funcionario_id = @FuncionarioId
            ORDER BY codigo_beneficio
            """;

        var rows = await _dbConnection.QueryAsync<BeneficioRow>(sql, new { FuncionarioId = funcionarioId });
        return rows.Select(MapearBeneficio).ToArray();
    }

    public async Task<IReadOnlyList<FuncionarioQueryResult>> FiltrarAsync(FuncionarioFiltroParams filtro)
    {
        var sql = new StringBuilder("""
            SELECT id AS Id, nome AS Nome, cpf AS Cpf, matricula AS Matricula,
                data_admissao AS DataAdmissaoObj, data_desligamento AS DataDesligamentoObj,
                cargo AS Cargo, salario_base AS SalarioBase,
                tipo_contrato AS TipoContratoTexto, centro_custo AS CentroCusto,
                res_cep AS ResCep, res_logradouro AS ResLogradouro, res_numero AS ResNumero,
                res_complemento AS ResComplemento, res_bairro AS ResBairro, res_cidade AS ResCidade, res_uf AS ResUf,
                trab_nome_local AS TrabNomeLocal, trab_cep AS TrabCep, trab_logradouro AS TrabLogradouro,
                trab_numero AS TrabNumero, trab_complemento AS TrabComplemento, trab_bairro AS TrabBairro,
                trab_cidade AS TrabCidade, trab_uf AS TrabUf,
                situacao AS SituacaoTexto, motivo_afastamento AS MotivoAfastamento,
                jornada AS JornadaTexto, jornada_detalhe AS JornadaDetalhe,
                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
            FROM funcionarios WHERE 1=1
            """);

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            sql.Append(" AND nome ILIKE @Nome");
        if (!string.IsNullOrWhiteSpace(filtro.Cpf))
            sql.Append(" AND cpf = @Cpf");
        if (!string.IsNullOrWhiteSpace(filtro.Matricula))
            sql.Append(" AND matricula ILIKE @Matricula");
        if (filtro.Situacao is not null)
            sql.Append(" AND situacao = @Situacao");

        sql.Append(" ORDER BY nome");

        var rows = await _dbConnection.QueryAsync<FuncionarioRow>(sql.ToString(), new
        {
            Nome = string.IsNullOrWhiteSpace(filtro.Nome) ? null : $"%{filtro.Nome.Trim()}%",
            Cpf = string.IsNullOrWhiteSpace(filtro.Cpf) ? null : CpfUtil.Normalizar(filtro.Cpf),
            Matricula = string.IsNullOrWhiteSpace(filtro.Matricula) ? null : $"%{filtro.Matricula.Trim()}%",
            Situacao = filtro.Situacao is null ? null : FuncionarioConversao.SituacaoParaBanco(filtro.Situacao.Value),
        });

        return rows.Select(Mapear).ToArray();
    }

    public async Task<bool> CpfExisteAsync(string cpf, Guid? excetoId = null)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM funcionarios
                WHERE cpf = @Cpf AND (@ExcetoId IS NULL OR id <> @ExcetoId))
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Cpf = cpf, ExcetoId = excetoId });
    }

    public async Task<bool> MatriculaExisteAsync(string matricula, Guid? excetoId = null)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM funcionarios
                WHERE matricula = @Matricula AND (@ExcetoId IS NULL OR id <> @ExcetoId))
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Matricula = matricula, ExcetoId = excetoId });
    }

    private async Task SyncBeneficiosAsync(
        Guid funcionarioId,
        IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios,
        IDbTransaction tx)
    {
        await _dbConnection.ExecuteAsync(
            "DELETE FROM funcionario_beneficios WHERE funcionario_id = @FuncionarioId",
            new { FuncionarioId = funcionarioId },
            tx);

        const string insert = """
            INSERT INTO funcionario_beneficios
                (id, funcionario_id, codigo_beneficio, ativo, data_inicio, data_fim, opt_in, data_inclusao)
            VALUES
                (@Id, @FuncionarioId, @CodigoBeneficio, @Ativo, @DataInicio, @DataFim, @OptIn, @DataInclusao)
            """;

        var agora = DateTime.UtcNow;
        foreach (var b in beneficios)
        {
            await _dbConnection.ExecuteAsync(insert, new
            {
                b.Id,
                FuncionarioId = funcionarioId,
                b.CodigoBeneficio,
                b.Ativo,
                DataInicio = b.DataInicio?.ToDateTime(TimeOnly.MinValue),
                DataFim = b.DataFim?.ToDateTime(TimeOnly.MinValue),
                b.OptIn,
                DataInclusao = agora,
            }, tx);
        }
    }

    private void EnsureOpen()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();
    }

    private static object MapearInsert(FuncionarioSalvarParams p) => new
    {
        p.Id,
        p.Nome,
        p.Cpf,
        p.Matricula,
        DataAdmissao = p.DataAdmissao.ToDateTime(TimeOnly.MinValue),
        DataDesligamento = p.DataDesligamento?.ToDateTime(TimeOnly.MinValue),
        p.Cargo,
        p.SalarioBase,
        TipoContrato = FuncionarioConversao.TipoContratoParaBanco(p.TipoContrato),
        p.CentroCusto,
        p.ResCep,
        p.ResLogradouro,
        p.ResNumero,
        p.ResComplemento,
        p.ResBairro,
        p.ResCidade,
        p.ResUf,
        p.TrabNomeLocal,
        p.TrabCep,
        p.TrabLogradouro,
        p.TrabNumero,
        p.TrabComplemento,
        p.TrabBairro,
        p.TrabCidade,
        p.TrabUf,
        Situacao = FuncionarioConversao.SituacaoParaBanco(p.Situacao),
        p.MotivoAfastamento,
        Jornada = FuncionarioConversao.JornadaParaBanco(p.Jornada),
        p.JornadaDetalhe,
        p.DataInclusao,
    };

    private static object MapearUpdate(FuncionarioAtualizarParams p) => new
    {
        p.Id,
        p.Nome,
        p.Cpf,
        p.Matricula,
        DataAdmissao = p.DataAdmissao.ToDateTime(TimeOnly.MinValue),
        DataDesligamento = p.DataDesligamento?.ToDateTime(TimeOnly.MinValue),
        p.Cargo,
        p.SalarioBase,
        TipoContrato = FuncionarioConversao.TipoContratoParaBanco(p.TipoContrato),
        p.CentroCusto,
        p.ResCep,
        p.ResLogradouro,
        p.ResNumero,
        p.ResComplemento,
        p.ResBairro,
        p.ResCidade,
        p.ResUf,
        p.TrabNomeLocal,
        p.TrabCep,
        p.TrabLogradouro,
        p.TrabNumero,
        p.TrabComplemento,
        p.TrabBairro,
        p.TrabCidade,
        p.TrabUf,
        Situacao = FuncionarioConversao.SituacaoParaBanco(p.Situacao),
        p.MotivoAfastamento,
        Jornada = FuncionarioConversao.JornadaParaBanco(p.Jornada),
        p.JornadaDetalhe,
        DataAlteracao = DateTime.UtcNow,
        p.UsuarioAlteracaoId,
    };

    private static FuncionarioQueryResult Mapear(FuncionarioRow row) => new()
    {
        Id = row.Id,
        Nome = row.Nome,
        Cpf = row.Cpf,
        Matricula = row.Matricula,
        DataAdmissao = ToDateOnly(row.DataAdmissaoObj),
        DataDesligamento = ToDateOnlyNullable(row.DataDesligamentoObj),
        Cargo = row.Cargo,
        SalarioBase = row.SalarioBase,
        TipoContrato = FuncionarioConversao.TipoContratoDeBanco(row.TipoContratoTexto),
        CentroCusto = row.CentroCusto,
        ResCep = row.ResCep,
        ResLogradouro = row.ResLogradouro,
        ResNumero = row.ResNumero,
        ResComplemento = row.ResComplemento,
        ResBairro = row.ResBairro,
        ResCidade = row.ResCidade,
        ResUf = row.ResUf,
        TrabNomeLocal = row.TrabNomeLocal,
        TrabCep = row.TrabCep,
        TrabLogradouro = row.TrabLogradouro,
        TrabNumero = row.TrabNumero,
        TrabComplemento = row.TrabComplemento,
        TrabBairro = row.TrabBairro,
        TrabCidade = row.TrabCidade,
        TrabUf = row.TrabUf,
        Situacao = FuncionarioConversao.SituacaoDeBanco(row.SituacaoTexto),
        MotivoAfastamento = row.MotivoAfastamento,
        Jornada = FuncionarioConversao.JornadaDeBanco(row.JornadaTexto),
        JornadaDetalhe = row.JornadaDetalhe,
        DataInclusao = row.DataInclusao,
        DataAlteracao = row.DataAlteracao,
    };

    private static FuncionarioBeneficioQueryResult MapearBeneficio(BeneficioRow row) => new()
    {
        Id = row.Id,
        FuncionarioId = row.FuncionarioId,
        CodigoBeneficio = row.CodigoBeneficio,
        Ativo = row.Ativo,
        DataInicio = ToDateOnlyNullable(row.DataInicioObj),
        DataFim = ToDateOnlyNullable(row.DataFimObj),
        OptIn = row.OptIn,
    };

    private static DateOnly ToDateOnly(object valor) => valor switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => DateOnly.FromDateTime(Convert.ToDateTime(valor)),
    };

    private static DateOnly? ToDateOnlyNullable(object? valor) =>
        valor is null or DBNull ? null : ToDateOnly(valor);

    private sealed class FuncionarioRow
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string? Matricula { get; set; }
        public object DataAdmissaoObj { get; set; } = default!;
        public object? DataDesligamentoObj { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public decimal SalarioBase { get; set; }
        public string TipoContratoTexto { get; set; } = string.Empty;
        public string? CentroCusto { get; set; }
        public string? ResCep { get; set; }
        public string? ResLogradouro { get; set; }
        public string? ResNumero { get; set; }
        public string? ResComplemento { get; set; }
        public string? ResBairro { get; set; }
        public string? ResCidade { get; set; }
        public string? ResUf { get; set; }
        public string? TrabNomeLocal { get; set; }
        public string? TrabCep { get; set; }
        public string? TrabLogradouro { get; set; }
        public string? TrabNumero { get; set; }
        public string? TrabComplemento { get; set; }
        public string? TrabBairro { get; set; }
        public string? TrabCidade { get; set; }
        public string? TrabUf { get; set; }
        public string SituacaoTexto { get; set; } = string.Empty;
        public string? MotivoAfastamento { get; set; }
        public string JornadaTexto { get; set; } = string.Empty;
        public string? JornadaDetalhe { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }

    private sealed class BeneficioRow
    {
        public Guid Id { get; set; }
        public Guid FuncionarioId { get; set; }
        public string CodigoBeneficio { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public object? DataInicioObj { get; set; }
        public object? DataFimObj { get; set; }
        public bool OptIn { get; set; }
    }
}
