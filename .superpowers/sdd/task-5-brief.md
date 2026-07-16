### Task 5: Repository + Service + API — Linhas de ônibus

**Files:**
- Create: `LinhaOnibusRepository.cs`, `LinhaOnibusService.cs`, `ILinhaOnibusService.cs`, `LinhaOnibusDtos.cs`, `LinhaOnibusProfile.cs`, `LinhasOnibusController.cs`
- Modify: `Program.cs` (DI)
- Test: `tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs`
- Test: `tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs` (padrão Afastamentos)

**Interfaces:**
- Consumes: `ILinhaOnibusRepository`, `IFuncionarioLinhaRepository.ExisteVinculoAbertoPorLinhaAsync`
- Produces:

```csharp
public record LinhaOnibusDto(Guid Id, string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusSalvarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);
public record LinhaOnibusAtualizarDto(string Descricao, DateOnly DataInicio, DateOnly? DataFim, decimal ValorTarifa);

public interface ILinhaOnibusService
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(string? descricao, bool? somenteVigentes);
}
```

Mensagens constantes em `LinhaOnibusService`:

```csharp
public const string MensagemDataFim = "A data fim deve ser maior ou igual à data início.";
public const string MensagemNaoEncontrada = "Linha não encontrada.";
public const string MensagemVinculosAbertos = "Existem vínculos em aberto para esta linha; encerre-os antes de finalizar a vigência.";
public const string MensagemTarifa = "O valor da tarifa deve ser maior ou igual a zero.";
```

Regras em `AtualizarAsync`: se `dto.DataFim` não nulo e `ExisteVinculoAbertoPorLinhaAsync(id)` → throw `MensagemVinculosAbertos`.  
Validar datas com mesma lógica de afastamentos; tarifa ≥ 0.  
Vigência filtro: `AfastamentoPeriodo.EstaAtivoEm(dataInicio, dataFim, referencia)` (referencia = hoje).

Controller: `[Route("api/linhas-onibus")]`, `CodigoMenu = "linhas_onibus"`, espelhar `AfastamentosController` **sem** DELETE.

- [ ] **Step 1: Write failing service tests** (data fim inválida; tarifa negativa; encerrar com vínculo aberto; not found)

- [ ] **Step 2: Run — expect FAIL**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj --filter "FullyQualifiedName~LinhaOnibusService" -v q`

- [ ] **Step 3: Implement repository (Dapper, search_path tenant — copiar padrão `FuncionarioAfastamentoRepository`), service, profile, controller, DI**

Para Task 5 o método `ExisteVinculoAbertoPorLinhaAsync` pode retornar `false` temporariamente via stub/mock nos testes do service; implementação real do repo de vínculo na Task 6. Alternativa: implementar stub no `FuncionarioLinhaRepository` mínimo só com esse método na Task 5 — preferir **mock** de `IFuncionarioLinhaRepository` nos testes de `LinhaOnibusService`.

- [ ] **Step 4: Run service + controller tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Infrastructure/Repositories/LinhaOnibusRepository.cs src/Beneficios.Application/Services/LinhaOnibusService.cs src/Beneficios.Application/Interfaces/ILinhaOnibusService.cs src/Beneficios.Application/DTOs/LinhaOnibusDtos.cs src/Beneficios.Application/Mappings/LinhaOnibusProfile.cs src/Beneficios.Api/Controllers/LinhasOnibusController.cs src/Beneficios.Api/Program.cs tests/Beneficios.Tests/Application/LinhaOnibusServiceTests.cs tests/Beneficios.Tests/Api/LinhasOnibusControllerTests.cs
git commit -m "feat: add linhas-onibus service, repository, and API"
```

---
