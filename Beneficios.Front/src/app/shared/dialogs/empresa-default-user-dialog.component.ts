import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { TENANT_DEFAULT_USER } from '../constants/tenant-default-user';

export const EMPRESA_DEFAULT_USER_DIALOG_WIDTH = '520px';

export interface EmpresaDefaultUserDialogData {
  dominio: string;
  razaoSocial: string;
}

@Component({
  selector: 'app-empresa-default-user-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  styles: `
    .dialog-body {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 32px 24px 16px;
    }

    .dialog-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background-color: var(--beneficios-primary);
      color: #fff;
      margin-bottom: 20px;
    }

    .dialog-icon-mark {
      font-family: Roboto, 'Helvetica Neue', sans-serif;
      font-size: 2rem;
      font-weight: 500;
      line-height: 1;
    }

    h2[mat-dialog-title] {
      margin: 0 0 16px;
      padding: 0;
      text-align: center;
      font-size: 1.25rem;
      font-weight: 500;
    }

    .credentials {
      width: 100%;
      text-align: left;
      margin: 0 0 8px;
      padding: 16px;
      border-radius: 8px;
      background: #f5f5f5;
      font-size: 0.95rem;
      line-height: 1.6;
    }

    .credentials p {
      margin: 0 0 8px;
    }

    .credentials p:last-child {
      margin-bottom: 0;
    }

    .hint {
      margin: 0 0 16px;
      color: rgba(0, 0, 0, 0.6);
      font-size: 0.875rem;
      line-height: 1.5;
    }

    mat-dialog-actions {
      justify-content: center;
      padding: 8px 24px 24px;
    }
  `,
  template: `
    <div class="dialog-body">
      <div class="dialog-icon" aria-hidden="true">
        <span class="dialog-icon-mark">✓</span>
      </div>
      <h2 mat-dialog-title>Empresa cadastrada</h2>
      <p class="hint">
        Um usuário padrão foi criado para acesso da empresa
        <strong>{{ data.razaoSocial }}</strong>.
        Utilize as credenciais abaixo para o primeiro login e cadastro dos demais usuários.
      </p>
      <div class="credentials">
        <p><strong>E-mail:</strong> {{ defaultUser.email }}</p>
        <p><strong>Senha:</strong> {{ defaultUser.senha }}</p>
        <p><strong>URL de acesso:</strong> http://{{ data.dominio }}.localhost:4200</p>
      </div>
    </div>
    <mat-dialog-actions>
      <button mat-flat-button color="primary" mat-dialog-close>Entendi</button>
    </mat-dialog-actions>
  `,
})
export class EmpresaDefaultUserDialogComponent {
  readonly data = inject<EmpresaDefaultUserDialogData>(MAT_DIALOG_DATA);
  readonly defaultUser = TENANT_DEFAULT_USER;
}
