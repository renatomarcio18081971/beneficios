import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { TOKEN_KEY } from './auth.models';
import { TenantService } from '../tenant/tenant.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: AuthService;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withXhr(), withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        {
          provide: TenantService,
          useValue: { getSubdomain: () => 'empresa1', getMode: () => 'tenant', isAdminMode: () => false },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('deve adicionar Authorization e X-Tenant', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');

    http.get('/api/usuarios').subscribe();

    const req = httpMock.expectOne('/api/usuarios');
    expect(req.request.headers.get('Authorization')).toBe('Bearer jwt-token');
    expect(req.request.headers.get('X-Tenant')).toBe('empresa1');
    req.flush([]);
  });

  it('deve fazer logout em 401', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    spyOn(authService, 'logout').and.callThrough();

    http.get('/api/usuarios').subscribe({ error: () => undefined });

    const req = httpMock.expectOne('/api/usuarios');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
    expect(sessionStorage.getItem(TOKEN_KEY)).toBeNull();
  });
});
