using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Repositories;

public class CalendarioDiaRepository : ICalendarioDiaRepository
{
    private readonly IDbConnection _dbConnection;

    public CalendarioDiaRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> AnoExisteAsync(int ano)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM calendario_dias
                WHERE EXTRACT(YEAR FROM data) = @Ano
            )
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Ano = ano });
    }

    public async Task InserirLoteAsync(IReadOnlyList<CalendarioDiaSalvarParams> dias)
    {
        const string sql = """
            INSERT INTO calendario_dias
                (id, data, eh_dia_util, tipo_excecao, origem, observacao, data_inclusao)
            VALUES
                (@Id, @Data, @EhDiaUtil, @TipoExcecao, @Origem, @Observacao, @DataInclusao)
            """;

        var agora = DateTime.UtcNow;
        foreach (var dia in dias)
        {
            await _dbConnection.ExecuteAsync(sql, new
            {
                dia.Id,
                Data = dia.Data.ToDateTime(TimeOnly.MinValue),
                dia.EhDiaUtil,
                TipoExcecao = CalendarioDiaConversao.TipoExcecaoParaBanco(dia.TipoExcecao),
                Origem = CalendarioDiaConversao.OrigemParaBanco(dia.Origem),
                dia.Observacao,
                DataInclusao = agora,
            });
        }
    }

    public async Task<CalendarioDiaQueryResult[]> ObterPorMesAsync(int ano, int mes)
    {
        const string sql = """
            SELECT
                id AS Id,
                data AS Data,
                eh_dia_util AS EhDiaUtil,
                tipo_excecao AS TipoExcecaoTexto,
                origem AS OrigemTexto,
                observacao AS Observacao,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM calendario_dias
            WHERE EXTRACT(YEAR FROM data) = @Ano
              AND EXTRACT(MONTH FROM data) = @Mes
            ORDER BY data
            """;

        var rows = await _dbConnection.QueryAsync<CalendarioDiaRow>(sql, new { Ano = ano, Mes = mes });
        return rows.Select(Mapear).ToArray();
    }

    public async Task<CalendarioDiaQueryResult?> ObterPorIdAsync(Guid id)
    {
        const string sql = """
            SELECT
                id AS Id,
                data AS Data,
                eh_dia_util AS EhDiaUtil,
                tipo_excecao AS TipoExcecaoTexto,
                origem AS OrigemTexto,
                observacao AS Observacao,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM calendario_dias
            WHERE id = @Id
            """;

        var row = await _dbConnection.QueryFirstOrDefaultAsync<CalendarioDiaRow>(sql, new { Id = id });
        return row is null ? null : Mapear(row);
    }

    public async Task<bool> AtualizarAsync(CalendarioDiaAtualizarParams parametros)
    {
        const string sql = """
            UPDATE calendario_dias
            SET eh_dia_util = @EhDiaUtil,
                tipo_excecao = @TipoExcecao,
                origem = @Origem,
                observacao = @Observacao,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;

        var rows = await _dbConnection.ExecuteAsync(sql, new
        {
            parametros.Id,
            parametros.EhDiaUtil,
            TipoExcecao = CalendarioDiaConversao.TipoExcecaoParaBanco(parametros.TipoExcecao),
            Origem = CalendarioDiaConversao.OrigemParaBanco(parametros.Origem),
            parametros.Observacao,
            DataAlteracao = DateTime.UtcNow,
            parametros.UsuarioAlteracaoId,
        });
        return rows > 0;
    }

    private static CalendarioDiaQueryResult Mapear(CalendarioDiaRow row)
    {
        DateOnly data = row.Data switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => DateOnly.FromDateTime(Convert.ToDateTime(row.Data)),
        };

        return new CalendarioDiaQueryResult
        {
            Id = row.Id,
            Data = data,
            EhDiaUtil = row.EhDiaUtil,
            TipoExcecao = CalendarioDiaConversao.TipoExcecaoDeBanco(row.TipoExcecaoTexto),
            Origem = CalendarioDiaConversao.OrigemDeBanco(row.OrigemTexto),
            Observacao = row.Observacao,
            DataInclusao = row.DataInclusao,
            DataAlteracao = row.DataAlteracao,
        };
    }

    private sealed class CalendarioDiaRow
    {
        public Guid Id { get; set; }
        public object Data { get; set; } = default!;
        public bool EhDiaUtil { get; set; }
        public string? TipoExcecaoTexto { get; set; }
        public string OrigemTexto { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
