### Task 7: Desativar VT encerra vínculos abertos

**Files:**
- Modify: `src/Beneficios.Application/Services/FuncionarioService.cs`
- Modify: `tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs` (ou criar se insuficiente)
- Consumes: `IFuncionarioLinhaRepository.EncerrarAbertosPorFuncionarioAsync`

**Interfaces:**
- Após `AtualizarAsync`/`SalvarAsync` persistir benefícios: se a lista normalizada **não** contém VT ativo (ausente ou `Ativo == false`), chamar:

```csharp
await _funcionarioLinhaRepository.EncerrarAbertosPorFuncionarioAsync(
    funcionarioId,
    DateOnly.FromDateTime(DateTime.UtcNow),
    DateTime.UtcNow,
    usuarioAlteracaoId);
```

Injetar `IFuncionarioLinhaRepository` no `FuncionarioService` (ou método no `IFuncionarioLinhaService` `EncerrarAbertosPorFuncionarioAsync` se preferir não acoplar Application→repo de outro agregado — **preferir** método no `IFuncionarioLinhaService` para manter o padrão de serviços).

Se usar service:

```csharp
Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId);
```

- [ ] **Step 1: Write failing test** — atualizar funcionário com VT `ativo: false` → verifica chamada a encerrar vínculos

- [ ] **Step 2: Run — expect FAIL**

- [ ] **Step 3: Implement hook no `FuncionarioService`**

- [ ] **Step 4: Run FuncionarioService + FuncionarioLinha tests — expect PASS**

- [ ] **Step 5: Commit**

```bash
git add src/Beneficios.Application/Services/FuncionarioService.cs src/Beneficios.Application/Services/FuncionarioLinhaService.cs src/Beneficios.Application/Interfaces/IFuncionarioLinhaService.cs tests/Beneficios.Tests/Application/FuncionarioServiceTests.cs
git commit -m "feat: close open bus-line links when VT is deactivated"
```

---
