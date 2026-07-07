import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UsuarioService } from './usuario.service';
import { environment } from '../../../environments/environment';

describe('UsuarioService', () => {
  let service: UsuarioService;
  let httpMock: HttpTestingController;

  const baseUrl = `${environment.apiUrl}/usuarios`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(UsuarioService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('list deve buscar todos os usuarios', () => {
    const usuarios = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'Joao',
        email: 'joao@example.com',
        empresaId: '22222222-2222-2222-2222-222222222222',
        empresaNome: 'Empresa 1',
        dataInclusao: '2024-01-15T10:30:00',
        dataAlteracao: null,
      },
    ];

    service.list().subscribe((result) => {
      expect(result).toEqual(usuarios);
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(usuarios);
  });

  it('getById deve buscar usuario por id', () => {
    const usuario = {
      id: '11111111-1111-1111-1111-111111111111',
      nome: 'Joao',
      email: 'joao@example.com',
      empresaId: '22222222-2222-2222-2222-222222222222',
      empresaNome: 'Empresa 1',
      dataInclusao: '2024-01-15T10:30:00',
      dataAlteracao: null,
    };

    service.getById(usuario.id).subscribe((result) => {
      expect(result).toEqual(usuario);
    });

    const req = httpMock.expectOne(`${baseUrl}/${usuario.id}`);
    expect(req.request.method).toBe('GET');
    req.flush(usuario);
  });

  it('create deve enviar POST com payload completo', () => {
    const payload = {
      nome: 'Maria',
      senha: 'senha123',
      email: 'maria@example.com',
      empresaId: '22222222-2222-2222-2222-222222222222',
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
      nome: 'Joao Atualizado',
      email: 'joao.novo@example.com',
      empresaId: '22222222-2222-2222-2222-222222222222',
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
