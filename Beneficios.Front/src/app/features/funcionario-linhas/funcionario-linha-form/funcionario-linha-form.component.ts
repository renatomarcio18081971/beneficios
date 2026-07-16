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
import { FuncionarioLinhaService } from '../../../core/api/funcionario-linha.service';
import { FuncionarioService } from '../../../core/api/funcionario.service';
import { Funcionario } from '../../../core/api/funcionario.models';
import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
import { LinhaOnibusDto } from '../../../core/api/linha-onibus.models';
import { PermissaoService } from '../../../core/auth/permissao.service';

const MSG_VT_INATIVO =
  'Funcionário sem vale transporte ativo; não é possível vincular linhas.';

@Component({
  selector: 'app-funcionario-linha-form',
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
  templateUrl: './funcionario-linha-form.component.html',
  styleUrl: './funcionario-linha-form.component.scss',
})
export class FuncionarioLinhaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(FuncionarioLinhaService);
  private readonly funcionarioService = inject(FuncionarioService);
  private readonly linhaService = inject(LinhaOnibusService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  private readonly permissao = inject(PermissaoService);

  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly verificandoVt = signal(false);
  readonly mensagemVtInativo = signal<string | null>(null);
  readonly id = signal<string | null>(null);
  readonly funcionarios = signal<Funcionario[]>([]);
  readonly linhas = signal<LinhaOnibusDto[]>([]);
  readonly podeEditar = this.permissao.possuiPermissao('funcionario_linhas', 'editar');
  readonly podeCriar = this.permissao.possuiPermissao('funcionario_linhas', 'criar');

  readonly form = this.fb.nonNullable.group({
    funcionarioId: ['', Validators.required],
    linhaOnibusId: ['', Validators.required],
    quantidade: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
    dataInicio: ['', Validators.required],
    dataFim: [''],
  });

  get titulo(): string {
    return this.id() ? 'Editar vínculo' : 'Novo vínculo';
  }

  get podeSalvar(): boolean {
    return this.id() ? this.podeEditar : this.podeCriar;
  }

  get submitDesabilitado(): boolean {
    return this.salvando() || this.verificandoVt() || !!this.mensagemVtInativo();
  }

  ngOnInit(): void {
    this.funcionarioService.filtrar().subscribe({
      next: (lista) => this.funcionarios.set(lista),
      error: () => this.funcionarios.set([]),
    });

    this.form.controls.funcionarioId.valueChanges.subscribe((funcionarioId) => {
      this.verificarVtAtivo(funcionarioId);
    });

    const id = this.route.snapshot.paramMap.get('id');
    const funcionarioId = this.route.snapshot.queryParamMap.get('funcionarioId');
    if (funcionarioId && !id) {
      this.form.patchValue({ funcionarioId });
    }

    if (!id) {
      this.carregarLinhasVigentes();
      if (!this.podeCriar && !this.podeEditar) this.form.disable();
      return;
    }

    this.id.set(id);
    this.carregando.set(true);
    this.service
      .obterPorId(id)
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (v) => {
          this.carregarLinhasVigentes({ id: v.linhaOnibusId, descricao: v.linhaDescricao });
          this.form.patchValue({
            funcionarioId: v.funcionarioId,
            linhaOnibusId: v.linhaOnibusId,
            quantidade: v.quantidade,
            dataInicio: v.dataInicio?.substring(0, 10) ?? '',
            dataFim: v.dataFim?.substring(0, 10) ?? '',
          });
          if (!this.podeEditar) this.form.disable();
        },
        error: () => {
          this.snack.open('Vínculo não encontrado.', 'Fechar', { duration: 4000 });
          void this.router.navigate(['/funcionario-linhas']);
        },
      });
  }

  salvar(): void {
    if (this.form.invalid || !this.podeSalvar || this.submitDesabilitado) {
      this.form.markAllAsTouched();
      if (this.mensagemVtInativo()) {
        this.snack.open(this.mensagemVtInativo()!, 'Fechar', { duration: 5000 });
      }
      return;
    }
    const v = this.form.getRawValue();
    this.salvando.set(true);
    const done = () => this.salvando.set(false);
    const onOk = () => {
      this.snack.open('Salvo com sucesso.', 'Fechar', { duration: 3000 });
      void this.router.navigate(['/funcionario-linhas'], {
        queryParams: v.funcionarioId ? { funcionarioId: v.funcionarioId } : undefined,
      });
    };
    const onErr = (err: { error?: { message?: string } }) =>
      this.snack.open(err?.error?.message ?? 'Erro ao salvar.', 'Fechar', { duration: 5000 });

    if (this.id()) {
      this.service
        .atualizar(this.id()!, {
          linhaOnibusId: v.linhaOnibusId,
          quantidade: Number(v.quantidade),
          dataInicio: v.dataInicio,
          dataFim: v.dataFim || null,
        })
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    } else {
      this.service
        .criar({
          funcionarioId: v.funcionarioId,
          linhaOnibusId: v.linhaOnibusId,
          quantidade: Number(v.quantidade),
          dataInicio: v.dataInicio,
          dataFim: v.dataFim || null,
        })
        .pipe(finalize(done))
        .subscribe({ next: onOk, error: onErr });
    }
  }

  private verificarVtAtivo(funcionarioId: string | null | undefined): void {
    if (!funcionarioId) {
      this.mensagemVtInativo.set(null);
      this.verificandoVt.set(false);
      return;
    }

    this.verificandoVt.set(true);
    this.funcionarioService
      .obterPorId(funcionarioId)
      .pipe(finalize(() => this.verificandoVt.set(false)))
      .subscribe({
        next: (f) => {
          const vtAtivo = f.beneficios?.some(
            (b) => b.codigoBeneficio === 'vale_transporte' && b.ativo,
          );
          this.mensagemVtInativo.set(vtAtivo ? null : MSG_VT_INATIVO);
        },
        error: () => {
          this.mensagemVtInativo.set(MSG_VT_INATIVO);
        },
      });
  }

  private carregarLinhasVigentes(incluir?: { id: string; descricao: string }): void {
    this.linhaService.filtrar({ somenteVigentes: true }).subscribe({
      next: (lista) => {
        if (incluir && !lista.some((l) => l.id === incluir.id)) {
          lista = [
            ...lista,
            {
              id: incluir.id,
              descricao: incluir.descricao,
              dataInicio: '',
              dataFim: null,
              valorTarifa: 0,
            },
          ];
        }
        this.linhas.set(lista);
      },
      error: () =>
        this.linhas.set(
          incluir
            ? [
                {
                  id: incluir.id,
                  descricao: incluir.descricao,
                  dataInicio: '',
                  dataFim: null,
                  valorTarifa: 0,
                },
              ]
            : [],
        ),
    });
  }
}
