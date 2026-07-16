import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';
import { tenantGuard } from './core/auth/tenant.guard';
import { guestGuard } from './core/auth/guest.guard';
import { permissaoGuard } from './core/auth/permissao.guard';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'recuperar-senha',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/recuperar-senha/recuperar-senha.component').then((m) => m.RecuperarSenhaComponent),
  },
  {
    path: 'redefinir-senha',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/redefinir-senha/redefinir-senha.component').then((m) => m.RedefinirSenhaComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'empresas',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/empresas/empresa-list/empresa-list.component').then((m) => m.EmpresaListComponent),
      },
      {
        path: 'empresas/nova',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/empresas/empresa-form/empresa-form.component').then((m) => m.EmpresaFormComponent),
      },
      {
        path: 'empresas/:id/editar',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/empresas/empresa-form/empresa-form.component').then((m) => m.EmpresaFormComponent),
      },
      {
        path: 'dashboard',
        canActivate: [tenantGuard],
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'usuarios',
        canActivate: [tenantGuard, permissaoGuard('usuarios')],
        loadComponent: () =>
          import('./features/usuarios/usuario-list/usuario-list.component').then((m) => m.UsuarioListComponent),
      },
      {
        path: 'usuarios/novo',
        canActivate: [tenantGuard, permissaoGuard('usuarios')],
        loadComponent: () =>
          import('./features/usuarios/usuario-form/usuario-form.component').then((m) => m.UsuarioFormComponent),
      },
      {
        path: 'usuarios/:id/editar',
        canActivate: [tenantGuard, permissaoGuard('usuarios')],
        loadComponent: () =>
          import('./features/usuarios/usuario-form/usuario-form.component').then((m) => m.UsuarioFormComponent),
      },
      {
        path: 'perfis',
        canActivate: [tenantGuard, permissaoGuard('perfis')],
        loadComponent: () =>
          import('./features/perfis/perfil-list/perfil-list.component').then((m) => m.PerfilListComponent),
      },
      {
        path: 'perfis/novo',
        canActivate: [tenantGuard, permissaoGuard('perfis')],
        loadComponent: () =>
          import('./features/perfis/perfil-form/perfil-form.component').then((m) => m.PerfilFormComponent),
      },
      {
        path: 'perfis/:id/editar',
        canActivate: [tenantGuard, permissaoGuard('perfis')],
        loadComponent: () =>
          import('./features/perfis/perfil-form/perfil-form.component').then((m) => m.PerfilFormComponent),
      },
      {
        path: 'calendario',
        canActivate: [tenantGuard, permissaoGuard('calendario')],
        loadComponent: () =>
          import('./features/calendario/calendario-page/calendario-page.component').then(
            (m) => m.CalendarioPageComponent,
          ),
      },
      {
        path: 'funcionarios',
        canActivate: [tenantGuard, permissaoGuard('funcionarios')],
        loadComponent: () =>
          import('./features/funcionarios/funcionario-list/funcionario-list.component').then(
            (m) => m.FuncionarioListComponent,
          ),
      },
      {
        path: 'funcionarios/novo',
        canActivate: [tenantGuard, permissaoGuard('funcionarios')],
        loadComponent: () =>
          import('./features/funcionarios/funcionario-form/funcionario-form.component').then(
            (m) => m.FuncionarioFormComponent,
          ),
      },
      {
        path: 'funcionarios/:id/editar',
        canActivate: [tenantGuard, permissaoGuard('funcionarios')],
        loadComponent: () =>
          import('./features/funcionarios/funcionario-form/funcionario-form.component').then(
            (m) => m.FuncionarioFormComponent,
          ),
      },
      {
        path: 'afastamentos',
        canActivate: [tenantGuard, permissaoGuard('afastamentos')],
        loadComponent: () =>
          import('./features/afastamentos/afastamento-list/afastamento-list.component').then(
            (m) => m.AfastamentoListComponent,
          ),
      },
      {
        path: 'afastamentos/novo',
        canActivate: [tenantGuard, permissaoGuard('afastamentos')],
        loadComponent: () =>
          import('./features/afastamentos/afastamento-form/afastamento-form.component').then(
            (m) => m.AfastamentoFormComponent,
          ),
      },
      {
        path: 'afastamentos/:id/editar',
        canActivate: [tenantGuard, permissaoGuard('afastamentos')],
        loadComponent: () =>
          import('./features/afastamentos/afastamento-form/afastamento-form.component').then(
            (m) => m.AfastamentoFormComponent,
          ),
      },
      { path: '', pathMatch: 'full', redirectTo: 'login' },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
