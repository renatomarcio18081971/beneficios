using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Text;

namespace Beneficios.Infrastructure.Repositories;

public class FuncionarioLinhaRepository : IFuncionarioLinhaRepository
{
    private readonly IDbConnection _dbConnection;

    public FuncionarioLinhaRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            INSERT INTO funcionario_linhas (
                id, funcionario_id, linha_onibus_id, quantidade,
                data_inicio, data_fim, data_inclusao, usuario_alteracao_id)
            VALUES (
                @Id, @FuncionarioId, @LinhaOnibusId, @Quantidade,
                @DataInicio, @DataFim, @DataInclusao, @UsuarioAlteracaoId)
            """;
        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros));
        return parametros.Id;
    }

    public async Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            UPDATE funcionario_linhas SET
                linha_onibus_id = @LinhaOnibusId,
                quantidade = @Quantidade,
                data_inicio = @DataInicio,
                data_fim = @DataFim,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;
        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros));
        if (rows == 0)
            throw new InvalidOperationException("Vínculo não encontrado.");
    }

    public async Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id)
    {
        const string sql = """
            SELECT fl.id AS Id, fl.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
                fl.linha_onibus_id AS LinhaOnibusId, l.descricao AS LinhaDescricao,
                fl.quantidade AS Quantidade,
                fl.data_inicio AS DataInicioObj, fl.data_fim AS DataFimObj,
                fl.data_inclusao AS DataInclusao, fl.data_alteracao AS DataAlteracao
            FROM funcionario_linhas fl
            INNER JOIN funcionarios f ON f.id = fl.funcionario_id
            INNER JOIN linhas_onibus l ON l.id = fl.linha_onibus_id
            WHERE fl.id = @Id
            """;
        var row = await _dbConnection.QueryFirstOrDefaultAsync<VinculoRow>(sql, new { Id = id });
        return row is null ? null : Mapear(row);
    }

    public async Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro)
    {
        var sql = new StringBuilder("""
            SELECT fl.id AS Id, fl.funcionario_id AS FuncionarioId, f.nome AS FuncionarioNome,
                fl.linha_onibus_id AS LinhaOnibusId, l.descricao AS LinhaDescricao,
                fl.quantidade AS Quantidade,
                fl.data_inicio AS DataInicioObj, fl.data_fim AS DataFimObj,
                fl.data_inclusao AS DataInclusao, fl.data_alteracao AS DataAlteracao
            FROM funcionario_linhas fl
            INNER JOIN funcionarios f ON f.id = fl.funcionario_id
            INNER JOIN linhas_onibus l ON l.id = fl.linha_onibus_id
            WHERE 1=1
            """);

        if (filtro.FuncionarioId is not null)
            sql.Append(" AND fl.funcionario_id = @FuncionarioId");

        if (filtro.SomenteVigentes == true && filtro.Referencia is not null)
        {
            sql.Append("""
                 AND fl.data_inicio <= @Referencia
                 AND (fl.data_fim IS NULL OR fl.data_fim >= @Referencia)
                """);
        }

        sql.Append(" ORDER BY f.nome, l.descricao");

        try
        {
            var rows = await _dbConnection.QueryAsync<VinculoRow>(sql.ToString(), new
            {
                filtro.FuncionarioId,
                Referencia = filtro.Referencia?.ToDateTime(TimeOnly.MinValue),
            });
            return rows.Select(Mapear).ToArray();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return Array.Empty<FuncionarioLinhaQueryResult>();
        }
    }

    public async Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM funcionario_linhas
                WHERE funcionario_id = @FuncionarioId
                  AND linha_onibus_id = @LinhaOnibusId
                  AND (@ExcetoId IS NULL OR id <> @ExcetoId))
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
        {
            FuncionarioId = funcionarioId,
            LinhaOnibusId = linhaOnibusId,
            ExcetoId = excetoId,
        });
    }

    public async Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM funcionario_linhas
                WHERE linha_onibus_id = @LinhaOnibusId
                  AND data_fim IS NULL)
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { LinhaOnibusId = linhaOnibusId });
    }

    public async Task EncerrarAbertosPorFuncionarioAsync(
        Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId)
    {
        EnsureOpen();
        const string sql = """
            UPDATE funcionario_linhas
            SET data_fim = @DataFim, data_alteracao = @DataAlteracao, usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE funcionario_id = @FuncionarioId AND data_fim IS NULL
            """;
        await _dbConnection.ExecuteAsync(sql, new
        {
            FuncionarioId = funcionarioId,
            DataFim = dataFim.ToDateTime(TimeOnly.MinValue),
            DataAlteracao = dataAlteracao,
            UsuarioAlteracaoId = usuarioAlteracaoId,
        });
    }

    private void EnsureOpen()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();
    }

    private static object MapearInsert(FuncionarioLinhaSalvarParams p) => new
    {
        p.Id,
        p.FuncionarioId,
        p.LinhaOnibusId,
        p.Quantidade,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.DataInclusao,
        p.UsuarioAlteracaoId,
    };

    private static object MapearUpdate(FuncionarioLinhaAtualizarParams p) => new
    {
        p.Id,
        p.LinhaOnibusId,
        p.Quantidade,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.DataAlteracao,
        p.UsuarioAlteracaoId,
    };

    private static FuncionarioLinhaQueryResult Mapear(VinculoRow row) => new()
    {
        Id = row.Id,
        FuncionarioId = row.FuncionarioId,
        FuncionarioNome = row.FuncionarioNome,
        LinhaOnibusId = row.LinhaOnibusId,
        LinhaDescricao = row.LinhaDescricao,
        Quantidade = row.Quantidade,
        DataInicio = ToDateOnly(row.DataInicioObj),
        DataFim = ToDateOnlyNullable(row.DataFimObj),
        DataInclusao = row.DataInclusao,
        DataAlteracao = row.DataAlteracao,
    };

    private static DateOnly ToDateOnly(object valor) => valor switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => DateOnly.FromDateTime(Convert.ToDateTime(valor)),
    };

    private static DateOnly? ToDateOnlyNullable(object? valor) =>
        valor is null or DBNull ? null : ToDateOnly(valor);

    private sealed class VinculoRow
    {
        public Guid Id { get; set; }
        public Guid FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = "";
        public Guid LinhaOnibusId { get; set; }
        public string LinhaDescricao { get; set; } = "";
        public int Quantidade { get; set; }
        public object DataInicioObj { get; set; } = default!;
        public object? DataFimObj { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
