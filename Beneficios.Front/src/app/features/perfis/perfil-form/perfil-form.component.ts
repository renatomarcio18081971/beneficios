import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { PerfilService } from '../../../core/api/perfil.service';
import { PermissaoMenu } from '../../../core/auth/auth.models';
import { MODULOS_SISTEMA, AcaoPermissao } from '../../../core/auth/modulos-sistema';
import {
  ConfirmSaveDialogComponent,
  ConfirmSaveDialogData,
  CONFIRM_SAVE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-save-dialog.component';

interface MatrizLinha {
  codigo: string;
  nome: string;
  visualizar: boolean;
  criar: boolean;
  editar: boolean;
  excluir: boolean;
  suporta: Record<AcaoPermissao, boolean>;
}

@Component({
  selector: 'app-perfil-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './perfil-form.component.html',
  styleUrl: './perfil-form.component.scss',
})
export class PerfilFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly perfilService = inject(PerfilService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly loadingData = signal(false);
  readonly errorMessage = signal('');
  readonly matriz = signal<MatrizLinha[]>([]);
  isEdit = false;
  perfilId = '';
  ehSistema = false;

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2)]],
  });

  ngOnInit(): void {
    this.matriz.set(this.buildMatrizVazia());

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.perfilId = id;
      this.loadPerfil(id);
    }
  }

  get title(): string {
    return this.isEdit ? 'Editar perfil' : 'Novo perfil';
  }

  toggle(codigo: string, acao: AcaoPermissao, checked: boolean): void {
    this.matriz.update((linhas) =>
      linhas.map((linha) =>
        linha.codigo === codigo && linha.suporta[acao]
          ? { ...linha, [acao]: checked }
          : linha,
      ),
    );
  }

  loadPerfil(id: string): void {
    this.loadingData.set(true);
    this.errorMessage.set('');

    this.perfilService
      .getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingData.set(false)),
      )
      .subscribe({
        next: (perfil) => {
          this.ehSistema = perfil.ehSistema;
          this.form.patchValue({ nome: perfil.nome });
          if (perfil.ehSistema) {
            this.form.controls.nome.disable();
          }
          this.matriz.set(this.mergePermissoes(perfil.permissoes ?? []));
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar o perfil.');
        },
      });
  }

  submit(): void {
    if (this.form.invalid || this.loading() || this.ehSistema) {
      this.form.markAllAsTouched();
      return;
    }

    const dialogRef = this.dialog.open<ConfirmSaveDialogComponent, ConfirmSaveDialogData, boolean>(
      ConfirmSaveDialogComponent,
      {
        width: CONFIRM_SAVE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: { nome: this.form.getRawValue().nome || 'perfil', entityLabel: 'o perfil', isEdit: this.isEdit },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.persist();
    });
  }

  private persist(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    const nome = this.form.getRawValue().nome.trim();
    const permissoes = this.matriz().map((linha) => ({
      codigoMenu: linha.codigo,
      visualizar: linha.visualizar,
      criar: linha.criar,
      editar: linha.editar,
      excluir: linha.excluir,
    }));

    if (this.isEdit) {
      this.perfilService
        .update(this.perfilId, { nome, permissoes })
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          finalize(() => this.loading.set(false)),
        )
        .subscribe({
          next: () => void this.router.navigate(['/perfis']),
          error: () => {
            this.errorMessage.set('Não foi possível salvar o perfil.');
          },
        });
      return;
    }

    this.perfilService
      .create({ nome, permissoes })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: () => void this.router.navigate(['/perfis']),
        error: () => {
          this.errorMessage.set('Não foi possível salvar o perfil.');
        },
      });
  }

  private buildMatrizVazia(): MatrizLinha[] {
    return MODULOS_SISTEMA.map((modulo) => ({
      codigo: modulo.codigo,
      nome: modulo.nomeExibicao,
      visualizar: false,
      criar: false,
      editar: false,
      excluir: false,
      suporta: {
        visualizar: modulo.acoesSuportadas.includes('visualizar'),
        criar: modulo.acoesSuportadas.includes('criar'),
        editar: modulo.acoesSuportadas.includes('editar'),
        excluir: modulo.acoesSuportadas.includes('excluir'),
      },
    }));
  }

  private mergePermissoes(permissoes: PermissaoMenu[]): MatrizLinha[] {
    return this.buildMatrizVazia().map((linha) => {
      const found = permissoes.find((p) => p.codigoMenu.toLowerCase() === linha.codigo);
      return {
        ...linha,
        visualizar: found?.visualizar ?? false,
        criar: found?.criar ?? false,
        editar: found?.editar ?? false,
        excluir: found?.excluir ?? false,
      };
    });
  }
}
