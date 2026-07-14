import { Page, Route } from '@playwright/test';

const API_BASE = 'http://localhost:5000/api';

const fullPermissoes = [
  { codigoMenu: 'usuarios', visualizar: true, criar: true, editar: true, excluir: true },
  { codigoMenu: 'perfis', visualizar: true, criar: true, editar: true, excluir: true },
  { codigoMenu: 'dias_uteis', visualizar: true, criar: true, editar: true, excluir: true },
  { codigoMenu: 'funcionarios', visualizar: true, criar: true, editar: true, excluir: true },
];

export const perfilDonoId = '44444444-4444-4444-4444-444444444444';

export const tenantSession = {
  token: 'tenant-jwt-token',
  usuarioId: '11111111-1111-1111-1111-111111111111',
  nome: 'Usuário Tenant',
  email: 'tenant@empresa1.com',
  perfil: 'Empresa',
  empresaId: '22222222-2222-2222-2222-222222222222',
  empresaDominio: 'empresa1',
  perfilAcessoId: perfilDonoId,
  perfilAcessoNome: 'Dono',
  permissoes: fullPermissoes,
};

export const adminSession = {
  token: 'admin-jwt-token',
  usuarioId: '99999999-9999-9999-9999-999999999999',
  nome: 'Administrador',
  email: 'admin@exemplo.com',
  perfil: 'Admin',
  empresaId: null,
  empresaDominio: 'admin',
  perfilAcessoId: null,
  perfilAcessoNome: '',
  permissoes: [],
};

/** Payload de sessão no sessionStorage (espelha UserSession do front). */
export function tenantUserStorage(session = tenantSession) {
  return {
    usuarioId: session.usuarioId,
    nome: session.nome,
    email: session.email,
    perfil: session.perfil,
    empresaId: session.empresaId,
    empresaDominio: session.empresaDominio,
    perfilAcessoId: session.perfilAcessoId,
    perfilAcessoNome: session.perfilAcessoNome,
    permissoes: session.permissoes,
  };
}

export async function mockTenantLogin(page: Page): Promise<void> {
  await page.route(`${API_BASE}/usuarios/login`, async (route: Route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(tenantSession),
    });
  });
}

export async function mockTenantLoginSemPerfil(page: Page): Promise<void> {
  await page.route(`${API_BASE}/usuarios/login`, async (route: Route) => {
    await route.fulfill({
      status: 401,
      contentType: 'application/json',
      body: JSON.stringify({
        message: 'Usuário sem perfil configurado, procure o administrador do sistema !',
      }),
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

export async function mockPerfilList(
  page: Page,
  perfis: unknown[] = [
    {
      id: perfilDonoId,
      nome: 'Dono',
      ehSistema: true,
      dataInclusao: '2024-01-01T00:00:00',
      dataAlteracao: null,
      permissoes: fullPermissoes,
    },
  ],
): Promise<void> {
  await page.route(`${API_BASE}/perfis`, async (route: Route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(perfis),
      });
      return;
    }

    if (route.request().method() === 'POST') {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({ id: '55555555-5555-5555-5555-555555555555' }),
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

export async function mockDiasUteisMes(page: Page, dias?: unknown[]): Promise<void> {
  const amostra =
    dias ??
    [
      {
        id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        data: '2026-07-01',
        ehDiaUtil: true,
        tipoExcecao: null,
        origem: 'Geracao',
        observacao: null,
      },
      {
        id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
        data: '2026-07-02',
        ehDiaUtil: true,
        tipoExcecao: null,
        origem: 'Geracao',
        observacao: null,
      },
      {
        id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
        data: '2026-07-03',
        ehDiaUtil: false,
        tipoExcecao: 'Nacional',
        origem: 'Nacional',
        observacao: 'Independência do Brasil',
      },
    ];

  await page.route(`${API_BASE}/dias-uteis**`, async (route: Route) => {
    if (route.request().method() === 'GET' && !route.request().url().match(/dias-uteis\/[^/?]+$/)) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(amostra),
      });
      return;
    }
    await route.continue();
  });
}

export async function mockFuncionarioList(page: Page, funcionarios: unknown[] = []): Promise<void> {
  await page.route(`${API_BASE}/funcionarios**`, async (route: Route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(funcionarios),
      });
      return;
    }
    await route.continue();
  });
}

export async function loginTenant(page: Page): Promise<void> {
  await mockTenantLogin(page);
  await mockUsuarioList(page);
  await mockPerfilList(page);
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
