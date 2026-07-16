### Task 10: Cobertura ≥ 80%, E2E smoke e verificação final

**Files:**
- Expandir testes Application/Api/Repository conforme gaps do `scripts/check-coverage.ps1`
- Opcional: `e2e/fixtures/api-mocks.ts` + `e2e/tenant-flows.spec.ts` — mocks das novas rotas e smoke mínimo

- [ ] **Step 1: Run backend tests + coverage**

Run: `dotnet test tests/Beneficios.Tests/Beneficios.Tests.csproj -v q`  
Run: `powershell -ExecutionPolicy Bypass -File scripts/check-coverage.ps1`  
Expected: PASS com cobertura ≥ 80%

- [ ] **Step 2: Fill gaps** — testes faltantes em services/controllers/repos até passar o script

- [ ] **Step 3: Front build + unit tests relevantes**

Run: `cd Beneficios.Front; npx ng build; npx ng test --watch=false --browsers=ChromeHeadless` (ou o comando que o repo usa)

- [ ] **Step 4: Commit**

```bash
git add tests/ Beneficios.Front/e2e/
git commit -m "test: expand linhas-onibus coverage to meet Sonar threshold"
```

---
