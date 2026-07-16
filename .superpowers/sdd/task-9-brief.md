### Task 9: Frontend — Funcionário × Linhas + atalho

**Files:**
- Create: `funcionario-linha.models.ts`, `funcionario-linha.service.ts`, list, form
- Modify: `app.routes.ts`
- Modify: `funcionario-list.component.ts/html`
- Modify: `funcionario-form.component.html` (link como afastamentos)

Atalho listagem (espelhar afastamentos):

```typescript
readonly podeVerLinhas = this.permissao.possuiPermissao('funcionario_linhas', 'visualizar');
```

```html
@if (podeVerLinhas) {
  <a mat-button [routerLink]="['/funcionario-linhas']" [queryParams]="{ funcionarioId: row.id }">Linhas</a>
}
```

Form edição: link `Ver linhas de ônibus` com `funcionarioId: id()`.

Listagem vínculos: lê `funcionarioId` da query; colunas Funcionário, Linha, Qtd, datas; Novo/Editar sem Excluir.  
Form: funcionário, linha (filtrar vigentes via `linhas-onibus?somenteVigentes=true` ou filtro client), quantidade ≥ 1, datas. Pré-selecionar funcionário se query na rota `/novo`.

- [ ] **Step 1: Implement list/form/service/routes + atalhos**

- [ ] **Step 2: `npx ng build` — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add Beneficios.Front/src/app/core/api/funcionario-linha.* Beneficios.Front/src/app/features/funcionario-linhas Beneficios.Front/src/app/app.routes.ts Beneficios.Front/src/app/features/funcionarios
git commit -m "feat(front): add funcionario-linhas UI and funcionario shortcuts"
```

---
