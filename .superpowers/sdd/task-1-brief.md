### Task 1: Catálogo de feriados nacionais

**Files:**
- Create: `src/Beneficios.Domain/FeriadosNacionaisCatalog.cs`
- Test: `tests/Beneficios.Tests/Domain/FeriadosNacionaisCatalogTests.cs`

**Interfaces:**
- Produces: `FeriadosNacionaisCatalog.ObterParaAno(int ano) → IReadOnlyList<FeriadoNacional>` onde `FeriadoNacional` é `record(DateOnly Data, string Nome)`
- Produces: datas móveis derivadas da Páscoa (algoritmo de Meeus/Jones/Butcher ou equivalente documentado no teste)

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void ObterParaAno_2026_DeveConterFeriadosFixosEMoveisConhecidos()
{
    var feriados = FeriadosNacionaisCatalog.ObterParaAno(2026);
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 1, 1) && f.Nome.Contains("Confraterniza", StringComparison.OrdinalIgnoreCase));
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 12, 25));
    // Carnaval 2026-02-16/17 (segunda/terça) — usar a regra do catálogo (terça de carnaval oficial)
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 4, 3)); // Sexta-feira Santa (Páscoa 2026-04-05 - 2)
    Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 2, 17)); // Carnaval (terça)
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~FeriadosNacionaisCatalogTests" -v n`  
Expected: FAIL (tipo/catálogo inexistente)

- [ ] **Step 3: Write minimal implementation**

Implementar `FeriadosNacionaisCatalog` com:
- Fixos: 01/01, 21/04, 01/05, 07/09, 12/10, 02/11, 15/11, 25/12 (e 20/11 Dia da Consciência Negra se adotado como nacional no escopo — **incluir** 20/11)
- Móveis a partir da Páscoa: Carnaval (terça = Páscoa−47), Sexta-feira Santa (Páscoa−2), Corpus Christi (Páscoa+60)

- [ ] **Step 4: Run test to verify it passes**

Run: mesmo comando — Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/FeriadosNacionaisCatalog.cs tests/Beneficios.Tests/Domain/FeriadosNacionaisCatalogTests.cs
git commit -m "feat(domain): add Brazilian national holidays catalog"
```

---
