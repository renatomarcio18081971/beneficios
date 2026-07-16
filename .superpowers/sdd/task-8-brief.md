### Task 8: Frontend — Linhas de Ônibus

**Files:**
- Create: `Beneficios.Front/src/app/core/api/linha-onibus.models.ts`
- Create: `Beneficios.Front/src/app/core/api/linha-onibus.service.ts`
- Create: `features/linhas-onibus/linha-onibus-list/*` (ts/html/scss)
- Create: `features/linhas-onibus/linha-onibus-form/*`
- Modify: `app.routes.ts`

**Layout:** copiar estrutura visual de `afastamento-list` / `afastamento-form` (ou `funcionario-list`/`funcionario-form`) — mesmos padrões Material, filtros, botões. **Não** inventar layout novo. Usar reactive forms como nos cadastros existentes. Skill `angular-developer` + Context7 para dúvidas de Angular 22.

Models:

```typescript
export interface LinhaOnibusDto {
  id: string;
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}

export interface LinhaOnibusSalvarDto {
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}
```

Service: `GET/POST api/linhas-onibus`, `GET/PUT api/linhas-onibus/:id`.

Rotas:

```typescript
{
  path: 'linhas-onibus',
  canActivate: [tenantGuard, permissaoGuard('linhas_onibus')],
  loadComponent: () => import('...linha-onibus-list...').then(m => m.LinhaOnibusListComponent),
},
// nova + :id/editar idem
```

Listagem: colunas Descrição, Data início, Data fim (“Em aberto”), Tarifa; Novo/Editar; **sem** Excluir.  
Form: descrição, datas, tarifa ≥ 0.

- [ ] **Step 1: Scaffold list + form + service + routes (BOM)**

- [ ] **Step 2: `npx ng build` no `Beneficios.Front` — expect PASS**

- [ ] **Step 3: Commit**

```bash
git add Beneficios.Front/src/app/core/api/linha-onibus.* Beneficios.Front/src/app/features/linhas-onibus Beneficios.Front/src/app/app.routes.ts
git commit -m "feat(front): add linhas-onibus list and form"
```

---
