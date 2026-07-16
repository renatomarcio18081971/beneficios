import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { guestGuard } from './guest.guard';
import { TenantService } from '../tenant/tenant.service';
import { TOKEN_KEY, USER_KEY } from './auth.models';

describe('guestGuard', () => {
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

  it('deve permitir visitante nao autenticado', () => {
    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });

  it('deve redirecionar admin logado para empresas', () => {
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

    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toEqual(router.createUrlTree(['/empresas']));
  });
});
