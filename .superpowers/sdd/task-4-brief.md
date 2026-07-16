### Task 4: DDL e provisionamento tenant

**Files:**
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs`
- Modify: `src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs`
- Create: `src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql`
- Create: `src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql`
- Modify: fixture de integração se aplicar DDL tenant (ex.: `PostgresFixture.cs`)
- Test: `tests/Beneficios.Tests/Infrastructure/DatabaseConfigurationTests.cs` ou `TenantProvisionerTests.cs` — assert métodos SQL não vazios / provisionamento inclui tabelas se o padrão do repo for esse

**Interfaces:**
- Produces: `TenantSchemaSql.CriarTabelaLinhasOnibus(string schemaName)` e `CriarTabelaFuncionarioLinhas(string schemaName)`
- Consumes: padrão de `CriarTabelaFuncionarioAfastamentos`

- [ ] **Step 1: Add SQL builders** (espelhar afastamentos; UNIQUE e FKs conforme spec)

```csharp
public static string CriarTabelaLinhasOnibus(string schemaName)
{
    var quotedSchema = CitarIdentificador(schemaName);
    var indexPrefix = schemaName.Replace('-', '_');
    return $"""
        CREATE TABLE IF NOT EXISTS {quotedSchema}.linhas_onibus (
            id UUID PRIMARY KEY,
            descricao VARCHAR NOT NULL,
            data_inicio DATE NOT NULL,
            data_fim DATE NULL,
            valor_tarifa NUMERIC(18,2) NOT NULL,
            data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
            data_alteracao TIMESTAMP NULL,
            usuario_alteracao_id UUID NULL
        );
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_descricao
            ON {quotedSchema}.linhas_onibus(descricao);
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_linhas_onibus_datas
            ON {quotedSchema}.linhas_onibus(data_inicio, data_fim);
        """;
}

public static string CriarTabelaFuncionarioLinhas(string schemaName)
{
    var quotedSchema = CitarIdentificador(schemaName);
    var indexPrefix = schemaName.Replace('-', '_');
    return $"""
        CREATE TABLE IF NOT EXISTS {quotedSchema}.funcionario_linhas (
            id UUID PRIMARY KEY,
            funcionario_id UUID NOT NULL,
            linha_onibus_id UUID NOT NULL,
            quantidade INT NOT NULL,
            data_inicio DATE NOT NULL,
            data_fim DATE NULL,
            data_inclusao TIMESTAMP NOT NULL DEFAULT NOW(),
            data_alteracao TIMESTAMP NULL,
            usuario_alteracao_id UUID NULL,
            CONSTRAINT fk_{indexPrefix}_func_linha_funcionario
                FOREIGN KEY (funcionario_id) REFERENCES {quotedSchema}.funcionarios(id),
            CONSTRAINT fk_{indexPrefix}_func_linha_linha
                FOREIGN KEY (linha_onibus_id) REFERENCES {quotedSchema}.linhas_onibus(id),
            CONSTRAINT uq_{indexPrefix}_func_linha UNIQUE (funcionario_id, linha_onibus_id)
        );
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_funcionario
            ON {quotedSchema}.funcionario_linhas(funcionario_id);
        CREATE INDEX IF NOT EXISTS idx_{indexPrefix}_func_linha_linha
            ON {quotedSchema}.funcionario_linhas(linha_onibus_id);
        """;
}
```

Scripts `14_` / `15_`: comentário de referência apontando para esses métodos (padrão scripts 11–13).

- [ ] **Step 2: Call both in `ProvisionarAsync` and `GarantirPerfisNoSchemaAsync`** (após afastamentos; **linhas antes** de funcionario_linhas por FK)

- [ ] **Step 3: Build Infrastructure — expect PASS**

- [ ] **Step 4: Commit**

```bash
git add src/Beneficios.Infrastructure/Tenancy/TenantSchemaSql.cs src/Beneficios.Infrastructure/Tenancy/TenantProvisioner.cs src/Beneficios.Infrastructure/Scripts/14_Create_Linhas_Onibus_Tenant.sql src/Beneficios.Infrastructure/Scripts/15_Create_Funcionario_Linhas_Tenant.sql
git commit -m "feat(infra): add linhas_onibus and funcionario_linhas DDL provisioning"
```

---
