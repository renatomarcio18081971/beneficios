import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Empresa,
  EmpresaAtualizarRequest,
  EmpresaCreateResponse,
  EmpresaFiltroRequest,
  EmpresaSalvarRequest,
} from './empresa.models';

@Injectable({ providedIn: 'root' })
export class EmpresaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/empresas`;

  list(): Observable<Empresa[]> {
    return this.http.get<Empresa[]>(this.baseUrl);
  }

  filtrar(filtro: EmpresaFiltroRequest): Observable<Empresa[]> {
    let params = new HttpParams();

    if (filtro.razaoSocial?.trim()) {
      params = params.set('razaoSocial', filtro.razaoSocial.trim());
    }

    if (filtro.dominio?.trim()) {
      params = params.set('dominio', filtro.dominio.trim());
    }

    return this.http.get<Empresa[]>(`${this.baseUrl}/filtrar`, { params });
  }

  getById(id: string): Observable<Empresa> {
    return this.http.get<Empresa>(`${this.baseUrl}/${id}`);
  }

  create(payload: EmpresaSalvarRequest): Observable<EmpresaCreateResponse> {
    return this.http.post<EmpresaCreateResponse>(this.baseUrl, payload);
  }

  update(id: string, payload: EmpresaAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
