import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

export const CONFIRM_DELETE_DIALOG_WIDTH = '480px';

export interface ConfirmDeleteDialogData {
  nome: string;
  entityLabel?: string;
}

@Component({
  selector: 'app-confirm-delete-dialog',
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
      line-height: 1.4;
    }

    mat-dialog-content {
      padding: 0;
      text-align: center;
      font-size: 1rem;
      line-height: 1.5;
      color: var(--beneficios-text);
    }

    mat-dialog-actions {
      padding: 16px 24px 24px;
      gap: 8px;
    }
  `,
  template: `
    <div class="dialog-body">
      <div class="dialog-icon" aria-hidden="true">
        <span class="dialog-icon-mark">?</span>
      </div>

      <h2 mat-dialog-title>Confirma a operação</h2>

      <mat-dialog-content>
        Deseja excluir {{ entityLabel }} <strong>{{ data.nome }}</strong>?
      </mat-dialog-content>
    </div>

    <mat-dialog-actions align="center">
      <button type="button" mat-button mat-dialog-close>Cancelar</button>
      <button type="button" mat-flat-button color="primary" [mat-dialog-close]="true">Excluir</button>
    </mat-dialog-actions>
  `,
})
export class ConfirmDeleteDialogComponent {
  readonly data = inject<ConfirmDeleteDialogData>(MAT_DIALOG_DATA);

  get entityLabel(): string {
    return this.data.entityLabel ?? 'o usuário';
  }
}
