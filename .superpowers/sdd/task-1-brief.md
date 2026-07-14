## Task 1: Catálogo de módulos + `AcaoPermissao`

**Files:**
- Create: `src/Beneficios.Domain/Enums/AcaoPermissao.cs`
- Create: `src/Beneficios.Domain/Models/ModuloSistema.cs`
- Create: `src/Beneficios.Domain/ModulosSistemaCatalog.cs`
- Test: `tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs`

**Interfaces:**
- Produces: `AcaoPermissao` { Visualizar, Criar, Editar, Excluir }; `ModuloSistema(string Codigo, string NomeExibicao, string Rota, AcaoPermissao AcoesSuportadas)`; `ModulosSistemaCatalog.Todos` com códigos `dashboard`, `usuarios`, `perfis`

- [ ] **Step 1: Write the failing test**

```csharp
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class ModulosSistemaCatalogTests
{
    [Fact]
    public void Todos_DeveConterCodigosDaV1()
    {
        var codigos = ModulosSistemaCatalog.Todos.Select(m => m.Codigo).ToArray();
        Assert.Contains("dashboard", codigos);
        Assert.Contains("usuarios", codigos);
        Assert.Contains("perfis", codigos);
    }

    [Fact]
    public void Dashboard_DeveSuportarSomenteVisualizar()
    {
        var dashboard = ModulosSistemaCatalog.Todos.Single(m => m.Codigo == "dashboard");
        Assert.Equal(AcaoPermissao.Visualizar, dashboard.AcoesSuportadas);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "ModulosSistemaCatalogTests" -v n`  
Expected: FAIL (tipos não existem)

- [ ] **Step 3: Implement**

`AcaoPermissao.cs`:

```csharp
namespace Beneficios.Domain.Enums;

[Flags]
public enum AcaoPermissao
{
    Nenhuma = 0,
    Visualizar = 1,
    Criar = 2,
    Editar = 4,
    Excluir = 8,
    Todas = Visualizar | Criar | Editar | Excluir
}
```

`ModuloSistema.cs`:

```csharp
using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public sealed record ModuloSistema(
    string Codigo,
    string NomeExibicao,
    string Rota,
    AcaoPermissao AcoesSuportadas);
```

`ModulosSistemaCatalog.cs`:

```csharp
using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;

namespace Beneficios.Domain;

public static class ModulosSistemaCatalog
{
    public static IReadOnlyList<ModuloSistema> Todos { get; } =
    [
        new("dashboard", "Dashboard", "/dashboard", AcaoPermissao.Visualizar),
        new("usuarios", "Usuários", "/usuarios", AcaoPermissao.Todas),
        new("perfis", "Perfis", "/perfis", AcaoPermissao.Todas),
    ];

    public static ModuloSistema? ObterPorCodigo(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
```

Salvar com **UTF-8 BOM**.

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain/Enums/AcaoPermissao.cs src/Beneficios.Domain/Models/ModuloSistema.cs src/Beneficios.Domain/ModulosSistemaCatalog.cs tests/Beneficios.Tests/Domain/ModulosSistemaCatalogTests.cs
git commit -m "feat: add module catalog and permission actions for tenant RBAC"
```

---

