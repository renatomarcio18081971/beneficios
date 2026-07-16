### Task 6: Repository + Service + API — Funcionário × Linhas

**Files:**
- Create: `FuncionarioLinhaRepository.cs`, `FuncionarioLinhaService.cs`, `IFuncionarioLinhaService.cs`, `FuncionarioLinhaDtos.cs`, `FuncionarioLinhaProfile.cs`, `FuncionarioLinhasController.cs`
- Modify: `Program.cs`
- Test: `FuncionarioLinhaServiceTests.cs`, `FuncionarioLinhasControllerTests.cs`

**Interfaces:**
- Consumes: `IFuncionarioLinhaRepository`, `ILinhaOnibusRepository`, `IFuncionarioRepository.ObterPorIdAsync` + `ObterBeneficiosAsync`
- Produces:

```csharp
public record FuncionarioLinhaDto(
    Guid Id, Guid FuncionarioId, string FuncionarioNome,
    Guid LinhaOnibusId, string LinhaDescricao,
    int Quantidade, DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaSalvarDto(
    Guid FuncionarioId, Guid LinhaOnibusId, int Quantidade,
    DateOnly DataInicio, DateOnly? DataFim);

public record FuncionarioLinhaAtualizarDto(
    Guid LinhaOnibusId, int Quantidade, DateOnly DataInicio, DateOnly? DataFim);

public interface IFuncionarioLinhaService
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes);
}
```

Mensagens:

```csharp
public const string MensagemVtInativo = "Funcionário sem vale transporte ativo; não é possível vincular linhas.";
public const string MensagemLinhaNaoVigente = "A linha selecionada não está vigente na data de início do vínculo.";
public const string MensagemParDuplicado = "Esta linha já está vinculada a este funcionário.";
public const string MensagemQuantidade = "A quantidade de utilizações deve ser maior ou igual a 1.";
public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
public const string MensagemVinculoNaoEncontrado = "Vínculo não encontrado.";
public const string MensagemFuncionarioNaoEncontrado = "Funcionário não encontrado.";
public const string MensagemLinhaNaoEncontrada = "Linha não encontrada.";
public const string CodigoValeTransporte = "vale_transporte";
```

Fluxo `SalvarAsync` / `AtualizarAsync`:
1. Validar datas e quantidade ≥ 1  
2. Funcionário existe  
3. Benefícios: algum com `CodigoBeneficio == vale_transporte` e `Ativo == true` (senão `MensagemVtInativo`)  
4. Linha existe; `AfastamentoPeriodo.EstaAtivoEm(linha.DataInicio, linha.DataFim, dto.DataInicio)`  
5. `ExisteParAsync` → `MensagemParDuplicado`  
6. Persist  

SQL `EncerrarAbertosPorFuncionarioAsync`:

```sql
UPDATE funcionario_linhas
SET data_fim = @DataFim, data_alteracao = @DataAlteracao, usuario_alteracao_id = @UsuarioAlteracaoId
WHERE funcionario_id = @FuncionarioId AND data_fim IS NULL
```

Controller: `[Route("api/funcionario-linhas")]`, `CodigoMenu = "funcionario_linhas"`, sem DELETE.

- [ ] **Step 1: Write failing tests** (VT off, linha vencida, duplicado, quantidade 0, happy path)

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement repository + service + API + DI**

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Infrastructure/Repositories/FuncionarioLinhaRepository.cs src/Beneficios.Application/Services/FuncionarioLinhaService.cs src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs src/Beneficios.Application/DTOs/FuncionarioLinhaDtos.cs src/Beneficios.Application/Mappings/FuncionarioLinhaProfile.cs src/Beneficios.Api/Controllers/FuncionarioLinhasController.cs src/Beneficios.Api/Program.cs tests/Beneficios.Tests/Application/FuncionarioLinhaServiceTests.cs tests/Beneficios.Tests/Api/FuncionarioLinhasControllerTests.cs
git commit -m "feat: add funcionario-linhas service, repository, and API"
```

---
