import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EmpresaService } from './empresa.service';
import { environment } from '../../../environments/environment';

describe('EmpresaService', () => {
  let service: EmpresaService;
  let httpMock: HttpTestingController;

  const baseUrl = `${environment.apiUrl}/empresas`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(EmpresaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('list deve buscar todas as empresas', () => {
    const empresas = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        razaoSocial: 'Empresa 1 LTDA',
        dominio: 'empresa1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ];

    service.list().subscribe((result) => {
      expect(result).toEqual(empresas);
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(empresas);
  });

  it('getById deve buscar empresa por id', () => {
    const empresa = {
      id: '11111111-1111-1111-1111-111111111111',
      razaoSocial: 'Empresa 1 LTDA',
      dominio: 'empresa1',
      dataInclusao: '2024-01-15T10:30:00',
      dataAlteracao: null,
    };

    service.getById(empresa.id).subscribe((result) => {
      expect(result).toEqual(empresa);
    });

    const req = httpMock.expectOne(`${baseUrl}/${empresa.id}`);
    expect(req.request.method).toBe('GET');
    req.flush(empresa);
  });

  it('create deve enviar POST com payload completo', () => {
    const payload = {
      razaoSocial: 'Nova Empresa LTDA',
      dominio: 'novaempresa',
    };

    service.create(payload).subscribe((result) => {
      expect(result.id).toBe('33333333-3333-3333-3333-333333333333');
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ id: '33333333-3333-3333-3333-333333333333' });
  });

  it('update deve enviar PUT', () => {
    const id = '11111111-1111-1111-1111-111111111111';
    const payload = {
      razaoSocial: 'Empresa Atualizada LTDA',
      dominio: 'empresa1',
    };

    service.update(id, payload).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/${id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush(null);
  });

  it('delete deve enviar DELETE', () => {
    const id = '11111111-1111-1111-1111-111111111111';

    service.delete(id).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/${id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
