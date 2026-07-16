import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FuncionarioLinhaAtualizarRequest,
  FuncionarioLinhaDto,
  FuncionarioLinhaFiltroRequest,
  FuncionarioLinhaSalvarRequest,
} from './funcionario-linha.models';

@Injectable({ providedIn: 'root' })
export class FuncionarioLinhaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/funcionario-linhas`;

  filtrar(filtro: FuncionarioLinhaFiltroRequest = {}): Observable<FuncionarioLinhaDto[]> {
    let params = new HttpParams();
    if (filtro.funcionarioId) params = params.set('funcionarioId', filtro.funcionarioId);
    if (filtro.somenteVigentes === true) params = params.set('somenteVigentes', 'true');
    if (filtro.somenteVigentes === false) params = params.set('somenteVigentes', 'false');
    return this.http.get<FuncionarioLinhaDto[]>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<FuncionarioLinhaDto> {
    return this.http.get<FuncionarioLinhaDto>(`${this.baseUrl}/${id}`);
  }

  criar(payload: FuncionarioLinhaSalvarRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, payload);
  }

  atualizar(id: string, payload: FuncionarioLinhaAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }
}
