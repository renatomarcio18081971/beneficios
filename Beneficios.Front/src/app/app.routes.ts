import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';
import { tenantGuard } from './core/auth/tenant.guard';
import { guestGuard } from './core/auth/guest.guard';
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
        canActivate: [tenantGuard],
        loadComponent: () =>
          import('./features/usuarios/usuario-list/usuario-list.component').then((m) => m.UsuarioListComponent),
      },
      {
        path: 'usuarios/novo',
        canActivate: [tenantGuard],
        loadComponent: () =>
          import('./features/usuarios/usuario-form/usuario-form.component').then((m) => m.UsuarioFormComponent),
      },
      {
        path: 'usuarios/:id/editar',
        canActivate: [tenantGuard],
        loadComponent: () =>
          import('./features/usuarios/usuario-form/usuario-form.component').then((m) => m.UsuarioFormComponent),
      },
      { path: '', pathMatch: 'full', redirectTo: 'login' },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
