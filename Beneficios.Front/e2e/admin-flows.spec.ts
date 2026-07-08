import { expect, test } from '@playwright/test';
import { loginAdmin, mockAdminLogin } from './fixtures/api-mocks';

test.describe('Fluxos críticos admin', () => {
  test('login admin redireciona para empresas', async ({ page }) => {
    await loginAdmin(page);
    await expect(page.getByRole('heading', { name: 'Empresas' })).toBeVisible();
  });

  test('listagem carrega linhas após loading', async ({ page }) => {
    await mockAdminLogin(page);
    await page.route('**/api/empresas', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: '11111111-1111-1111-1111-111111111111',
              razaoSocial: 'Empresa A',
              dominio: 'a',
              dataInclusao: '2024-01-15T10:30:00',
            },
          ]),
        });
        return;
      }

      await route.continue();
    });

    await page.goto('/login');
    await page.getByLabel('Email').fill('admin@exemplo.com');
    await page.getByLabel('Senha').fill('admin123');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await page.waitForURL('**/empresas');
    await expect(page.getByText('Empresa A')).toBeVisible({ timeout: 10000 });
  });

  test('edição carrega formulário após loading', async ({ page }) => {
    const empresaId = '11111111-1111-1111-1111-111111111111';

    await mockAdminLogin(page);
    await page.route('**/api/empresas**', async (route) => {
      const url = route.request().url();
      const method = route.request().method();

      if (method === 'GET' && url.endsWith(`/api/empresas/${empresaId}`)) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: empresaId,
            razaoSocial: 'Empresa A',
            dominio: 'a',
            dataInclusao: '2024-01-15T10:30:00',
            dataAlteracao: null,
          }),
        });
        return;
      }

      if (method === 'GET' && url.endsWith('/api/empresas')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: empresaId,
              razaoSocial: 'Empresa A',
              dominio: 'a',
              dataInclusao: '2024-01-15T10:30:00',
            },
          ]),
        });
        return;
      }

      await route.continue();
    });

    await page.goto('/login');
    await page.getByLabel('Email').fill('admin@exemplo.com');
    await page.getByLabel('Senha').fill('admin123');
    await page.getByRole('button', { name: 'Entrar' }).click();
    await page.waitForURL('**/empresas');
    await page.getByRole('button', { name: 'Editar' }).first().click();
    await page.waitForURL(`**/empresas/${empresaId}/editar`);
    await expect(page.getByLabel('Razão Social')).toHaveValue('Empresa A', { timeout: 10000 });
    await expect(page.getByLabel('Domínio')).toHaveValue('a');
  });
});
