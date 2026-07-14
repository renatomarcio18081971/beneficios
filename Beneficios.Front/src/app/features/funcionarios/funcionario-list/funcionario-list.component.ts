import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { FuncionarioService } from '../../../core/api/funcionario.service';
import { Funcionario, SITUACOES } from '../../../core/api/funcionario.models';
import { labelTipoAfastamento } from '../../../core/api/afastamento.models';
import { PermissaoService } from '../../../core/auth/permissao.service';

@Component({
  selector: 'app-funcionario-list',
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
  templateUrl: './funcionario-list.component.html',
  styleUrl: './funcionario-list.component.scss',
})
export class FuncionarioListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(FuncionarioService);
  private readonly permissao = inject(PermissaoService);

  readonly situacoes = SITUACOES;
  readonly labelMotivo = labelTipoAfastamento;
  readonly carregando = signal(false);
  readonly dataSource = signal<Funcionario[]>([]);
  readonly displayedColumns = ['nome', 'cpf', 'cargo', 'situacao', 'motivo', 'jornada', 'acoes'];
  readonly podeCriar = this.permissao.possuiPermissao('funcionarios', 'criar');
  readonly podeEditar = this.permissao.possuiPermissao('funcionarios', 'editar');
  readonly podeVerAfastamentos = this.permissao.possuiPermissao('afastamentos', 'visualizar');

  readonly filtro = this.fb.nonNullable.group({
    nome: [''],
    cpf: [''],
    matricula: [''],
    situacao: ['' as string],
  });

  constructor() {
    this.buscar();
  }

  buscar(): void {
    this.carregando.set(true);
    const v = this.filtro.getRawValue();
    this.service
      .filtrar({
        nome: v.nome || undefined,
        cpf: v.cpf || undefined,
        matricula: v.matricula || undefined,
        situacao: (v.situacao || undefined) as Funcionario['situacao'] | undefined,
      })
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (lista) => this.dataSource.set(lista),
        error: () => this.dataSource.set([]),
      });
  }

  limpar(): void {
    this.filtro.reset({ nome: '', cpf: '', matricula: '', situacao: '' });
    this.buscar();
  }
}
