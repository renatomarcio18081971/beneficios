import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import {
  CalendarioDia,
  CalendarioDiaAtualizarRequest,
  TIPOS_EXCECAO_CALENDARIO,
} from '../../../core/api/calendario.models';

export const DIA_EDITAR_DIALOG_WIDTH = '420px';

export interface DiaEditarDialogData {
  dia: CalendarioDia;
  podeEditar: boolean;
}

@Component({
  selector: 'app-dia-editar-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
  ],
  templateUrl: './dia-editar-dialog.component.html',
  styleUrl: './dia-editar-dialog.component.scss',
})
export class DiaEditarDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<DiaEditarDialogComponent, CalendarioDiaAtualizarRequest | null>);
  readonly data = inject<DiaEditarDialogData>(MAT_DIALOG_DATA);

  readonly tipos = TIPOS_EXCECAO_CALENDARIO;

  readonly form = this.fb.nonNullable.group({
    ehDiaUtil: this.data.dia.ehDiaUtil,
    tipoExcecao: this.data.dia.tipoExcecao as string | null,
    observacao: this.data.dia.observacao ?? '',
  });

  constructor() {
    if (!this.data.podeEditar) {
      this.form.disable();
    }
  }

  dataFormatada(): string {
    const [y, m, d] = this.data.dia.data.substring(0, 10).split('-');
    return `${d}/${m}/${y}`;
  }

  cancelar(): void {
    this.dialogRef.close(null);
  }

  salvar(): void {
    if (!this.data.podeEditar || this.form.invalid) {
      return;
    }

    const raw = this.form.getRawValue();
    this.dialogRef.close({
      ehDiaUtil: raw.ehDiaUtil,
      tipoExcecao: (raw.tipoExcecao || null) as CalendarioDiaAtualizarRequest['tipoExcecao'],
      observacao: raw.observacao.trim() ? raw.observacao.trim() : null,
    });
  }
}
