import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Perfil,
  PerfilAtualizarRequest,
  PerfilCreateResponse,
  PerfilFiltroRequest,
  PerfilSalvarRequest,
} from './perfil.models';

@Injectable({ providedIn: 'root' })
export class PerfilService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/perfis`;

  list(): Observable<Perfil[]> {
    return this.http.get<Perfil[]>(this.baseUrl);
  }

  filtrar(filtro: PerfilFiltroRequest): Observable<Perfil[]> {
    let params = new HttpParams();
    if (filtro.nome?.trim()) {
      params = params.set('nome', filtro.nome.trim());
    }
    return this.http.get<Perfil[]>(`${this.baseUrl}/filtrar`, { params });
  }

  getById(id: string): Observable<Perfil> {
    return this.http.get<Perfil>(`${this.baseUrl}/${id}`);
  }

  create(payload: PerfilSalvarRequest): Observable<PerfilCreateResponse> {
    return this.http.post<PerfilCreateResponse>(this.baseUrl, payload);
  }

  update(id: string, payload: PerfilAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
