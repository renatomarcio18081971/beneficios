import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/auth/auth.service';
import { UsuarioService } from '../../../core/api/usuario.service';

@Component({
  selector: 'app-usuario-form',
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
  templateUrl: './usuario-form.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './usuario-form.component.scss',
})
export class UsuarioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usuarioService = inject(UsuarioService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  loading = false;
  loadingData = false;
  errorMessage = '';
  isEdit = false;
  usuarioId = '';

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    senha: [''],
  });

  ngOnInit(): void {
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

  get title(): string {
    return this.isEdit ? 'Editar usuário' : 'Novo usuário';
  }

  loadUsuario(id: string): void {
    this.loadingData = true;
    this.usuarioService.getById(id).subscribe({
      next: (usuario) => {
        this.form.patchValue({
          nome: usuario.nome,
          email: usuario.email,
        });
        this.loadingData = false;
      },
      error: () => {
        this.loadingData = false;
        this.errorMessage = 'Não foi possível carregar o usuário.';
      },
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    const empresaId = this.authService.currentUser()?.empresaId;
    if (!empresaId) {
      this.errorMessage = 'Empresa não identificada na sessão.';
      return;
    }

    const { nome, email, senha } = this.form.getRawValue();
    this.loading = true;
    this.errorMessage = '';

    if (this.isEdit) {
      this.usuarioService.update(this.usuarioId, { nome, email, empresaId }).subscribe({
        next: () => void this.router.navigate(['/usuarios']),
        error: () => {
          this.loading = false;
          this.errorMessage = 'Não foi possível atualizar o usuário.';
        },
      });
      return;
    }

    this.usuarioService.create({ nome, email, senha, empresaId }).subscribe({
      next: () => void this.router.navigate(['/usuarios']),
      error: () => {
        this.loading = false;
        this.errorMessage = 'Não foi possível criar o usuário.';
      },
    });
  }
}
