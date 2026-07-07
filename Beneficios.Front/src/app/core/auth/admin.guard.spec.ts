import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { adminGuard } from './admin.guard';
import { TenantService } from '../tenant/tenant.service';
import { TOKEN_KEY, USER_KEY } from './auth.models';

describe('adminGuard', () => {
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: TenantService,
          useValue: {
            isAdminMode: () => true,
            getSubdomain: () => 'admin',
            getMode: () => 'admin' as const,
          },
        },
      ],
    });
    router = TestBed.inject(Router);
  });

  it('deve permitir admin autenticado em modo admin', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    sessionStorage.setItem(
      USER_KEY,
      JSON.stringify({
        usuarioId: '1',
        nome: 'Admin',
        email: 'admin@test.com',
        perfil: 'Admin',
        empresaDominio: '',
      }),
    );

    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });

  it('deve redirecionar usuario empresa em modo admin', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    sessionStorage.setItem(
      USER_KEY,
      JSON.stringify({
        usuarioId: '1',
        nome: 'User',
        email: 'u@test.com',
        perfil: 'Empresa',
        empresaDominio: 'empresa1',
      }),
    );

    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
