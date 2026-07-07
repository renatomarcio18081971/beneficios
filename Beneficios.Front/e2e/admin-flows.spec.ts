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
});
