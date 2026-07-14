using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Dapper;
using System.Data;

namespace Beneficios.Infrastructure.Repositories;

public class PerfilRepository : IPerfilRepository
{
    private readonly IDbConnection _dbConnection;

    public PerfilRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Guid> SalvarAsync(PerfilSalvarParams perfil)
    {
        const string sqlPerfil = """
            INSERT INTO perfis (id, nome, eh_sistema, data_inclusao)
            VALUES (@Id, @Nome, @EhSistema, @DataInclusao)
            """;

        await _dbConnection.ExecuteAsync(sqlPerfil, new
        {
            perfil.Id,
            perfil.Nome,
            perfil.EhSistema,
            DataInclusao = DateTime.UtcNow,
        });

        await InserirPermissoesAsync(perfil.Id, perfil.Permissoes);
        return perfil.Id;
    }

    public async Task<bool> AtualizarAsync(PerfilAtualizarParams perfil)
    {
        const string sql = """
            UPDATE perfis
            SET nome = @Nome,
                data_alteracao = @DataAlteracao,
                usuario_alteracao_id = @UsuarioAlteracaoId
            WHERE id = @Id
            """;

        var rows = await _dbConnection.ExecuteAsync(sql, new
        {
            perfil.Id,
            perfil.Nome,
            DataAlteracao = DateTime.UtcNow,
            perfil.UsuarioAlteracaoId,
        });

        if (rows == 0)
            return false;

        await _dbConnection.ExecuteAsync(
            "DELETE FROM perfil_permissoes WHERE perfil_id = @Id",
            new { perfil.Id });

        await InserirPermissoesAsync(perfil.Id, perfil.Permissoes);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rows = await _dbConnection.ExecuteAsync(
            "DELETE FROM perfis WHERE id = @Id",
            new { Id = id });
        return rows > 0;
    }

    public async Task<PerfilQueryResult?> ObterUmAsync(Guid id)
    {
        const string sql = """
            SELECT
                id AS Id,
                nome AS Nome,
                eh_sistema AS EhSistema,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM perfis
            WHERE id = @Id
            """;

        var perfil = await _dbConnection.QueryFirstOrDefaultAsync<PerfilQueryResult>(sql, new { Id = id });
        if (perfil is null)
            return null;

        perfil.Permissoes = (await ObterPermissoesDoPerfilAsync(id)).ToList();
        return perfil;
    }

    public async Task<PerfilQueryResult[]> ObterTodosAsync()
    {
        const string sql = """
            SELECT
                id AS Id,
                nome AS Nome,
                eh_sistema AS EhSistema,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM perfis
            ORDER BY nome
            """;

        var perfis = (await _dbConnection.QueryAsync<PerfilQueryResult>(sql)).ToArray();
        foreach (var perfil in perfis)
            perfil.Permissoes = (await ObterPermissoesDoPerfilAsync(perfil.Id)).ToList();

        return perfis;
    }

    public async Task<PerfilQueryResult[]> FiltrarAsync(PerfilFiltroParams filtro)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            conditions.Add("nome ILIKE @Nome");
            parameters.Add("Nome", $"%{filtro.Nome.Trim()}%");
        }

        var where = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;

        var sql = $"""
            SELECT
                id AS Id,
                nome AS Nome,
                eh_sistema AS EhSistema,
                data_inclusao AS DataInclusao,
                data_alteracao AS DataAlteracao
            FROM perfis
            {where}
            ORDER BY nome
            """;

        var perfis = (await _dbConnection.QueryAsync<PerfilQueryResult>(sql, parameters)).ToArray();
        foreach (var perfil in perfis)
            perfil.Permissoes = (await ObterPermissoesDoPerfilAsync(perfil.Id)).ToList();

        return perfis;
    }

    public async Task<bool> EstaEmUsoAsync(Guid id)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM usuarios WHERE perfil_id = @Id
            )
            """;
        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public async Task<PerfilPermissaoParams[]> ObterPermissoesPorUsuarioAsync(Guid usuarioId)
    {
        const string sql = """
            SELECT
                pp.id AS Id,
                pp.perfil_id AS PerfilId,
                pp.codigo_menu AS CodigoMenu,
                pp.visualizar AS Visualizar,
                pp.criar AS Criar,
                pp.editar AS Editar,
                pp.excluir AS Excluir
            FROM usuarios u
            INNER JOIN perfil_permissoes pp ON pp.perfil_id = u.perfil_id
            WHERE u.id = @UsuarioId
            """;

        var result = await _dbConnection.QueryAsync<PerfilPermissaoParams>(sql, new { UsuarioId = usuarioId });
        return result.ToArray();
    }

    private async Task InserirPermissoesAsync(Guid perfilId, IReadOnlyList<PerfilPermissaoParams> permissoes)
    {
        const string sql = """
            INSERT INTO perfil_permissoes
                (id, perfil_id, codigo_menu, visualizar, criar, editar, excluir)
            VALUES
                (@Id, @PerfilId, @CodigoMenu, @Visualizar, @Criar, @Editar, @Excluir)
            """;

        foreach (var permissao in permissoes)
        {
            await _dbConnection.ExecuteAsync(sql, new
            {
                Id = permissao.Id == Guid.Empty ? Guid.NewGuid() : permissao.Id,
                PerfilId = perfilId,
                permissao.CodigoMenu,
                permissao.Visualizar,
                permissao.Criar,
                permissao.Editar,
                permissao.Excluir,
            });
        }
    }

    private async Task<IEnumerable<PerfilPermissaoParams>> ObterPermissoesDoPerfilAsync(Guid perfilId)
    {
        const string sql = """
            SELECT
                id AS Id,
                perfil_id AS PerfilId,
                codigo_menu AS CodigoMenu,
                visualizar AS Visualizar,
                criar AS Criar,
                editar AS Editar,
                excluir AS Excluir
            FROM perfil_permissoes
            WHERE perfil_id = @PerfilId
            ORDER BY codigo_menu
            """;

        return await _dbConnection.QueryAsync<PerfilPermissaoParams>(sql, new { PerfilId = perfilId });
    }
}
