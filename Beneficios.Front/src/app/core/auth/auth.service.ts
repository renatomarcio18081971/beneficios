import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TenantService } from '../tenant/tenant.service';
import { LoginResponse, TOKEN_KEY, USER_KEY, UserSession } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tenantService = inject(TenantService);

  login(email: string, senha: string): Observable<LoginResponse> {
    const subdomain = this.tenantService.getSubdomain();
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/usuarios/login`, { email, senha }, {
        headers: { 'X-Tenant': subdomain },
      })
      .pipe(
        tap((response) => {
          const session: UserSession = {
            usuarioId: response.usuarioId,
            nome: response.nome,
            email: response.email,
            perfil: response.perfil,
            empresaId: response.empresaId,
            empresaDominio: response.empresaDominio,
          };
          sessionStorage.setItem(TOKEN_KEY, response.token);
          sessionStorage.setItem(USER_KEY, JSON.stringify(session));
        }),
      );
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
  }

  getToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  currentUser(): UserSession | null {
    const raw = sessionStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    return JSON.parse(raw) as UserSession;
  }
}
