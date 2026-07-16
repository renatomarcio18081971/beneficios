import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Funcionario,
  FuncionarioFiltroRequest,
  FuncionarioSalvarRequest,
} from './funcionario.models';

@Injectable({ providedIn: 'root' })
export class FuncionarioService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/funcionarios`;

  filtrar(filtro: FuncionarioFiltroRequest = {}): Observable<Funcionario[]> {
    let params = new HttpParams();
    if (filtro.nome?.trim()) params = params.set('nome', filtro.nome.trim());
    if (filtro.cpf?.trim()) params = params.set('cpf', filtro.cpf.trim());
    if (filtro.matricula?.trim()) params = params.set('matricula', filtro.matricula.trim());
    if (filtro.situacao) params = params.set('situacao', filtro.situacao);
    return this.http.get<Funcionario[]>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<Funcionario> {
    return this.http.get<Funcionario>(`${this.baseUrl}/${id}`);
  }

  criar(payload: FuncionarioSalvarRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, payload);
  }

  atualizar(id: string, payload: FuncionarioSalvarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }
}
