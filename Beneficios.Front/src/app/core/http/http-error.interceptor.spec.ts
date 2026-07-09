import { HttpErrorResponse, provideHttpClient, withInterceptors, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { httpErrorInterceptor, httpErrorMessages, shouldHandle } from './http-error.interceptor';
import { authInterceptor } from '../auth/auth.interceptor';
import { HttpClient } from '@angular/common/http';
import { TenantService } from '../tenant/tenant.service';

describe('httpErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let snackBar: jasmine.SpyObj<MatSnackBar>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    snackBar = jasmine.createSpyObj('MatSnackBar', ['open']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withXhr(), withInterceptors([authInterceptor, httpErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBar },
        { provide: Router, useValue: router },
        {
          provide: TenantService,
          useValue: { getSubdomain: () => 'empresa1', getMode: () => 'tenant', isAdminMode: () => false },
        },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('shouldHandle deve ignorar login e status sem mensagem', () => {
    expect(shouldHandle(new HttpErrorResponse({ status: 401 }), '/api/usuarios/login')).toBeFalse();
    expect(shouldHandle(new HttpErrorResponse({ status: 400 }), '/api/usuarios')).toBeFalse();
  });

  it('deve exibir snackbar em 403', () => {
    http.get('/api/usuarios').subscribe({ error: () => undefined });

    const req = httpMock.expectOne('/api/usuarios');
    req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });

    expect(snackBar.open).toHaveBeenCalledWith(httpErrorMessages[403], 'Fechar', { duration: 5000 });
  });

  it('deve exibir snackbar e redirecionar em 401 fora do login', () => {
    http.get('/api/usuarios').subscribe({ error: () => undefined });

    const req = httpMock.expectOne('/api/usuarios');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(snackBar.open).toHaveBeenCalledWith(httpErrorMessages[401], 'Fechar', { duration: 5000 });
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('nao deve exibir snackbar global em 401 do login', () => {
    http.post('/api/usuarios/login', {}).subscribe({ error: () => undefined });

    const req = httpMock.expectOne('/api/usuarios/login');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(snackBar.open).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
