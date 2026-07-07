## Task 1: Backend — Campo Perfil (schema + domain)

**Files:**
- Create: `src/Beneficios.Infrastructure/Scripts/04_Alter_Table_Usuarios_Perfil.sql`
- Create: `src/Beneficios.Domain/Enums/UsuarioPerfil.cs`
- Modify: `src/Beneficios.Domain/Entities/Usuario.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioAuthResult.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioQueryResult.cs`
- Modify: `src/Beneficios.Domain/Models/UsuarioSalvarParams.cs`
- Modify: `src/Beneficios.Infrastructure/Scripts/03_Insert_Sample_Data.sql`
- Test: `tests/Beneficios.Tests/Domain/UsuarioTests.cs`

**Interfaces:**
- Produces: `UsuarioPerfil` enum (`Admin = 0`, `Empresa = 1`), propriedade `Perfil` nas entidades/models

- [ ] **Step 1: Write the failing test**

Adicionar em `tests/Beneficios.Tests/Domain/UsuarioTests.cs`:

```csharp
[Fact]
public void Usuario_DeveTerPropriedadePerfil()
{
    var usuario = new Usuario { Perfil = UsuarioPerfil.Admin };
    Assert.Equal(UsuarioPerfil.Admin, usuario.Perfil);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n`
Expected: FAIL — `UsuarioPerfil` / `Perfil` não existem

- [ ] **Step 3: Write minimal implementation**

`04_Alter_Table_Usuarios_Perfil.sql`:

```sql
ALTER TABLE beneficios.usuarios
    ADD COLUMN perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa';

UPDATE beneficios.usuarios SET perfil = 'Admin' WHERE email = 'admin@exemplo.com';
```

`UsuarioPerfil.cs`:

```csharp
namespace Beneficios.Domain.Enums;

public enum UsuarioPerfil
{
    Empresa = 0,
    Admin = 1
}
```

Adicionar `public UsuarioPerfil Perfil { get; set; }` em `Usuario`, `UsuarioAuthResult`, `UsuarioQueryResult`, `UsuarioSalvarParams`.

Atualizar `03_Insert_Sample_Data.sql` para incluir coluna `perfil` nos INSERTs (`Admin` para admin@exemplo.com, `Empresa` para teste@exemplo.com).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Domain src/Beneficios.Infrastructure/Scripts tests/Beneficios.Tests/Domain/UsuarioTests.cs
git commit -m "feat(domain): add UsuarioPerfil enum and perfil column script"
```
