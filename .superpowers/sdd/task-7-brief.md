### Task 7: Provisionamento + garantia de ano (startup + hosted service)

**Files:**
- Create: `src/Beneficios.Domain/Interfaces/ICalendarioAnoGarantia.cs`
- Create: `src/Beneficios.Infrastructure/Tenancy/CalendarioAnoGarantia.cs`
- Create: `src/Beneficios.Api/Background/CalendarioAnoHostedService.cs`
- Modify: `TenantProvisioner.cs` — após DDL calendário, gerar ano corrente no schema (via SQL/lote no mesmo connection **ou** helper estático compartilhado com a garantia)
- Modify: `Program.cs` — `AddHostedService` + try/catch startup `GarantirAnoCorrenteEmTenantsExistentesAsync`
- Test: `tests/Beneficios.Tests/Infrastructure/CalendarioAnoGarantiaTests.cs` e/ou extensão `TenantProvisionerTests`

**Interfaces:**
- Produces: `Task GarantirAnoCorrenteEmTenantsExistentesAsync(CancellationToken ct = default)`  
  - Lista `tenant_%`  
  - Para cada schema: `search_path` / SQL qualified: se não há dias no ano UTC corrente → gerar  
- HostedService: delay inicial curto + loop a cada 24h; falhas logadas

**Nota de design:** Gerar ano no provisioner pode duplicar lógica do service. Preferir extrair `CalendarioAnoGerador` interno (Infrastructure) usado por provisioner (connection explícita + schema) **e** por `CalendarioAnoGarantia`, enquanto `CalendarioDiaService` usa o gerador via repository do request scope. Alternativa aceitável: no provisioner abrir connection com SearchPath do tenant e resolver `ICalendarioDiaService` não funciona facilmente — então **gerador estático/infra** compartilhado é a opção recomendada nesta task.

- [ ] **Step 1: Write failing tests** (tenant sem tabela dias do ano → garantia cria; segundo run não duplica)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement gerador compartilhado + provisioner + garantia + hosted service + Program.cs**

- [ ] **Step 4: Run tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat: provision and yearly job for tenant calendars"
```

---

