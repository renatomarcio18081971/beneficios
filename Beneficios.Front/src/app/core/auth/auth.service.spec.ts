import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { TOKEN_KEY, USER_KEY } from './auth.models';
import { environment } from '../../../environments/environment';
import { TenantService } from '../tenant/tenant.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: TenantService,
          useValue: { getSubdomain: () => 'empresa1', getMode: () => 'tenant', isAdminMode: () => false },
        },
      ],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('login deve persistir token e usuario na sessao', () => {
    const response = {
      token: 'jwt-token',
      usuarioId: '11111111-1111-1111-1111-111111111111',
      nome: 'Joao',
      email: 'joao@example.com',
      perfil: 'Empresa' as const,
      empresaId: '22222222-2222-2222-2222-222222222222',
      empresaDominio: 'empresa1',
    };

    service.login('joao@example.com', 'senha123').subscribe((result) => {
      expect(result.token).toBe('jwt-token');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/usuarios/login`);
    expect(req.request.headers.get('X-Tenant')).toBe('empresa1');
    req.flush(response);

    expect(sessionStorage.getItem(TOKEN_KEY)).toBe('jwt-token');
    expect(sessionStorage.getItem(USER_KEY)).toContain('joao@example.com');
    expect(service.isAuthenticated()).toBeTrue();
  });

  it('logout deve limpar sessao', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    sessionStorage.setItem(USER_KEY, '{}');

    service.logout();

    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(sessionStorage.getItem(USER_KEY)).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
  });
});
