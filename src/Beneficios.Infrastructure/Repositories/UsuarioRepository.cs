using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnection _dbConnection;

    public UsuarioRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> CreateAsync(UsuarioCreateParams usuario)
    {
        var sql = @"
            INSERT INTO usuarios (id, nome, senha, email, empresa_id, data_inclusao)
            VALUES (@Id, @Nome, @Senha, @Email, @EmpresaId, @DataInclusao)";

        await _dbConnection.ExecuteAsync(sql, new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Senha,
            usuario.Email,
            usuario.EmpresaId,
            DataInclusao = DateTime.UtcNow
        });

        return usuario.Id;
    }

    public async Task<bool> UpdateAsync(UsuarioUpdateParams usuario)
    {
        var sql = @"
            UPDATE usuarios
            SET nome = @Nome,
                email = @Email,
                empresa_id = @EmpresaId,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id";

        var rowsAffected = await _dbConnection.ExecuteAsync(sql, new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.EmpresaId,
            DataAlteracao = DateTime.UtcNow,
            usuario.UsuarioAlteracaoId
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sql = "DELETE FROM usuarios WHERE id = @Id";
        var rowsAffected = await _dbConnection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<UsuarioQueryResult?> GetByIdAsync(Guid id)
    {
        var sql = @"
            SELECT 
                u.id AS Id,
                u.nome AS Nome,
                u.email AS Email,
                u.empresa_id AS EmpresaId,
                e.razao_social AS EmpresaNome,
                u.data_inclusao AS DataInclusao,
                u.data_alteracao AS DataAlteracao
            FROM usuarios u
            LEFT JOIN empresas e ON u.empresa_id = e.id
            WHERE u.id = @Id";

        return await _dbConnection.QueryFirstOrDefaultAsync<UsuarioQueryResult>(sql, new { Id = id });
    }

    public async Task<UsuarioQueryResult[]> GetAllAsync()
    {
        var sql = @"
            SELECT 
                u.id AS Id,
                u.nome AS Nome,
                u.email AS Email,
                u.empresa_id AS EmpresaId,
                e.razao_social AS EmpresaNome,
                u.data_inclusao AS DataInclusao,
                u.data_alteracao AS DataAlteracao
            FROM usuarios u
            LEFT JOIN empresas e ON u.empresa_id = e.id
            ORDER BY u.nome";

        var result = await _dbConnection.QueryAsync<UsuarioQueryResult>(sql);
        return result.ToArray();
    }

    public async Task<UsuarioAuthResult?> GetByEmailAsync(string email)
    {
        var sql = @"
            SELECT 
                u.id AS Id,
                u.nome AS Nome,
                u.email AS Email,
                u.senha AS Senha,
                u.empresa_id AS EmpresaId,
                u.token AS Token
            FROM usuarios u
            WHERE u.email = @Email";

        return await _dbConnection.QueryFirstOrDefaultAsync<UsuarioAuthResult>(sql, new { Email = email });
    }

    public async Task<bool> UpdateTokenAsync(Guid id, string token)
    {
        var sql = "UPDATE usuarios SET token = @Token WHERE id = @Id";
        var rowsAffected = await _dbConnection.ExecuteAsync(sql, new { Id = id, Token = token });
        return rowsAffected > 0;
    }
}
