import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

const LOGIN_PATH = '/usuarios/login';

export const httpErrorMessages: Record<number, string> = {
  401: 'Sessão expirada. Faça login novamente.',
  403: 'Você não tem permissão para esta ação.',
  404: 'Registro não encontrado.',
  500: 'Erro interno. Tente novamente mais tarde.',
};

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (!shouldHandle(error, req.url)) {
        return throwError(() => error);
      }

      const message = httpErrorMessages[error.status];
      if (message) {
        snackBar.open(message, 'Fechar', { duration: 5000 });
      }

      if (error.status === 401) {
        void router.navigate(['/login']);
      }

      return throwError(() => error);
    }),
  );
};

export function shouldHandle(error: HttpErrorResponse, requestUrl: string): boolean {
  if (!(error.status in httpErrorMessages)) {
    return false;
  }

  return !requestUrl.includes(LOGIN_PATH);
}
