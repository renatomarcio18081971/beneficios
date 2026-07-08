using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly IDbConnection _dbConnection;

    public EmpresaRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(EmpresaSalvarParams empresa)
    {
        var sql = @"
            INSERT INTO empresas (id, razao_social, dominio, data_inclusao)
            VALUES (@Id, @RazaoSocial, @Dominio, @DataInclusao)";

        await _dbConnection.ExecuteAsync(sql, new
        {
            empresa.Id,
            empresa.RazaoSocial,
            empresa.Dominio,
            DataInclusao = DateTime.UtcNow
        });

        return empresa.Id;
    }

    public async Task<bool> AtualizarAsync(EmpresaAtualizarParams empresa)
    {
        var sql = @"
            UPDATE empresas
            SET razao_social = @RazaoSocial,
                dominio = @Dominio,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id";

        var rowsAffected = await _dbConnection.ExecuteAsync(sql, new
        {
            empresa.Id,
            empresa.RazaoSocial,
            empresa.Dominio,
            DataAlteracao = DateTime.UtcNow,
            empresa.UsuarioAlteracaoId
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM empresas WHERE id = @Id";
        var rowsAffected = await _dbConnection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<EmpresaQueryResult?> ObterUmAsync(Guid id)
    {
        var sql = @"
            SELECT 
                id AS Id,
                razao_social AS RazaoSocial,
                dominio AS Dominio,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM empresas
            WHERE id = @Id";

        return await _dbConnection.QueryFirstOrDefaultAsync<EmpresaQueryResult>(sql, new { Id = id });
    }

    public async Task<EmpresaQueryResult[]> ObterTodosAsync()
    {
        var sql = @"
            SELECT 
                id AS Id,
                razao_social AS RazaoSocial,
                dominio AS Dominio,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM empresas
            ORDER BY razao_social";

        var result = await _dbConnection.QueryAsync<EmpresaQueryResult>(sql);
        return result.ToArray();
    }
}
