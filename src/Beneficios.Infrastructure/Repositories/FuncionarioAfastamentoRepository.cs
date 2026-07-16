using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Text;

namespace Beneficios.Infrastructure.Repositories;

public class FuncionarioAfastamentoRepository : IFuncionarioAfastamentoRepository
{
    private readonly IDbConnection _dbConnection;

    public FuncionarioAfastamentoRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(FuncionarioAfastamentoSalvarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            INSERT INTO funcionario_afastamentos (
                id, funcionario_id, tipo, data_inicio, data_fim, observacao,
                data_inclusao, usuario_alteracao_id)
            VALUES (
                @Id, @FuncionarioId, @Tipo, @DataInicio, @DataFim, @Observacao,
                @DataInclusao, @UsuarioAlteracaoId)
            """;
        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros));
        return parametros.Id;
    }

    public async Task AtualizarAsync(FuncionarioAfastamentoAtualizarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            UPDATE funcionario_afastamentos SET
                funcionario_id = @FuncionarioId,
                tipo = @Tipo,
                data_inicio = @DataInicio,
                data_fim = @DataFim,
                observacao = @Observacao,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;
        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros));
        if (rows == 0)
            throw new InvalidOperationException("Afastamento não encontrado.");
    }

    public async Task ExcluirAsync(Guid id)
    {
        EnsureOpen();
        var rows = await _dbConnection.ExecuteAsync(
            "DELETE FROM funcionario_afastamentos WHERE id = @Id", new { Id = id });
        if (rows == 0)
            throw new InvalidOperationException("Afastamento não encontrado.");
    }

    public async Task<FuncionarioAfastamentoQueryResult?> ObterPorIdAsync(Guid id)
    {
        const string sql = """
            SELECT a.id AS Id, a.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
                a.tipo AS Tipo, a.data_inicio AS DataInicioObj, a.data_fim AS DataFimObj,
                a.observacao AS Observacao
            FROM funcionario_afastamentos a
            INNER JOIN funcionarios f ON f.id = a.funcionario_id
            WHERE a.id = @Id
            """;
        var row = await _dbConnection.QueryFirstOrDefaultAsync<AfastamentoRow>(sql, new { Id = id });
        return row is null ? null : Mapear(row);
    }

    public async Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> FiltrarAsync(
        FuncionarioAfastamentoFiltroParams filtro)
    {
        var sql = new StringBuilder("""
            SELECT a.id AS Id, a.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
                a.tipo AS Tipo, a.data_inicio AS DataInicioObj, a.data_fim AS DataFimObj,
                a.observacao AS Observacao
            FROM funcionario_afastamentos a
            INNER JOIN funcionarios f ON f.id = a.funcionario_id
            WHERE 1=1
            """);

        if (filtro.FuncionarioId is not null)
            sql.Append(" AND a.funcionario_id = @FuncionarioId");
        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
            sql.Append(" AND a.tipo = @Tipo");
        if (filtro.DataInicio is not null || filtro.DataFim is not null)
        {
            sql.Append("""
                 AND a.data_inicio <= COALESCE(@DataFimFiltro, DATE '9999-12-31')
                 AND COALESCE(a.data_fim, DATE '9999-12-31') >= COALESCE(@DataInicioFiltro, DATE '0001-01-01')
                """);
        }

        sql.Append(" ORDER BY a.data_inicio DESC, f.nome");

        try
        {
            var rows = await _dbConnection.QueryAsync<AfastamentoRow>(sql.ToString(), new
            {
                filtro.FuncionarioId,
                Tipo = filtro.Tipo,
                DataInicioFiltro = filtro.DataInicio?.ToDateTime(TimeOnly.MinValue),
                DataFimFiltro = filtro.DataFim?.ToDateTime(TimeOnly.MinValue),
            });
            return rows.Select(Mapear).ToArray();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return Array.Empty<FuncionarioAfastamentoQueryResult>();
        }
    }

    public async Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> ListarPorFuncionarioAsync(Guid funcionarioId)
    {
        return await FiltrarAsync(new FuncionarioAfastamentoFiltroParams { FuncionarioId = funcionarioId });
    }

    public async Task<bool> ExisteSobreposicaoAsync(
        Guid funcionarioId, DateOnly dataInicio, DateOnly? dataFim, Guid? excetoId = null)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM funcionario_afastamentos a
                WHERE a.funcionario_id = @FuncionarioId
                  AND (@ExcetoId IS NULL OR a.id <> @ExcetoId)
                  AND a.data_inicio <= COALESCE(@DataFim, DATE '9999-12-31')
                  AND @DataInicio <= COALESCE(a.data_fim, DATE '9999-12-31'))
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
        {
            FuncionarioId = funcionarioId,
            DataInicio = dataInicio.ToDateTime(TimeOnly.MinValue),
            DataFim = dataFim?.ToDateTime(TimeOnly.MinValue),
            ExcetoId = excetoId,
        });
    }

    public async Task<FuncionarioAfastamentoQueryResult?> ObterAtivoEmAsync(Guid funcionarioId, DateOnly referencia)
    {
        const string sql = """
            SELECT a.id AS Id, a.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
                a.tipo AS Tipo, a.data_inicio AS DataInicioObj, a.data_fim AS DataFimObj,
                a.observacao AS Observacao
            FROM funcionario_afastamentos a
            INNER JOIN funcionarios f ON f.id = a.funcionario_id
            WHERE a.funcionario_id = @FuncionarioId
              AND a.data_inicio <= @Referencia
              AND (a.data_fim IS NULL OR a.data_fim >= @Referencia)
            LIMIT 1
            """;
        var row = await _dbConnection.QueryFirstOrDefaultAsync<AfastamentoRow>(sql, new
        {
            FuncionarioId = funcionarioId,
            Referencia = referencia.ToDateTime(TimeOnly.MinValue),
        });
        return row is null ? null : Mapear(row);
    }

    private void EnsureOpen()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();
    }

    private static object MapearInsert(FuncionarioAfastamentoSalvarParams p) => new
    {
        p.Id,
        p.FuncionarioId,
        p.Tipo,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.Observacao,
        p.DataInclusao,
        p.UsuarioAlteracaoId,
    };

    private static object MapearUpdate(FuncionarioAfastamentoAtualizarParams p) => new
    {
        p.Id,
        p.FuncionarioId,
        p.Tipo,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.Observacao,
        p.DataAlteracao,
        p.UsuarioAlteracaoId,
    };

    private static FuncionarioAfastamentoQueryResult Mapear(AfastamentoRow row) => new()
    {
        Id = row.Id,
        FuncionarioId = row.FuncionarioId,
        FuncionarioNome = row.FuncionarioNome,
        Tipo = row.Tipo,
        DataInicio = ToDateOnly(row.DataInicioObj),
        DataFim = ToDateOnlyNullable(row.DataFimObj),
        Observacao = row.Observacao,
    };

    private static DateOnly ToDateOnly(object valor) => valor switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => DateOnly.FromDateTime(Convert.ToDateTime(valor)),
    };

    private static DateOnly? ToDateOnlyNullable(object? valor) =>
        valor is null or DBNull ? null : ToDateOnly(valor);

    private sealed class AfastamentoRow
    {
        public Guid Id { get; set; }
        public Guid FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public object DataInicioObj { get; set; } = default!;
        public object? DataFimObj { get; set; }
        public string? Observacao { get; set; }
    }
}