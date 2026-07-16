import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
import { PermissaoService } from '../../../core/auth/permissao.service';
import { CurrencyBrlInputDirective } from '../../../shared/utils/currency-brl-input.directive';

@Component({
  selector: 'app-linha-onibus-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    CurrencyBrlInputDirective,
  ],
  templateUrl: './linha-onibus-form.component.html',
  styleUrl: './linha-onibus-form.component.scss',
})
export class LinhaOnibusFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LinhaOnibusService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  private readonly permissao = inject(PermissaoService);

  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly id = signal<string | null>(null);
  readonly podeEditar = this.permissao.possuiPermissao('linhas_onibus', 'editar');
  readonly podeCriar = this.permissao.possuiPermissao('linhas_onibus', 'criar');

  readonly form = this.fb.nonNullable.group({
    descricao: ['', Validators.required],
    dataInicio: ['', Validators.required],
    dataFim: [''],
    valorTarifa: [0 as number, [Validators.required, Validators.min(0)]],
  });

  get titulo(): string {
    return this.id() ? 'Editar linha de ônibus' : 'Nova linha de ônibus';
  }

  get podeSalvar(): boolean {
    return this.id() ? this.podeEditar : this.podeCriar;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      if (!this.podeCriar && !this.podeEditar) this.form.disable();
      return;
    }

    this.id.set(id);
    this.carregando.set(true);
    this.service
      .obterPorId(id)
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (linha) => {
          this.form.patchValue({
            descricao: linha.descricao,
            dataInicio: linha.dataInicio?.substring(0, 10) ?? '',
            dataFim: linha.dataFim?.substring(0, 10) ?? '',
            valorTarifa: linha.valorTarifa,
          });
          if (!this.podeEditar) this.form.disable();
        },
        error: () => {
          this.snack.open('Linha de ônibus não encontrada.', 'Fechar', { duration: 4000 });
          void this.router.navigate(['/linhas-onibus']);
        },
      });
  }

  salvar(): void {
    if (this.form.invalid || !this.podeSalvar) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.salvando.set(true);
    const done = () => this.salvando.set(false);
    const payload = {
      descricao: v.descricao.trim(),
      dataInicio: v.dataInicio,
      dataFim: v.dataFim || null,
      valorTarifa: Number(v.valorTarifa),
    };
    const onOk = () => {
      this.snack.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
      void this.router.navigate(['/linhas-onibus']);
    };
    const onErr = (err: { error?: { message?: string } }) =>
      this.snack.open(err?.error?.message ?? 'Erro ao salvar.', 'Fechar', { duration: 5000 });

    if (this.id()) {
      this.service
        .atualizar(this.id()!, payload)
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    } else {
      this.service
        .criar(payload)
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    }
  }
}
