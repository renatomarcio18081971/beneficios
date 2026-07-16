import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Afastamento,
  AfastamentoAtualizarRequest,
  AfastamentoFiltroRequest,
  AfastamentoSalvarRequest,
} from './afastamento.models';

@Injectable({ providedIn: 'root' })
export class AfastamentoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/afastamentos`;

  filtrar(filtro: AfastamentoFiltroRequest = {}): Observable<Afastamento[]> {
    let params = new HttpParams();
    if (filtro.funcionarioId) params = params.set('funcionarioId', filtro.funcionarioId);
    if (filtro.tipo) params = params.set('tipo', filtro.tipo);
    if (filtro.dataInicio) params = params.set('dataInicio', filtro.dataInicio);
    if (filtro.dataFim) params = params.set('dataFim', filtro.dataFim);
    return this.http.get<Afastamento[]>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<Afastamento> {
    return this.http.get<Afastamento>(`${this.baseUrl}/${id}`);
  }

  criar(payload: AfastamentoSalvarRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, payload);
  }

  atualizar(id: string, payload: AfastamentoAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
