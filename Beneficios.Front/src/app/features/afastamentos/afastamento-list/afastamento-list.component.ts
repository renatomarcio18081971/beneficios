import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { AfastamentoService } from '../../../core/api/afastamento.service';
import {
  Afastamento,
  TIPOS_AFASTAMENTO,
  TipoAfastamento,
  labelTipoAfastamento,
} from '../../../core/api/afastamento.models';
import { FuncionarioService } from '../../../core/api/funcionario.service';
import { Funcionario } from '../../../core/api/funcionario.models';
import { PermissaoService } from '../../../core/auth/permissao.service';

@Component({
  selector: 'app-afastamento-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './afastamento-list.component.html',
  styleUrl: './afastamento-list.component.scss',
})
export class AfastamentoListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AfastamentoService);
  private readonly funcionarioService = inject(FuncionarioService);
  private readonly route = inject(ActivatedRoute);
  private readonly permissao = inject(PermissaoService);

  readonly tipos = TIPOS_AFASTAMENTO;
  readonly labelTipo = labelTipoAfastamento;
  readonly carregando = signal(false);
  readonly dataSource = signal<Afastamento[]>([]);
  readonly funcionarios = signal<Funcionario[]>([]);
  readonly displayedColumns = ['funcionarioNome', 'tipo', 'dataInicio', 'dataFim', 'observacao', 'acoes'];
  readonly podeCriar = this.permissao.possuiPermissao('afastamentos', 'criar');
  readonly podeEditar = this.permissao.possuiPermissao('afastamentos', 'editar');
  readonly podeExcluir = this.permissao.possuiPermissao('afastamentos', 'excluir');

  readonly filtro = this.fb.nonNullable.group({
    funcionarioId: [''],
    tipo: ['' as string],
    dataInicio: [''],
    dataFim: [''],
  });

  ngOnInit(): void {
    this.funcionarioService.filtrar().subscribe({
      next: (lista) => this.funcionarios.set(lista),
      error: () => this.funcionarios.set([]),
    });

    const funcionarioId = this.route.snapshot.queryParamMap.get('funcionarioId');
    if (funcionarioId) {
      this.filtro.patchValue({ funcionarioId });
    }
    this.buscar();
  }

  buscar(): void {
    this.carregando.set(true);
    const v = this.filtro.getRawValue();
    this.service
      .filtrar({
        funcionarioId: v.funcionarioId || undefined,
        tipo: (v.tipo || undefined) as TipoAfastamento | undefined,
        dataInicio: v.dataInicio || undefined,
        dataFim: v.dataFim || undefined,
      })
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (lista) => this.dataSource.set(lista),
        error: () => this.dataSource.set([]),
      });
  }

  limpar(): void {
    this.filtro.reset({ funcionarioId: '', tipo: '', dataInicio: '', dataFim: '' });
    this.buscar();
  }

  excluir(row: Afastamento): void {
    if (!confirm(`Excluir afastamento de ${row.funcionarioNome}?`)) return;
    this.service.excluir(row.id).subscribe({
      next: () => this.buscar(),
      error: (err) => alert(err?.error?.message ?? 'Erro ao excluir.'),
    });
  }

  novoLink(): string[] {
    return ['/afastamentos/novo'];
  }

  novoQueryParams(): Record<string, string> | null {
    const id = this.filtro.controls.funcionarioId.value;
    return id ? { funcionarioId: id } : null;
  }
}
