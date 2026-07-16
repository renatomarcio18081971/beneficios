### Task 2: Domain models e interfaces — Linha de ônibus

**Files:**
- Create: `src/Beneficios.Domain/Models/LinhaOnibusSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/LinhaOnibusFiltroParams.cs`
- Create: `src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs`
- Test: `tests/Beneficios.Tests/Domain/LinhaOnibusModelsCompileTests.cs` (ou smoke via build)

**Interfaces:**
- Produces:

```csharp
public class LinhaOnibusSalvarParams
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public decimal ValorTarifa { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class LinhaOnibusAtualizarParams
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public decimal ValorTarifa { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class LinhaOnibusQueryResult
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal ValorTarifa { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}

public class LinhaOnibusFiltroParams
{
    public string? Descricao { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; } // default hoje no service se SomenteVigentes
}

public interface ILinhaOnibusRepository
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros);
    Task AtualizarAsync(LinhaOnibusAtualizarParams parametros);
    Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro);
}
```

- [ ] **Step 1: Create the files above (UTF-8 BOM)**

- [ ] **Step 2: Build Domain**

Run: `dotnet build src/Beneficios.Domain/Beneficios.Domain.csproj -v q`  
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add src/Beneficios.Domain/Models/LinhaOnibus*.cs src/Beneficios.Domain/Interfaces/ILinhaOnibusRepository.cs
git commit -m "feat(domain): add linha onibus persistence contracts"
```

---
