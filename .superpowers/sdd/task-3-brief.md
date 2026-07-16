### Task 3: Domain models e interfaces — Funcionário × Linha

**Files:**
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaSalvarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaAtualizarParams.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaQueryResult.cs`
- Create: `src/Beneficios.Domain/Models/FuncionarioLinhaFiltroParams.cs`
- Create: `src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs`

**Interfaces:**
- Produces:

```csharp
public class FuncionarioLinhaSalvarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class FuncionarioLinhaAtualizarParams
{
    public Guid Id { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}

public class FuncionarioLinhaQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public Guid LinhaOnibusId { get; set; }
    public string LinhaDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}

public class FuncionarioLinhaFiltroParams
{
    public Guid? FuncionarioId { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; }
}

public interface IFuncionarioLinhaRepository
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros);
    Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros);
    Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro);
    Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null);
    Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId);
    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId);
}
```

- [ ] **Step 1: Create files (UTF-8 BOM)**

- [ ] **Step 2: Build Domain — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add src/Beneficios.Domain/Models/FuncionarioLinha*.cs src/Beneficios.Domain/Interfaces/IFuncionarioLinhaRepository.cs
git commit -m "feat(domain): add funcionario-linha persistence contracts"
```

---
