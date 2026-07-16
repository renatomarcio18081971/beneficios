### Task 1: Enums, CPF util e catálogo de jornada

**Files:**
- Create: `src/Beneficios.Domain/Enums/TipoContrato.cs`
- Create: `src/Beneficios.Domain/Enums/SituacaoFuncionario.cs`
- Create: `src/Beneficios.Domain/Enums/JornadaTrabalho.cs`
- Create: `src/Beneficios.Domain/JornadaTrabalhoCatalog.cs`
- Create: `src/Beneficios.Domain/CpfUtil.cs`
- Test: `tests/Beneficios.Tests/Domain/CpfUtilTests.cs`
- Test: `tests/Beneficios.Tests/Domain/JornadaTrabalhoCatalogTests.cs`

**Interfaces:**
- Produces: `CpfUtil.Normalizar(string?) → string?`, `CpfUtil.EhValido(string cpfSomenteDigitos) → bool`
- Produces: `JornadaTrabalho` enum com valores alinhados aos códigos do banco (usar `[EnumMember]` / nomes Pascal + conversão string no repo, **ou** enum com nomes `QuarentaQuatroHorasClt` mapeados para `"44h_clt"` via helper — preferir helper `JornadaTrabalhoConversao` com strings literais do spec)
- Produces: `JornadaTrabalhoCatalog.ExigeDetalhe(string codigo) → bool` true só para `especial_categoria`
- Códigos exatos: `44h_clt`, `40h_seg_sex`, `36h`, `12x36`, `6x1`, `5x1`, `5x2`, `tempo_parcial`, `especial_categoria`

- [ ] **Step 1: Write failing CPF tests**

```csharp
[Fact]
public void EhValido_CpfValido_DeveRetornarTrue()
    => Assert.True(CpfUtil.EhValido("52998224725"));

[Fact]
public void EhValido_CpfInvalido_DeveRetornarFalse()
    => Assert.False(CpfUtil.EhValido("11111111111"));

[Fact]
public void Normalizar_RemoveMascara()
    => Assert.Equal("52998224725", CpfUtil.Normalizar("529.982.247-25"));
```

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~CpfUtil" -v q
```

Expected: FAIL (type missing)

- [ ] **Step 3: Implement `CpfUtil`, enums, `JornadaTrabalhoCatalog` (+ conversão string ↔ enum se usar enum)**

- [ ] **Step 4: Run catalog tests — PASS** (especial exige detalhe; labels não vazias)

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(domain): add funcionario enums, CPF util and jornada catalog"
```

---

