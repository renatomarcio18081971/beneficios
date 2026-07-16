import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { CalendarioService } from '../../../core/api/calendario.service';
import { CalendarioDia } from '../../../core/api/calendario.models';
import { PermissaoService } from '../../../core/auth/permissao.service';
import {
  DIA_EDITAR_DIALOG_WIDTH,
  DiaEditarDialogComponent,
  DiaEditarDialogData,
} from '../dia-editar-dialog/dia-editar-dialog.component';

const MESES = [
  'Janeiro',
  'Fevereiro',
  'Março',
  'Abril',
  'Maio',
  'Junho',
  'Julho',
  'Agosto',
  'Setembro',
  'Outubro',
  'Novembro',
  'Dezembro',
] as const;

const DIAS_SEMANA = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'] as const;

export interface CelulaCalendario {
  dia: CalendarioDia | null;
  vazia: boolean;
}

@Component({
  selector: 'app-calendario-page',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './calendario-page.component.html',
  styleUrl: './calendario-page.component.scss',
})
export class CalendarioPageComponent {
  private readonly service = inject(CalendarioService);
  private readonly permissao = inject(PermissaoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly diasSemana = DIAS_SEMANA;
  readonly carregando = signal(false);
  readonly gerando = signal(false);
  readonly dias = signal<CalendarioDia[]>([]);
  readonly ano = signal(new Date().getFullYear());
  readonly mes = signal(new Date().getMonth() + 1);

  readonly podeCriar = computed(() => this.permissao.possuiPermissao('calendario', 'criar'));
  readonly podeEditar = computed(() => this.permissao.possuiPermissao('calendario', 'editar'));

  readonly tituloMes = computed(() => `${MESES[this.mes() - 1]} de ${this.ano()}`);

  readonly celulas = computed((): CelulaCalendario[] => {
    const lista = this.dias();
    if (lista.length === 0) {
      return [];
    }

    const primeiro = lista[0];
    const dataRef = new Date(`${primeiro.data.substring(0, 10)}T12:00:00`);
    const offset = dataRef.getDay();
    const cells: CelulaCalendario[] = Array.from({ length: offset }, () => ({
      dia: null,
      vazia: true,
    }));

    for (const dia of lista) {
      cells.push({ dia, vazia: false });
    }

    return cells;
  });

  constructor() {
    this.carregar();
  }

  mesAnterior(): void {
    if (this.mes() === 1) {
      this.mes.set(12);
      this.ano.update((a) => a - 1);
    } else {
      this.mes.update((m) => m - 1);
    }
    this.carregar();
  }

  mesProximo(): void {
    if (this.mes() === 12) {
      this.mes.set(1);
      this.ano.update((a) => a + 1);
    } else {
      this.mes.update((m) => m + 1);
    }
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service
      .listarPorMes(this.ano(), this.mes())
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (dias) => this.dias.set(dias),
        error: () => {
          this.dias.set([]);
          this.snackBar.open('Erro ao carregar calendário.', 'Fechar', { duration: 4000 });
        },
      });
  }

  gerarAno(): void {
    if (!this.podeCriar() || this.gerando()) {
      return;
    }

    this.gerando.set(true);
    this.service
      .gerarAno(this.ano())
      .pipe(finalize(() => this.gerando.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Ano gerado com sucesso.', 'Fechar', { duration: 3000 });
          this.carregar();
        },
        error: (err) => {
          const msg =
            err?.error?.message ??
            (err?.status === 409 ? 'Este ano já está cadastrado.' : 'Erro ao gerar ano.');
          this.snackBar.open(msg, 'Fechar', { duration: 5000 });
        },
      });
  }

  abrirDia(dia: CalendarioDia): void {
    const ref = this.dialog.open(DiaEditarDialogComponent, {
      width: DIA_EDITAR_DIALOG_WIDTH,
      data: { dia, podeEditar: this.podeEditar() } satisfies DiaEditarDialogData,
    });

    ref.afterClosed().subscribe((payload) => {
      if (!payload || !this.podeEditar()) {
        return;
      }

      this.service.atualizar(dia.id, payload).subscribe({
        next: () => {
          this.snackBar.open('Dia atualizado.', 'Fechar', { duration: 2500 });
          this.carregar();
        },
        error: (err) => {
          const msg = err?.error?.message ?? 'Erro ao atualizar o dia.';
          this.snackBar.open(msg, 'Fechar', { duration: 4000 });
        },
      });
    });
  }

  numeroDia(dia: CalendarioDia): string {
    return dia.data.substring(8, 10);
  }
}
