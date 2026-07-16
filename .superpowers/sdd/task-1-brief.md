### Task 1: Módulos de permissão (back + front)

**Files:**
- Modify: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Modify: `Beneficios.Front/src/app/core/auth/modulos-sistema.ts`
- Modify: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: códigos `linhas_onibus` (rota `/linhas-onibus`, ícone `directions_bus`) e `funcionario_linhas` (rota `/funcionario-linhas`, ícone `commute`), ambos com `AcaoPermissao.Todas`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Todos_DeveConterLinhasOnibus()
{
    Assert.Contains(ModulosSistemaCatalog.Todos,
        m => m.Codigo == "linhas_onibus" && m.Rota == "/linhas-onibus");
}

[Fact]
public void Todos_DeveConterFuncionarioLinhas()
{
    Assert.Contains(ModulosSistemaCatalog.Todos,
        m => m.Codigo == "funcionario_linhas" && m.Rota == "/funcionario-linhas");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q`  
Expected: FAIL (códigos ausentes)

- [ ] **Step 3: Add modules**

Em `ModulosSistemaCatalog.Todos`, após `afastamentos`:

```csharp
new("linhas_onibus", "Linhas de Ônibus", "/linhas-onibus", AcaoPermissao.Todas),
new("funcionario_linhas", "Funcionário × Linhas", "/funcionario-linhas", AcaoPermissao.Todas),
```

Em `MODULOS_SISTEMA` (front), entradas equivalentes com `icone: 'directions_bus'` e `icone: 'commute'`.

- [ ] **Step 4: Run tests — expect PASS**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~ModulosSistemaCatalog" -v q`

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/ModulosSistemaCatalog.cs Beneficios.Front/src/app/core/auth/modulos-sistema.ts tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
git commit -m "feat: add linhas_onibus and funcionario_linhas permission modules"
```

---
