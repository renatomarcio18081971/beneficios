import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { TenantService } from '../tenant/tenant.service';

export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const tenantService = inject(TenantService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const user = authService.currentUser();
  if (!tenantService.isAdminMode() || user?.perfil !== 'Admin') {
    authService.logout();
    return router.createUrlTree(['/login']);
  }

  return true;
};
