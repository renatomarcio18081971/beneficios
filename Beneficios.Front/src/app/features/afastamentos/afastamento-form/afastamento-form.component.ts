import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { AfastamentoService } from '../../../core/api/afastamento.service';
import { TIPOS_AFASTAMENTO, TipoAfastamento } from '../../../core/api/afastamento.models';
import { FuncionarioService } from '../../../core/api/funcionario.service';
import { Funcionario } from '../../../core/api/funcionario.models';
import { PermissaoService } from '../../../core/auth/permissao.service';

@Component({
  selector: 'app-afastamento-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './afastamento-form.component.html',
  styleUrl: './afastamento-form.component.scss',
})
export class AfastamentoFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AfastamentoService);
  private readonly funcionarioService = inject(FuncionarioService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  private readonly permissao = inject(PermissaoService);

  readonly tipos = TIPOS_AFASTAMENTO;
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly id = signal<string | null>(null);
  readonly funcionarios = signal<Funcionario[]>([]);
  readonly podeEditar = this.permissao.possuiPermissao('afastamentos', 'editar');
  readonly podeCriar = this.permissao.possuiPermissao('afastamentos', 'criar');
  readonly podeExcluir = this.permissao.possuiPermissao('afastamentos', 'excluir');

  readonly form = this.fb.nonNullable.group({
    funcionarioId: ['', Validators.required],
    tipo: ['Ferias' as TipoAfastamento, Validators.required],
    dataInicio: ['', Validators.required],
    dataFim: [''],
    observacao: [''],
  });

  get titulo(): string {
    return this.id() ? 'Editar afastamento' : 'Novo afastamento';
  }

  get podeSalvar(): boolean {
    return this.id() ? this.podeEditar : this.podeCriar;
  }

  ngOnInit(): void {
    this.funcionarioService.filtrar().subscribe({
      next: (lista) => this.funcionarios.set(lista),
      error: () => this.funcionarios.set([]),
    });

    const id = this.route.snapshot.paramMap.get('id');
    const funcionarioId = this.route.snapshot.queryParamMap.get('funcionarioId');
    if (funcionarioId && !id) {
      this.form.patchValue({ funcionarioId });
    }

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
        next: (a) => {
          this.form.patchValue({
            funcionarioId: a.funcionarioId,
            tipo: a.tipo,
            dataInicio: a.dataInicio?.substring(0, 10) ?? '',
            dataFim: a.dataFim?.substring(0, 10) ?? '',
            observacao: a.observacao ?? '',
          });
          if (!this.podeEditar) this.form.disable();
        },
        error: () => {
          this.snack.open('Afastamento não encontrado.', 'Fechar', { duration: 4000 });
          void this.router.navigate(['/afastamentos']);
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
    const onOk = () => {
      this.snack.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
      void this.router.navigate(['/afastamentos'], {
        queryParams: v.funcionarioId ? { funcionarioId: v.funcionarioId } : undefined,
      });
    };
    const onErr = (err: { error?: { message?: string } }) =>
      this.snack.open(err?.error?.message ?? 'Erro ao salvar.', 'Fechar', { duration: 5000 });

    if (this.id()) {
      this.service
        .atualizar(this.id()!, {
          tipo: v.tipo,
          dataInicio: v.dataInicio,
          dataFim: v.dataFim || null,
          observacao: v.observacao || null,
        })
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    } else {
      this.service
        .criar({
          funcionarioId: v.funcionarioId,
          tipo: v.tipo,
          dataInicio: v.dataInicio,
          dataFim: v.dataFim || null,
          observacao: v.observacao || null,
        })
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    }
  }

  excluir(): void {
    const id = this.id();
    if (!id || !this.podeExcluir) return;
    if (!confirm('Excluir este afastamento?')) return;
    this.service.excluir(id).subscribe({
      next: () => {
        this.snack.open('Excluído.', 'Fechar', { duration: 3000 });
        void this.router.navigate(['/afastamentos']);
      },
      error: (err) =>
        this.snack.open(err?.error?.message ?? 'Erro ao excluir.', 'Fechar', { duration: 5000 }),
    });
  }
}
