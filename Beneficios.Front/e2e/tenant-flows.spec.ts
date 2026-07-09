import { expect, test } from '@playwright/test';
import {
  loginAdmin,
  loginTenant,
  mockTenantLogin,
  mockUnauthorized,
  mockUsuarioList,
  tenantSession,
} from './fixtures/api-mocks';

test.describe('Fluxos críticos tenant', () => {
  test('login tenant redireciona para dashboard', async ({ page }) => {
    await loginTenant(page);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('criar usuário na tenant', async ({ page }) => {
    await loginTenant(page);
    await page.getByRole('link', { name: 'Usuários' }).click();
    await page.waitForURL('**/usuarios');
    await page.getByRole('link', { name: /Novo usuário/i }).click();
    await page.getByLabel('Nome').fill('Novo Usuário');
    await page.getByLabel('Email').fill('novo@empresa1.com');
    await page.getByLabel('Senha').fill('senha123');
    await page.getByRole('button', { name: 'Salvar' }).click();
    await page.getByRole('button', { name: 'Sim, salvar' }).click();
    await page.waitForURL('**/usuarios');
    await expect(page.getByRole('heading', { name: 'Usuários' })).toBeVisible();
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
      sessionStorage.setItem('beneficios_user', JSON.stringify({
        usuarioId: session.usuarioId,
        nome: session.nome,
        email: session.email,
        perfil: session.perfil,
        empresaId: session.empresaId,
        empresaDominio: session.empresaDominio,
      }));
    }, tenantSession);

    await mockUnauthorized(page);
    await page.goto('/usuarios');
    await page.waitForURL('**/login');
    await expect(page.getByText('Sessão expirada. Faça login novamente.')).toBeVisible();
  });

  test('listagem carrega linhas após loading', async ({ page }) => {
    await mockTenantLogin(page);
    await mockUsuarioList(page, [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'João Silva',
        email: 'joao@empresa1.com',
        empresaId: tenantSession.empresaId,
        empresaNome: 'Empresa 1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ]);

    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify({
        usuarioId: session.usuarioId,
        nome: session.nome,
        email: session.email,
        perfil: session.perfil,
        empresaId: session.empresaId,
        empresaDominio: session.empresaDominio,
      }));
    }, tenantSession);

    await page.goto('/usuarios');
    await expect(page.getByText('João Silva')).toBeVisible({ timeout: 10000 });
  });

  test('exportar Excel na listagem de usuários', async ({ page }) => {
    await mockTenantLogin(page);
    await mockUsuarioList(page, [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'João',
        email: 'joao@empresa1.com',
        empresaId: tenantSession.empresaId,
        empresaNome: 'Empresa 1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ]);

    await page.addInitScript((session) => {
      sessionStorage.setItem('beneficios_token', session.token);
      sessionStorage.setItem('beneficios_user', JSON.stringify({
        usuarioId: session.usuarioId,
        nome: session.nome,
        email: session.email,
        perfil: session.perfil,
        empresaId: session.empresaId,
        empresaDominio: session.empresaDominio,
      }));
    }, tenantSession);

    const downloadPromise = page.waitForEvent('download');
    await page.goto('/usuarios');
    await page.getByRole('button', { name: /excel/i }).click();
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toMatch(/usuarios-empresa1-\d{4}-\d{2}-\d{2}\.xlsx/);
  });
});
