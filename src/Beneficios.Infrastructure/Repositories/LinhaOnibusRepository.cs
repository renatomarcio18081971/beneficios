using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Text;

namespace Beneficios.Infrastructure.Repositories;

public class LinhaOnibusRepository : ILinhaOnibusRepository
{
    private readonly IDbConnection _dbConnection;

    public LinhaOnibusRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            INSERT INTO linhas_onibus (
                id, descricao, data_inicio, data_fim, valor_tarifa,
                data_inclusao, usuario_alteracao_id)
            VALUES (
                @Id, @Descricao, @DataInicio, @DataFim, @ValorTarifa,
                @DataInclusao, @UsuarioAlteracaoId)
            """;
        await _dbConnection.ExecuteAsync(sql, MapearInsert(parametros));
        return parametros.Id;
    }

    public async Task AtualizarAsync(LinhaOnibusAtualizarParams parametros)
    {
        EnsureOpen();
        const string sql = """
            UPDATE linhas_onibus SET
                descricao = @Descricao,
                data_inicio = @DataInicio,
                data_fim = @DataFim,
                valor_tarifa = @ValorTarifa,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;
        var rows = await _dbConnection.ExecuteAsync(sql, MapearUpdate(parametros));
        if (rows == 0)
            throw new InvalidOperationException("Linha não encontrada.");
    }

    public async Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id)
    {
        const string sql = """
            SELECT id AS Id, descricao AS Descricao,
                data_inicio AS DataInicioObj, data_fim AS DataFimObj,
                valor_tarifa AS ValorTarifa,
                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
            FROM linhas_onibus
            WHERE id = @Id
            """;
        var row = await _dbConnection.QueryFirstOrDefaultAsync<LinhaRow>(sql, new { Id = id });
        return row is null ? null : Mapear(row);
    }

    public async Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro)
    {
        var sql = new StringBuilder("""
            SELECT id AS Id, descricao AS Descricao,
                data_inicio AS DataInicioObj, data_fim AS DataFimObj,
                valor_tarifa AS ValorTarifa,
                data_inclusao AS DataInclusao, data_alteracao AS DataAlteracao
            FROM linhas_onibus
            WHERE 1=1
            """);

        if (!string.IsNullOrWhiteSpace(filtro.Descricao))
            sql.Append(" AND descricao ILIKE @Descricao");

        if (filtro.SomenteVigentes == true && filtro.Referencia is not null)
        {
            sql.Append("""
                 AND data_inicio <= @Referencia
                 AND (data_fim IS NULL OR data_fim >= @Referencia)
                """);
        }

        sql.Append(" ORDER BY descricao");

        try
        {
            var rows = await _dbConnection.QueryAsync<LinhaRow>(sql.ToString(), new
            {
                Descricao = string.IsNullOrWhiteSpace(filtro.Descricao)
                    ? null
                    : $"%{filtro.Descricao.Trim()}%",
                Referencia = filtro.Referencia?.ToDateTime(TimeOnly.MinValue),
            });
            return rows.Select(Mapear).ToArray();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return Array.Empty<LinhaOnibusQueryResult>();
        }
    }

    private void EnsureOpen()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();
    }

    private static object MapearInsert(LinhaOnibusSalvarParams p) => new
    {
        p.Id,
        p.Descricao,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.ValorTarifa,
        p.DataInclusao,
        p.UsuarioAlteracaoId,
    };

    private static object MapearUpdate(LinhaOnibusAtualizarParams p) => new
    {
        p.Id,
        p.Descricao,
        DataInicio = p.DataInicio.ToDateTime(TimeOnly.MinValue),
        DataFim = p.DataFim?.ToDateTime(TimeOnly.MinValue),
        p.ValorTarifa,
        p.DataAlteracao,
        p.UsuarioAlteracaoId,
    };

    private static LinhaOnibusQueryResult Mapear(LinhaRow row) => new()
    {
        Id = row.Id,
        Descricao = row.Descricao,
        DataInicio = ToDateOnly(row.DataInicioObj),
        DataFim = ToDateOnlyNullable(row.DataFimObj),
        ValorTarifa = row.ValorTarifa,
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

    private sealed class LinhaRow
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = "";
        public object DataInicioObj { get; set; } = default!;
        public object? DataFimObj { get; set; }
        public decimal ValorTarifa { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
