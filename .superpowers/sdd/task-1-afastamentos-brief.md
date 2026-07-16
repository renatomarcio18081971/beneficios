### Task 1: Enum, catÃ¡logo e helpers de perÃ­odo

**Files:**
- Create: `src/Beneficios.Domain/Enums/TipoAfastamento.cs`
- Create: `src/Beneficios.Domain/TipoAfastamentoCatalog.cs`
- Create: `src/Beneficios.Domain/AfastamentoPeriodo.cs`
- Test: `tests/Beneficios.Tests/Domain/AfastamentoPeriodoTests.cs`
- Test: `tests/Beneficios.Tests/Domain/TipoAfastamentoCatalogTests.cs`

**Interfaces:**
- Produces:

```csharp
public enum TipoAfastamento
{
    Ferias = 1,
    LicencaMedica = 2,
    MaternidadePaternidade = 3,
    Acidente = 4,
    Suspensao = 5,
    Outros = 6
}

public static class TipoAfastamentoCatalog
{
    public static string ParaBanco(TipoAfastamento tipo); // ferias | licenca_medica | ...
    public static TipoAfastamento DeBanco(string codigo);
    public static string Label(TipoAfastamento tipo);
    public static IReadOnlyList<(string codigo, string label)> Todos { get; }
}

public static class AfastamentoPeriodo
{
    // fim null = +âˆž. Conflito: iniA <= fimB && iniB <= fimA
    public static bool Sobrepoe(DateOnly iniA, DateOnly? fimA, DateOnly iniB, DateOnly? fimB);

    // dataInicio <= ref && (dataFim is null || dataFim >= ref)
    public static bool EstaAtivoEm(DateOnly dataInicio, DateOnly? dataFim, DateOnly referencia);
}
```

CÃ³digos banco: `ferias`, `licenca_medica`, `maternidade_paternidade`, `acidente`, `suspensao`, `outros`.

- [ ] **Step 1: Write failing tests**

```csharp
[Fact]
public void Sobrepoe_IntervalosAbertos_DeveDetectarConflito()
{
    Assert.True(AfastamentoPeriodo.Sobrepoe(
        new DateOnly(2026, 1, 1), null,
        new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)));
}

[Fact]
public void Sobrepoe_IntervalosDisjuntos_NaoConflita()
{
    Assert.False(AfastamentoPeriodo.Sobrepoe(
        new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
        new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)));
}

[Fact]
public void EstaAtivoEm_FimNulo_TrueSeJaComecou()
{
    Assert.True(AfastamentoPeriodo.EstaAtivoEm(
        new DateOnly(2026, 1, 1), null, new DateOnly(2026, 7, 14)));
}

[Fact]
public void Catalog_Ferias_CodigoELabel()
{
    Assert.Equal("ferias", TipoAfastamentoCatalog.ParaBanco(TipoAfastamento.Ferias));
    Assert.Equal("FÃ©rias", TipoAfastamentoCatalog.Label(TipoAfastamento.Ferias));
}
```

- [ ] **Step 2: Run â€” FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~AfastamentoPeriodo|FullyQualifiedName~TipoAfastamentoCatalog" -v q
```

Expected: FAIL (types missing)

- [ ] **Step 3: Implement enum, catalog, `AfastamentoPeriodo` (UTF-8 BOM)**

- [ ] **Step 4: Run â€” PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/Enums/TipoAfastamento.cs src/Beneficios.Domain/TipoAfastamentoCatalog.cs src/Beneficios.Domain/AfastamentoPeriodo.cs tests/Beneficios.Tests/Domain/AfastamentoPeriodoTests.cs tests/Beneficios.Tests/Domain/TipoAfastamentoCatalogTests.cs
git commit -m "feat(domain): add tipo afastamento and period overlap helpers"
```

---
