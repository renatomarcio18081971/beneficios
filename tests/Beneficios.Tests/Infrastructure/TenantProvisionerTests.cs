using Beneficios.Domain.Models;
using Beneficios.Domain;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Beneficios.Infrastructure.Tenancy;
using Dapper;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class TenantProvisionerTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task ProvisionarAsync_DeveCriarSchemaTabelaEUsuarioPadrao()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = Guid.NewGuid();
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Nova Empresa LTDA",
            Dominio = "novaempresa",
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.CatalogConnectionString,
            })
            .Build();

        var provisioner = new TenantProvisioner(configuration);
        await provisioner.ProvisionarAsync("Nova Empresa LTDA", empresaId);

        var schemaName = TenantSchemaNames.FromRazaoSocial("Nova Empresa LTDA");
        var quotedSchema = TenantSchemaSql.CitarIdentificador(schemaName);

        var tableExists = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @SchemaName
                  AND table_name = 'usuarios'
            )
            """,
            new { SchemaName = schemaName });

        Assert.True(tableExists);

        var usuario = await fixture.Connection.QueryFirstOrDefaultAsync<UsuarioProvisionadoResult>(
            $"""
            SELECT nome AS Nome, email AS Email, senha AS Senha, perfil AS Perfil, empresa_id AS EmpresaId
            FROM {quotedSchema}.usuarios
            WHERE email = @Email
            """,
            new { Email = TenantDefaultUser.Email });

        Assert.NotNull(usuario);
        Assert.Equal(TenantDefaultUser.Nome, usuario!.Nome);
        Assert.Equal(TenantDefaultUser.Email, usuario.Email);
        Assert.Equal("Empresa", usuario.Perfil);
        Assert.Equal(empresaId, usuario.EmpresaId);
        Assert.Equal(Criptografia.Encrypt(TenantDefaultUser.Senha), usuario.Senha);

        var perfisTable = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'perfis')
            """, new { SchemaName = schemaName });
        Assert.True(perfisTable);

        var dono = await fixture.Connection.QueryFirstOrDefaultAsync<DonoProvisionadoResult>(
            $"""
            SELECT id AS Id, nome AS Nome, eh_sistema AS EhSistema
            FROM {quotedSchema}.perfis WHERE eh_sistema = TRUE LIMIT 1
            """);
        Assert.NotNull(dono);
        Assert.Equal("Dono", dono!.Nome);

        var permCount = await fixture.Connection.ExecuteScalarAsync<int>(
            $"""
            SELECT COUNT(*) FROM {quotedSchema}.perfil_permissoes
            WHERE perfil_id = @Id AND visualizar AND criar AND editar AND excluir
            """, new { dono.Id });
        Assert.Equal(ModulosSistemaCatalog.Todos.Count, permCount);

        var perfilIdUsuario = await fixture.Connection.ExecuteScalarAsync<Guid?>(
            $"""
            SELECT perfil_id FROM {quotedSchema}.usuarios WHERE email = @Email
            """, new { Email = TenantDefaultUser.Email });
        Assert.Equal(dono.Id, perfilIdUsuario);

        var calendarioExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'calendario_dias')
            """, new { SchemaName = schemaName });
        Assert.True(calendarioExiste);

        var anoCorrente = DateTime.UtcNow.Year;
        var diasDoAno = await fixture.Connection.ExecuteScalarAsync<int>(
            $"""
            SELECT COUNT(*)::int FROM {quotedSchema}.calendario_dias
            WHERE EXTRACT(YEAR FROM data) = @Ano
            """,
            new { Ano = anoCorrente });
        var esperado = DateTime.IsLeapYear(anoCorrente) ? 366 : 365;
        Assert.Equal(esperado, diasDoAno);

        var funcionariosExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionarios')
            """, new { SchemaName = schemaName });
        Assert.True(funcionariosExiste);

        var beneficiosExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionario_beneficios')
            """, new { SchemaName = schemaName });
        Assert.True(beneficiosExiste);

        var afastamentosExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionario_afastamentos')
            """, new { SchemaName = schemaName });
        Assert.True(afastamentosExiste);

        var linhasOnibusExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'linhas_onibus')
            """, new { SchemaName = schemaName });
        Assert.True(linhasOnibusExiste);

        var funcionarioLinhasExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionario_linhas')
            """, new { SchemaName = schemaName });
        Assert.True(funcionarioLinhasExiste);

        var motivoExiste = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.columns
              WHERE table_schema = @SchemaName AND table_name = 'funcionarios'
                AND column_name = 'motivo_afastamento')
            """, new { SchemaName = schemaName });
        Assert.False(motivoExiste);
    }

    [SkippableFact]
    public async Task GarantirPerfisEmTenantsExistentesAsync_DeveMigrarSchemaAntigo()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = Guid.NewGuid();
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Legacy LTDA",
            Dominio = "legacy",
        });

        var schemaName = TenantSchemaNames.FromRazaoSocial("Empresa Legacy LTDA");
        var quotedSchema = TenantSchemaSql.CitarIdentificador(schemaName);

        await fixture.Connection!.ExecuteAsync(TenantSchemaSql.CriarSchema(schemaName));
        await fixture.Connection.ExecuteAsync(TenantSchemaSql.CriarTabelaUsuarios(schemaName));
        await fixture.Connection.ExecuteAsync(
            $"""
            INSERT INTO {quotedSchema}.usuarios
                (id, nome, senha, email, perfil, empresa_id, data_inclusao)
            VALUES
                (@Id, @Nome, @Senha, @Email, 'Empresa', @EmpresaId, @DataInclusao)
            """,
            new
            {
                Id = Guid.NewGuid(),
                Nome = TenantDefaultUser.Nome,
                Senha = Criptografia.Encrypt(TenantDefaultUser.Senha),
                Email = TenantDefaultUser.Email,
                EmpresaId = empresaId,
                DataInclusao = DateTime.UtcNow,
            });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.CatalogConnectionString,
            })
            .Build();

        var provisioner = new TenantProvisioner(configuration);
        await provisioner.GarantirPerfisEmTenantsExistentesAsync();

        var donoId = await fixture.Connection.ExecuteScalarAsync<Guid?>(
            $"SELECT id FROM {quotedSchema}.perfis WHERE eh_sistema = TRUE LIMIT 1");
        Assert.NotNull(donoId);

        var perfilIdUsuario = await fixture.Connection.ExecuteScalarAsync<Guid?>(
            $"""
            SELECT perfil_id FROM {quotedSchema}.usuarios WHERE email = @Email
            """, new { Email = TenantDefaultUser.Email });
        Assert.Equal(donoId, perfilIdUsuario);
    }

    [SkippableFact]
    public async Task GarantirPerfisEmTenantsExistentesAsync_DeveRenomearDiasUteisECompletarModulos()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = Guid.NewGuid();
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Mig Calendario LTDA",
            Dominio = "migcalendario",
        });

        var schemaName = TenantSchemaNames.FromRazaoSocial("Empresa Mig Calendario LTDA");
        var quotedSchema = TenantSchemaSql.CitarIdentificador(schemaName);
        var agora = DateTime.UtcNow;
        var donoIdSeed = Guid.NewGuid();

        await fixture.Connection!.ExecuteAsync(TenantSchemaSql.CriarSchema(schemaName));
        await fixture.Connection.ExecuteAsync(TenantSchemaSql.CriarTabelaUsuarios(schemaName));
        await fixture.Connection.ExecuteAsync(TenantSchemaSql.CriarTabelaPerfis(schemaName));
        await fixture.Connection.ExecuteAsync(TenantSchemaSql.CriarTabelaPerfilPermissoes(schemaName));
        await fixture.Connection.ExecuteAsync(TenantSchemaSql.AlterarUsuariosAdicionarPerfilId(schemaName));

        await fixture.Connection.ExecuteAsync(
            TenantSchemaSql.InserirPerfilDono(schemaName),
            new { Id = donoIdSeed, Nome = "Dono", DataInclusao = agora });

        await fixture.Connection.ExecuteAsync(
            TenantSchemaSql.InserirPerfilPermissao(schemaName),
            new
            {
                Id = Guid.NewGuid(),
                PerfilId = donoIdSeed,
                CodigoMenu = "dias_uteis",
                Visualizar = true,
                Criar = true,
                Editar = true,
                Excluir = true,
            });

        await fixture.Connection.ExecuteAsync(
            $"""
            INSERT INTO {quotedSchema}.usuarios
                (id, nome, senha, email, perfil, empresa_id, perfil_id, data_inclusao)
            VALUES
                (@Id, @Nome, @Senha, @Email, 'Empresa', @EmpresaId, @PerfilId, @DataInclusao)
            """,
            new
            {
                Id = Guid.NewGuid(),
                Nome = TenantDefaultUser.Nome,
                Senha = Criptografia.Encrypt(TenantDefaultUser.Senha),
                Email = TenantDefaultUser.Email,
                EmpresaId = empresaId,
                PerfilId = donoIdSeed,
                DataInclusao = agora,
            });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.CatalogConnectionString,
            })
            .Build();

        await new TenantProvisioner(configuration).GarantirPerfisEmTenantsExistentesAsync();

        var diasUteisRestantes = await fixture.Connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {quotedSchema}.perfil_permissoes WHERE codigo_menu = 'dias_uteis'");
        Assert.Equal(0, diasUteisRestantes);

        var temCalendario = await fixture.Connection.ExecuteScalarAsync<bool>(
            $"""
            SELECT EXISTS(
              SELECT 1 FROM {quotedSchema}.perfil_permissoes
              WHERE perfil_id = @Id AND codigo_menu = 'calendario')
            """, new { Id = donoIdSeed });
        Assert.True(temCalendario);

        var permCount = await fixture.Connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {quotedSchema}.perfil_permissoes WHERE perfil_id = @Id",
            new { Id = donoIdSeed });
        Assert.Equal(ModulosSistemaCatalog.Todos.Count, permCount);

        var afastamentosExiste = await fixture.Connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionario_afastamentos')
            """, new { SchemaName = schemaName });
        Assert.True(afastamentosExiste);

        var linhasOnibusExiste = await fixture.Connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'linhas_onibus')
            """, new { SchemaName = schemaName });
        Assert.True(linhasOnibusExiste);

        var funcionarioLinhasExiste = await fixture.Connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'funcionario_linhas')
            """, new { SchemaName = schemaName });
        Assert.True(funcionarioLinhasExiste);
    }

    private sealed class DonoProvisionadoResult
    {
        public Guid Id { get; init; }
        public string Nome { get; init; } = string.Empty;
        public bool EhSistema { get; init; }
    }

    private sealed class UsuarioProvisionadoResult
    {
        public string Nome { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Senha { get; init; } = string.Empty;
        public string Perfil { get; init; } = string.Empty;
        public Guid EmpresaId { get; init; }
    }
}
