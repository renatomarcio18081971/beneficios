import { Component, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';

function senhasIguais(control: AbstractControl): ValidationErrors | null {
  const novaSenha = control.get('novaSenha')?.value;
  const confirmarNovaSenha = control.get('confirmarNovaSenha')?.value;

  if (!novaSenha || !confirmarNovaSenha) {
    return null;
  }

  return novaSenha === confirmarNovaSenha ? null : { senhasDiferentes: true };
}

@Component({
  selector: 'app-redefinir-senha',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  templateUrl: './redefinir-senha.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './redefinir-senha.component.scss',
})
export class RedefinirSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  errorMessage = '';
  readonly hideNovaSenha = signal(true);
  readonly hideConfirmarSenha = signal(true);

  readonly form = this.fb.nonNullable.group(
    {
      codigo: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      novaSenha: ['', [Validators.required, Validators.minLength(6)]],
      confirmarNovaSenha: ['', [Validators.required, Validators.minLength(6)]],
    },
    { validators: senhasIguais },
  );

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    const { codigo, novaSenha, confirmarNovaSenha } = this.form.getRawValue();

    this.authService.alterarSenha({ codigo, novaSenha, confirmarNovaSenha }).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/login']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        if (error.status === 400) {
          this.errorMessage = 'Código inválido.';
        } else {
          this.errorMessage =
            error.status === 0
              ? 'Não foi possível conectar à API. Verifique se o backend está em execução.'
              : 'Não foi possível redefinir a senha.';
        }
      },
    });
  }

  toggleNovaSenha(): void {
    this.hideNovaSenha.update((value) => !value);
  }

  toggleConfirmarSenha(): void {
    this.hideConfirmarSenha.update((value) => !value);
  }
}
