import { Page, Route } from '@playwright/test';

const API_BASE = 'http://localhost:5000/api';

export const tenantSession = {
  token: 'tenant-jwt-token',
  usuarioId: '11111111-1111-1111-1111-111111111111',
  nome: 'Usuário Tenant',
  email: 'tenant@empresa1.com',
  perfil: 'Empresa',
  empresaId: '22222222-2222-2222-2222-222222222222',
  empresaDominio: 'empresa1',
};

export const adminSession = {
  token: 'admin-jwt-token',
  usuarioId: '99999999-9999-9999-9999-999999999999',
  nome: 'Administrador',
  email: 'admin@exemplo.com',
  perfil: 'Admin',
  empresaId: null,
  empresaDominio: 'admin',
};

export async function mockTenantLogin(page: Page): Promise<void> {
  await page.route(`${API_BASE}/usuarios/login`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(tenantSession),
    });
  });
}

export async function mockAdminLogin(page: Page): Promise<void> {
  await page.route(`${API_BASE}/usuarios/login`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(adminSession),
    });
  });
}

export async function mockUsuarioList(page: Page, usuarios: unknown[] = []): Promise<void> {
  await page.route(`${API_BASE}/usuarios`, async (route: Route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(usuarios),
      });
      return;
    }

    if (route.request().method() === 'POST') {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({ id: '33333333-3333-3333-3333-333333333333' }),
      });
      return;
    }

    await route.continue();
  });
}

export async function mockEmpresaList(page: Page): Promise<void> {
  await page.route(`${API_BASE}/empresas`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    });
  });
}

export async function mockUnauthorized(page: Page): Promise<void> {
  await page.route(`${API_BASE}/usuarios`, async (route: Route) => {
    await route.fulfill({
      status: 401,
      contentType: 'application/json',
      body: JSON.stringify({ message: 'Unauthorized' }),
    });
  });
}

export async function loginTenant(page: Page): Promise<void> {
  await mockTenantLogin(page);
  await mockUsuarioList(page);
  await page.goto('/login');
  await page.getByLabel('Email').fill('tenant@empresa1.com');
  await page.getByLabel('Senha').fill('senha123');
  await page.getByRole('button', { name: 'Entrar' }).click();
  await page.waitForURL('**/dashboard');
}

export async function loginAdmin(page: Page): Promise<void> {
  await mockAdminLogin(page);
  await mockEmpresaList(page);
  await page.goto('/login');
  await page.getByLabel('Email').fill('admin@exemplo.com');
  await page.getByLabel('Senha').fill('admin123');
  await page.getByRole('button', { name: 'Entrar' }).click();
  await page.waitForURL('**/empresas');
}
