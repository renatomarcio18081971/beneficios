import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from './permission.service';
import { TenantService } from '../tenant/tenant.service';

export function permissionGuard(codigoMenu: string): CanActivateFn {
  return () => {
    const permission = inject(PermissionService);
    const tenant = inject(TenantService);
    const router = inject(Router);

    if (tenant.isAdminMode()) {
      return router.createUrlTree(['/empresas']);
    }

    if (permission.can(codigoMenu, 'visualizar')) {
      return true;
    }

    return router.createUrlTree(['/dashboard']);
  };
}
