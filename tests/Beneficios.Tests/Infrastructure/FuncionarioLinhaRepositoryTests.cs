using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class FuncionarioLinhaRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Funcionario Linha Repo";

    [SkippableFact]
    public async Task Salvar_Unicidade_Encerrar_VinculoAberto_DeveFuncionar()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var conn = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var funcRepo = new FuncionarioRepository(conn);
        var linhaRepo = new LinhaOnibusRepository(conn);
        var vinculoRepo = new FuncionarioLinhaRepository(conn);

        var funcionarioId = Guid.NewGuid();
        await funcRepo.SalvarAsync(new FuncionarioSalvarParams
        {
            Id = funcionarioId,
            Nome = "Maria Linhas",
            Cpf = "39053344705",
            DataAdmissao = new DateOnly(2024, 1, 1),
            Cargo = "Analista",
            SalarioBase = 2000m,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataInclusao = DateTime.UtcNow,
        }, []);

        var linhaId = Guid.NewGuid();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        await linhaRepo.SalvarAsync(new LinhaOnibusSalvarParams
        {
            Id = linhaId,
            Descricao = "Linha 42",
            DataInicio = hoje.AddDays(-10),
            DataFim = null,
            ValorTarifa = 3.80m,
            DataInclusao = DateTime.UtcNow,
        });

        var vinculoId = Guid.NewGuid();
        await vinculoRepo.SalvarAsync(new FuncionarioLinhaSalvarParams
        {
            Id = vinculoId,
            FuncionarioId = funcionarioId,
            LinhaOnibusId = linhaId,
            Quantidade = 2,
            DataInicio = hoje.AddDays(-5),
            DataFim = null,
            DataInclusao = DateTime.UtcNow,
        });

        Assert.True(await vinculoRepo.ExisteParAsync(funcionarioId, linhaId));
        Assert.False(await vinculoRepo.ExisteParAsync(funcionarioId, Guid.NewGuid()));
        Assert.True(await vinculoRepo.ExisteVinculoAbertoPorLinhaAsync(linhaId));

        var lista = await vinculoRepo.FiltrarAsync(new FuncionarioLinhaFiltroParams
        {
            FuncionarioId = funcionarioId,
            SomenteVigentes = true,
            Referencia = hoje,
        });
        Assert.Single(lista);
        Assert.Equal("Maria Linhas", lista[0].FuncionarioNome);
        Assert.Equal("Linha 42", lista[0].LinhaDescricao);
        Assert.Equal(2, lista[0].Quantidade);

        var detalhe = await vinculoRepo.ObterPorIdAsync(vinculoId);
        Assert.NotNull(detalhe);
        Assert.Equal(funcionarioId, detalhe!.FuncionarioId);

        await vinculoRepo.AtualizarAsync(new FuncionarioLinhaAtualizarParams
        {
            Id = vinculoId,
            LinhaOnibusId = linhaId,
            Quantidade = 1,
            DataInicio = hoje.AddDays(-5),
            DataFim = null,
            DataAlteracao = DateTime.UtcNow,
        });
        Assert.Equal(1, (await vinculoRepo.ObterPorIdAsync(vinculoId))!.Quantidade);

        await vinculoRepo.EncerrarAbertosPorFuncionarioAsync(
            funcionarioId, hoje, DateTime.UtcNow, Guid.NewGuid());

        Assert.False(await vinculoRepo.ExisteVinculoAbertoPorLinhaAsync(linhaId));
        var encerrado = await vinculoRepo.ObterPorIdAsync(vinculoId);
        Assert.Equal(hoje, encerrado!.DataFim);

        var vigentesApos = await vinculoRepo.FiltrarAsync(new FuncionarioLinhaFiltroParams
        {
            FuncionarioId = funcionarioId,
            SomenteVigentes = true,
            Referencia = hoje,
        });
        // data_fim = hoje ainda conta como vigente (data_fim >= referencia)
        Assert.Single(vigentesApos);
    }
}
