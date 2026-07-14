### Task 6: API `DiasUteisController` + DI + permissões

**Files:**
- Create: `src/Beneficios.Api/Controllers/DiasUteisController.cs`
- Modify: `src/Beneficios.Api/Program.cs` (DI repository/service/mapper)
- Test: `tests/Beneficios.Tests/Api/DiasUteisControllerTests.cs`

**Interfaces:**
- `GET /api/dias-uteis?ano=&mes=` → Visualizar  
- `GET /api/dias-uteis/{id}` → Visualizar  
- `PUT /api/dias-uteis/{id}` → Editar  
- `POST /api/dias-uteis/gerar-ano` body `{ "ano": 2027 }` → Criar; 400 com mensagem se já existe  
- `CodigoMenu = "dias_uteis"`; admin tenant bypass igual UsuariosController

- [ ] **Step 1: Write failing controller tests** (403 / happy path com mocks)

- [ ] **Step 2: Run — FAIL**

- [ ] **Step 3: Implement controller + DI**

- [ ] **Step 4: Run API tests — PASS**

- [ ] **Step 5: Commit**

```bash
git commit -m "feat(api): add dias-uteis endpoints with permission checks"
```

---

