using Beneficios.Domain;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public static class CalendarioAnoGerador
{
    public static async Task GerarSeAusenteAsync(
        IDbConnection connection,
        string schemaName,
        int ano,
        CancellationToken cancellationToken = default)
    {
        var quoted = TenantSchemaSql.CitarIdentificador(schemaName);
        var existe = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            $"""
            SELECT EXISTS(
                SELECT 1 FROM {quoted}.calendario_dias
                WHERE EXTRACT(YEAR FROM data) = @Ano
            )
            """,
            new { Ano = ano },
            cancellationToken: cancellationToken));

        if (existe)
            return;

        var dias = CalendarioAnoFabrica.MontarDiasDoAno(ano);
        const string insertTemplate = """
            INSERT INTO {0}.calendario_dias
                (id, data, eh_dia_util, tipo_excecao, origem, observacao, data_inclusao)
            VALUES
                (@Id, @Data, @EhDiaUtil, @TipoExcecao, @Origem, @Observacao, @DataInclusao)
            """;
        var sql = string.Format(insertTemplate, quoted);
        var agora = DateTime.UtcNow;

        foreach (var dia in dias)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    dia.Id,
                    Data = dia.Data.ToDateTime(TimeOnly.MinValue),
                    dia.EhDiaUtil,
                    TipoExcecao = CalendarioDiaConversao.TipoExcecaoParaBanco(dia.TipoExcecao),
                    Origem = CalendarioDiaConversao.OrigemParaBanco(dia.Origem),
                    dia.Observacao,
                    DataInclusao = agora,
                },
                cancellationToken: cancellationToken));
        }
    }
}
