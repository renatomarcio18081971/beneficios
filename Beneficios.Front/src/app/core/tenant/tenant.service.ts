import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export type TenantMode = 'admin' | 'tenant';

@Injectable({ providedIn: 'root' })
export class TenantService {
  getSubdomain(hostname: string = window.location.hostname): string {
    const suffix = `.${environment.baseDomain}`;

    if (!hostname.endsWith(suffix) && hostname !== environment.baseDomain) {
      return hostname.split('.')[0] || 'admin';
    }

    const subdomain = hostname.slice(0, -suffix.length);
    return subdomain || 'admin';
  }

  getMode(hostname?: string): TenantMode {
    return this.getSubdomain(hostname).toLowerCase() === 'admin' ? 'admin' : 'tenant';
  }

  isAdminMode(hostname?: string): boolean {
    return this.getMode(hostname) === 'admin';
  }
}
