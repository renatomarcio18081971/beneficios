## Task 2: Backend — JWT claims + LoginResponseDto

**Files:**
- Modify: `src/Beneficios.Application/DTOs/LoginResponseDto.cs`
- Modify: `src/Beneficios.Application/Interfaces/ITokenService.cs`
- Modify: `src/Beneficios.Application/Services/TokenService.cs`
- Modify: `src/Beneficios.Application/Mappings/UsuarioProfile.cs`
- Create/Modify: `tests/Beneficios.Tests/Application/TokenServiceTests.cs`

**Interfaces:**
- Consumes: `UsuarioPerfil` from Task 1
- Produces: `ITokenService.GenerateToken(Guid usuarioId, string email, UsuarioPerfil perfil, Guid? empresaId)`
- Produces: `LoginResponseDto` with `Perfil`, `EmpresaId`, `EmpresaDominio`

(Full steps in plan lines 158-235)

Commit message: `feat(auth): add perfil and empresa_id JWT claims`
