import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CalendarioDia, CalendarioDiaAtualizarRequest } from './dias-uteis.models';

@Injectable({ providedIn: 'root' })
export class DiasUteisService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/dias-uteis`;

  listarPorMes(ano: number, mes: number): Observable<CalendarioDia[]> {
    const params = new HttpParams().set('ano', ano).set('mes', mes);
    return this.http.get<CalendarioDia[]>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<CalendarioDia> {
    return this.http.get<CalendarioDia>(`${this.baseUrl}/${id}`);
  }

  atualizar(id: string, body: CalendarioDiaAtualizarRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  gerarAno(ano: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/gerar-ano`, { ano });
  }
}
