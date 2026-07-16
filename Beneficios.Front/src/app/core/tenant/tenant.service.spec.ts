import { TestBed } from '@angular/core/testing';
import { TenantService } from './tenant.service';

describe('TenantService', () => {
  let service: TenantService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TenantService);
  });

  it('admin.localhost deve retornar modo admin', () => {
    expect(service.getSubdomain('admin.localhost')).toBe('admin');
    expect(service.getMode('admin.localhost')).toBe('admin');
    expect(service.isAdminMode('admin.localhost')).toBeTrue();
  });

  it('empresa1.localhost deve retornar modo tenant', () => {
    expect(service.getSubdomain('empresa1.localhost')).toBe('empresa1');
    expect(service.getMode('empresa1.localhost')).toBe('tenant');
    expect(service.isAdminMode('empresa1.localhost')).toBeFalse();
  });
});
