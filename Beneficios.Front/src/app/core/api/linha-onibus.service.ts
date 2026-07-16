import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LinhaOnibusDto,
  LinhaOnibusFiltroRequest,
  LinhaOnibusSalvarDto,
} from './linha-onibus.models';

@Injectable({ providedIn: 'root' })
export class LinhaOnibusService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/linhas-onibus`;

  filtrar(filtro: LinhaOnibusFiltroRequest = {}): Observable<LinhaOnibusDto[]> {
    let params = new HttpParams();
    if (filtro.descricao) params = params.set('descricao', filtro.descricao);
    if (filtro.somenteVigentes === true) params = params.set('somenteVigentes', 'true');
    if (filtro.somenteVigentes === false) params = params.set('somenteVigentes', 'false');
    if (filtro.referencia) params = params.set('referencia', filtro.referencia);
    return this.http.get<LinhaOnibusDto[]>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<LinhaOnibusDto> {
    return this.http.get<LinhaOnibusDto>(`${this.baseUrl}/${id}`);
  }

  criar(payload: LinhaOnibusSalvarDto): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, payload);
  }

  atualizar(id: string, payload: LinhaOnibusSalvarDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }
}
