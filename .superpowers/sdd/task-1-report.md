# Task 1 Report — Backend: Campo Perfil (schema + domain)

**Status:** DONE  
**Commit:** `218bb415fbdfb96623337e61ca5e07d56952a7bc` — `feat(domain): add UsuarioPerfil enum and perfil column script`  
**Branch:** `develop`

## Summary

Added `UsuarioPerfil` enum (`Empresa = 0`, `Admin = 1`) and `Perfil` property across domain entity/models. Updated SQL scripts for fresh installs (`02`), sample data (`03`), and migration (`04`). Default `Perfil` on `Usuario` is `UsuarioPerfil.Empresa`.

## Files Changed

| Action | File |
|--------|------|
| Create | `src/Beneficios.Domain/Enums/UsuarioPerfil.cs` |
| Create | `src/Beneficios.Infrastructure/Scripts/04_Alter_Table_Usuarios_Perfil.sql` |
| Modify | `src/Beneficios.Domain/Entities/Usuario.cs` |
| Modify | `src/Beneficios.Domain/Models/UsuarioAuthResult.cs` |
| Modify | `src/Beneficios.Domain/Models/UsuarioQueryResult.cs` |
| Modify | `src/Beneficios.Domain/Models/UsuarioSalvarParams.cs` |
| Modify | `src/Beneficios.Infrastructure/Scripts/02_Create_Table_Usuarios.sql` |
| Modify | `src/Beneficios.Infrastructure/Scripts/03_Insert_Sample_Data.sql` |
| Modify | `tests/Beneficios.Tests/Domain/UsuarioTests.cs` |

## TDD Evidence

### Step 1–2: RED — Failing test

**Command:**
```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n
```

**Result:** FAIL (exit code 1)

**Key output:**
```
error CS0234: O nome de tipo ou namespace "Enums" não existe no namespace "Beneficios.Domain"
  (você está sem uma referência de assembly?)
  [C:\Projetos\dotnet\Beneficios\tests\Beneficios.Tests\Beneficios.Tests.csproj]
```

Build failed before test execution — `UsuarioPerfil` enum and `Perfil` property did not exist.

### Step 3–4: GREEN — Minimal implementation

**Command:**
```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "Usuario_DeveTerPropriedadePerfil" -v n
```

**Result:** PASS (exit code 0)

**Key output:**
```
Aprovado Beneficios.Tests.Domain.UsuarioTests.Usuario_DeveTerPropriedadePerfil [7 ms]
Total de testes: 1
     Aprovados: 1
```

### Step 5: Full test suite

**Command:**
```bash
dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj -v n
```

**Result:** PASS (exit code 0)

**Key output:**
```
Execução de Teste Bem-sucedida.
Total de testes: 74
     Aprovados: 74
Tempo total: 1,5466 Segundos
```

## Implementation Notes

- `UsuarioPerfil`: `Empresa = 0`, `Admin = 1` (per task brief Step 3; differs from Interfaces section which listed reversed values).
- `Usuario.Perfil` defaults to `UsuarioPerfil.Empresa`.
- `Usuario_PropriedadesPadraoDevemSerInicializadas` updated to assert default `Perfil`.
- `02_Create_Table_Usuarios.sql` includes `perfil VARCHAR(20) NOT NULL DEFAULT 'Empresa'` for fresh installs.
- `04_Alter_Table_Usuarios_Perfil.sql` adds column and sets `Admin` for `admin@exemplo.com`.

## Self-Review

| Check | Result |
|-------|--------|
| TDD cycle (RED → GREEN) | ✅ |
| Full suite green (74/74) | ✅ |
| Only task files committed | ✅ |
| SQL scripts consistent | ✅ |
| Default Perfil = Empresa | ✅ |

## Concerns / Out of Scope

- **Repository/Application layer** not updated in this task — `Perfil` is not yet read/written from DB. Expected in later tasks.
- **Enum order** in brief Interfaces section (`Admin = 0`) differs from implementation (`Empresa = 0`); implementation follows Step 3 and user guidance for default `Empresa`.
- Pre-existing uncommitted changes in Application/Api/Infrastructure repos remain untouched.

## Next Task Dependencies

Task 2+ can wire `Perfil` through repository queries, DTOs, AutoMapper profiles, and API responses using the domain types introduced here.
