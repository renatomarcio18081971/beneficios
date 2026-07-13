import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-recuperar-senha',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './recuperar-senha.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './recuperar-senha.component.scss',
})
export class RecuperarSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    const { email } = this.form.getRawValue();

    this.authService.solicitarAlteracaoSenha(email).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/redefinir-senha'], { state: { email } });
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        if (error.status === 404) {
          this.errorMessage = 'E-mail não localizado !';
          return;
        }

        this.errorMessage =
          error.status === 0
            ? 'Não foi possível conectar à API. Verifique se o backend está em execução.'
            : 'Não foi possível solicitar a alteração de senha.';
      },
    });
  }
}
