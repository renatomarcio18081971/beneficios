import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { TenantService } from '../tenant/tenant.service';
import { TOKEN_KEY } from './auth.models';

describe('authGuard', () => {
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: TenantService,
          useValue: { getSubdomain: () => 'admin', getMode: () => 'admin', isAdminMode: () => true },
        },
      ],
    });
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  it('deve permitir quando autenticado', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });

  it('deve redirecionar para login quando nao autenticado', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
