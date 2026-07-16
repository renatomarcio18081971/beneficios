import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Usuario,
  UsuarioAtualizarRequest,
  UsuarioCreateResponse,
  UsuarioFiltroRequest,
  UsuarioSalvarRequest,
} from './usuario.models';

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/usuarios`;

  list(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.baseUrl);
  }

  filtrar(filtro: UsuarioFiltroRequest): Observable<Usuario[]> {
    let params = new HttpParams();

    if (filtro.nome?.trim()) {
      params = params.set('nome', filtro.nome.trim());
    }

    if (filtro.email?.trim()) {
      params = params.set('email', filtro.email.trim());
    }

    return this.http.get<Usuario[]>(`${this.baseUrl}/filtrar`, { params });
  }

  getById(id: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/${id}`);
  }

  create(payload: UsuarioSalvarRequest): Observable<UsuarioCreateResponse> {
    return this.http.post<UsuarioCreateResponse>(this.baseUrl, payload);
  }

  update(id: string, payload: UsuarioAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
