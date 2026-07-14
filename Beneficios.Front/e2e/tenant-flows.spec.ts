import { expect, test } from '@playwright/test';
import {
  loginTenant,
  mockDiasUteisMes,
  mockPerfilList,
  mockTenantLogin,
  mockTenantLoginSemPerfil,
  mockUnauthorized,
  mockUsuarioList,
  perfilDonoId,
  tenantSession,
  tenantUserStorage,
} from './fixtures/api-mocks';

test.describe('Fluxos críticos tenant', () => {
  test('login tenant redireciona para dashboard', async ({ page }) => {
    await loginTenant(page);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('login sem perfil exibe mensagem fixa', async ({ page }) => {
    await mockTenantLoginSemPerfil(page);
    await page.goto('/login');
    await page.getByLabel('Email').fill('semperfil@empresa1.com');
    await page.getByLabel('Senha').fill('senha123');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await expect(
      page.getByText('Usuário sem perfil configurado, procure o administrador do sistema !'),
    ).toBeVisible();
  });

  test('menu exibe Perfis para usuário dono', async ({ page }) => {
    await loginTenant(page);
    await expect(page.getByRole('link', { name: 'Perfis' })).toBeVisible();
  });

  test('abrir Calendário exibe grade mensal', async ({ page }) => {
    await mockDiasUteisMes(page);
    await loginTenant(page);
    await page.getByRole('link', { name: 'Calendário' }).click();
    await page.waitForURL('**/dias-uteis');
    await expect(page.getByRole('heading', { name: 'Dias úteis' })).toBeVisible();
    await expect(page.getByRole('grid', { name: 'Calendário do mês' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Dia 01' })).toBeVisible();
  });

  test('criar usuário na tenant', async ({ page }) => {
    await loginTenant(page);
    await page.getByRole('link', { name: 'Usuários' }).click();
    await page.waitForURL('**/usuarios');
    await page.getByRole('link', { name: /Novo usuário/i }).click();
    await expect(page.locator('input[formcontrolname="nome"]')).toBeVisible();

    // Aguarda opções de perfil (carregamento HTTP) antes de preencher o form
    const perfilSelect = page.locator('mat-select[formcontrolname="perfilId"]');
    await expect(perfilSelect).toBeVisible({ timeout: 10000 });
    await perfilSelect.click();
    await expect(page.getByRole('option', { name: 'Dono' })).toBeVisible({ timeout: 10000 });
    await page.getByRole('option', { name: 'Dono' }).click();
    await expect(perfilSelect).toContainText('Dono');

    await page.locator('input[formcontrolname="email"]').fill('novo@empresa1.com');
    await page.locator('input[formcontrolname="senha"]').fill('senha123');
    await page.locator('input[formcontrolname="nome"]').click();
    await page.locator('input[formcontrolname="nome"]').fill('Novo Usuário');
    await expect(page.locator('input[formcontrolname="nome"]')).toHaveValue('Novo Usuário');

    await page.locator('form button[type="submit"]').click();
    await expect(page.locator('mat-dialog-container')).toBeVisible({ timeout: 10000 });
    await page.locator('mat-dialog-container').getByRole('button', { name: 'Sim, salvar' }).click();
    await page.waitForURL('**/usuarios');
    await expect(page.getByRole('heading', { name: 'Usuários' })).toBeVisible();
  });

  test('criar perfil na tenant', async ({ page }) => {
    await loginTenant(page);
    await page.getByRole('link', { name: 'Perfis' }).click();
    await page.waitForURL('**/perfis');
    await page.getByRole('link', { name: /Novo perfil/i }).click();
    await expect(page.getByText('Novo perfil')).toBeVisible();

    await page.locator('input[formcontrolname="nome"]').fill('Operador');
    await page.locator('button[type="submit"]').click();
    await expect(page.getByRole('heading', { name: 'Confirma a operação' })).toBeVisible({ timeout: 10000 });
    await page.locator('mat-dialog-container').getByRole('button', { name: 'Sim, salvar' }).click();
    await page.waitForURL('**/perfis');
    await expect(page.getByRole('heading', { name: 'Perfis' })).toBeVisible();
  });

  test('logout redireciona para login', async ({ page }) => {
    await loginTenant(page);
    await page.getByRole('button', { name: /usuário tenant/i }).click();
    await page.getByRole('menuitem', { name: 'Sair' }).click();
    await page.waitForURL('**/login');
    await expect(page.getByRole('button', { name: 'Entrar' })).toBeVisible();
  });

  test('401 exibe snackbar e redireciona para login', async ({ page }) => {
    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify(session.user));
    }, { token: tenantSession.token, user: tenantUserStorage() });

    await mockUnauthorized(page);
    await page.goto('/usuarios');
    await page.waitForURL('**/login');
    await expect(page.getByText('Sessão expirada. Faça login novamente.')).toBeVisible();
  });

  test('listagem carrega linhas após loading', async ({ page }) => {
    await mockTenantLogin(page);
    await mockPerfilList(page);
    await mockUsuarioList(page, [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'João Silva',
        email: 'joao@empresa1.com',
        empresaId: tenantSession.empresaId,
        perfilId: perfilDonoId,
        perfilAcessoNome: 'Dono',
        empresaNome: 'Empresa 1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ]);

    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify(session.user));
    }, { token: tenantSession.token, user: tenantUserStorage() });

    await page.goto('/usuarios');
    await expect(page.getByText('João Silva')).toBeVisible({ timeout: 10000 });
  });

  test('exportar Excel na listagem de usuários', async ({ page }) => {
    await mockTenantLogin(page);
    await mockPerfilList(page);
    await mockUsuarioList(page, [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'João',
        email: 'joao@empresa1.com',
        empresaId: tenantSession.empresaId,
        perfilId: perfilDonoId,
        perfilAcessoNome: 'Dono',
        empresaNome: 'Empresa 1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ]);

    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify(session.user));
    }, { token: tenantSession.token, user: tenantUserStorage() });

    const downloadPromise = page.waitForEvent('download');
    await page.goto('/usuarios');
    await page.getByRole('button', { name: /excel/i }).click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toMatch(/usuarios-empresa1-\d{4}-\d{2}-\d{2}\.xlsx/);
  });

  test('listagem de perfis carrega linhas', async ({ page }) => {
    await mockTenantLogin(page);
    await mockPerfilList(page, [
      {
        id: perfilDonoId,
        nome: 'Dono',
        ehSistema: true,
        dataInclusao: '2024-01-01T00:00:00',
        dataAlteracao: null,
        permissoes: tenantSession.permissoes,
      },
    ]);

    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify(session.user));
    }, { token: tenantSession.token, user: tenantUserStorage() });

    await page.goto('/perfis');
    await expect(page.getByRole('heading', { name: 'Perfis' })).toBeVisible();
    await expect(page.getByText('Dono')).toBeVisible();
  });
});
