import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { UsuarioService } from '../../../core/api/usuario.service';
import { PerfilService } from '../../../core/api/perfil.service';
import { Perfil } from '../../../core/api/perfil.models';
import { MatSelectModule } from '@angular/material/select';
import {
  ConfirmSaveDialogComponent,
  ConfirmSaveDialogData,
  CONFIRM_SAVE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-save-dialog.component';
import { isTenantDefaultUser } from '../../../shared/constants/tenant-default-user';

@Component({
  selector: 'app-usuario-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './usuario-form.component.html',
  styleUrl: './usuario-form.component.scss',
})
export class UsuarioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usuarioService = inject(UsuarioService);
  private readonly perfilService = inject(PerfilService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly loadingData = signal(false);
  readonly errorMessage = signal('');
  readonly senhaVisible = signal(false);
  readonly perfis = signal<Perfil[]>([]);
  isEdit = false;
  usuarioId = '';

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    senha: [''],
    perfilId: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loadPerfis();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.usuarioId = id;
      this.form.controls.senha.clearValidators();
      this.loadUsuario(id);
      return;
    }

    this.form.controls.senha.setValidators([Validators.required, Validators.minLength(6)]);
    this.form.controls.senha.updateValueAndValidity();
  }

  private loadPerfis(): void {
    this.perfilService
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (perfis) => this.perfis.set(perfis),
        error: () => this.errorMessage.set('Não foi possível carregar os perfis.'),
      });
  }

  get title(): string {
    return this.isEdit ? 'Editar usuário' : 'Novo usuário';
  }

  toggleSenhaVisible(): void {
    this.senhaVisible.update((visible) => !visible);
  }

  loadUsuario(id: string): void {
    this.loadingData.set(true);
    this.errorMessage.set('');

    this.usuarioService
      .getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingData.set(false)),
      )
      .subscribe({
        next: (usuario) => {
          if (isTenantDefaultUser(usuario.email)) {
            this.errorMessage.set('O usuário padrão da empresa não pode ser editado.');
            void this.router.navigate(['/usuarios']);
            return;
          }

          this.form.patchValue({
            nome: usuario.nome,
            email: usuario.email,
            perfilId: usuario.perfilId ?? '',
          });
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar o usuário.');
        },
      });
  }

  submit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const dialogRef = this.dialog.open<ConfirmSaveDialogComponent, ConfirmSaveDialogData, boolean>(
      ConfirmSaveDialogComponent,
      {
        width: CONFIRM_SAVE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: {
          nome: this.form.controls.nome.value,
          entityLabel: 'o usuário',
          isEdit: this.isEdit,
        },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.persist();
      }
    });
  }

  private persist(): void {
    const empresaId = this.authService.currentUser()?.empresaId;
    if (!empresaId) {
      this.errorMessage.set('Empresa não identificada na sessão.');
      return;
    }

    const { nome, email, senha, perfilId } = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set('');

    if (this.isEdit) {
      this.usuarioService.update(this.usuarioId, { nome, email, empresaId, perfilId }).subscribe({
        next: () => void this.router.navigate(['/usuarios']),
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Não foi possível atualizar o usuário.');
        },
      });
      return;
    }

    this.usuarioService.create({ nome, email, senha, empresaId, perfilId }).subscribe({
      next: () => void this.router.navigate(['/usuarios']),
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Não foi possível criar o usuário.');
      },
    });
  }
}
