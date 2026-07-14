import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissaoService } from './permissao.service';
import { TenantService } from '../tenant/tenant.service';

export function permissaoGuard(codigoMenu: string): CanActivateFn {
  return () => {
    const permissao = inject(PermissaoService);
    const tenant = inject(TenantService);
    const router = inject(Router);

    if (tenant.isAdminMode()) {
      return router.createUrlTree(['/empresas']);
    }

    if (permissao.possuiPermissao(codigoMenu, 'visualizar')) {
      return true;
    }

    return router.createUrlTree(['/dashboard']);
  };
}
