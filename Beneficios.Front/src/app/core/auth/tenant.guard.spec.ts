import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { tenantGuard } from './tenant.guard';
import { TenantService } from '../tenant/tenant.service';
import { TOKEN_KEY, USER_KEY } from './auth.models';

describe('tenantGuard', () => {
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: TenantService,
          useValue: {
            isAdminMode: () => false,
            getSubdomain: () => 'empresa1',
            getMode: () => 'tenant' as const,
          },
        },
      ],
    });
    router = TestBed.inject(Router);
  });

  it('deve permitir usuario da tenant correta', () => {
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

    const result = TestBed.runInInjectionContext(() => tenantGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });

  it('deve redirecionar quando dominio nao confere', () => {
    sessionStorage.setItem(TOKEN_KEY, 'jwt-token');
    sessionStorage.setItem(
      USER_KEY,
      JSON.stringify({
        usuarioId: '1',
        nome: 'User',
        email: 'u@test.com',
        perfil: 'Empresa',
        empresaDominio: 'outra',
      }),
    );

    const result = TestBed.runInInjectionContext(() => tenantGuard({} as never, {} as never));
    expect(result).toEqual(router.createUrlTree(['/login']));
  });
});
