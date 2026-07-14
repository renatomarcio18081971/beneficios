import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { FuncionarioService } from '../../../core/api/funcionario.service';
import {
  FuncionarioSalvarRequest,
  JORNADAS,
  JornadaTrabalho,
  SITUACOES,
  SituacaoFuncionario,
  TIPOS_CONTRATO,
  TipoContrato,
} from '../../../core/api/funcionario.models';
import { PermissaoService } from '../../../core/auth/permissao.service';

@Component({
  selector: 'app-funcionario-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './funcionario-form.component.html',
  styleUrl: './funcionario-form.component.scss',
})
export class FuncionarioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(FuncionarioService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);
  private readonly permissao = inject(PermissaoService);

  readonly jornadas = JORNADAS;
  readonly tipos = TIPOS_CONTRATO;
  readonly situacoes = SITUACOES;
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly id = signal<string | null>(null);
  readonly podeEditar = this.permissao.possuiPermissao('funcionarios', 'editar');
  readonly podeCriar = this.permissao.possuiPermissao('funcionarios', 'criar');

  readonly form = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    cpf: ['', Validators.required],
    matricula: [''],
    dataAdmissao: ['', Validators.required],
    dataDesligamento: [''],
    cargo: ['', Validators.required],
    salarioBase: [0, Validators.required],
    tipoContrato: ['Clt' as TipoContrato, Validators.required],
    centroCusto: [''],
    resCep: [''],
    resLogradouro: [''],
    resNumero: [''],
    resComplemento: [''],
    resBairro: [''],
    resCidade: [''],
    resUf: [''],
    trabNomeLocal: [''],
    trabCep: [''],
    trabLogradouro: [''],
    trabNumero: [''],
    trabComplemento: [''],
    trabBairro: [''],
    trabCidade: [''],
    trabUf: [''],
    situacao: ['Ativo' as SituacaoFuncionario, Validators.required],
    motivoAfastamento: [''],
    jornada: ['QuarentaHorasSegSex' as JornadaTrabalho, Validators.required],
    jornadaDetalhe: [''],
    vtAtivo: [false],
    vtDataInicio: [''],
    vtDataFim: [''],
    vtOptIn: [false],
  });

  get titulo(): string {
    return this.id() ? 'Editar funcionário' : 'Novo funcionário';
  }

  get exigeDetalhe(): boolean {
    return this.form.controls.jornada.value === 'EspecialCategoria';
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.carregar(id);
    }
  }

  onCepInput(control: 'resCep' | 'trabCep', event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 8);
    const masked = digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits;
    this.form.controls[control].setValue(masked, { emitEvent: false });
  }

  onCpfInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let d = input.value.replace(/\D/g, '').slice(0, 11);
    if (d.length > 9) d = `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`;
    else if (d.length > 6) d = `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6)}`;
    else if (d.length > 3) d = `${d.slice(0, 3)}.${d.slice(3)}`;
    this.form.controls.cpf.setValue(d, { emitEvent: false });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    if (v.situacao === 'Desligado' && !v.dataDesligamento) {
      this.snack.open('Informe a data de desligamento.', 'Fechar', { duration: 4000 });
      return;
    }
    if (v.jornada === 'EspecialCategoria' && !v.jornadaDetalhe.trim()) {
      this.snack.open('Informe o detalhe da jornada especial.', 'Fechar', { duration: 4000 });
      return;
    }

    const payload: FuncionarioSalvarRequest = {
      nome: v.nome.trim(),
      cpf: v.cpf,
      matricula: v.matricula.trim() || null,
      dataAdmissao: v.dataAdmissao,
      dataDesligamento: v.dataDesligamento || null,
      cargo: v.cargo.trim(),
      salarioBase: Number(v.salarioBase),
      tipoContrato: v.tipoContrato,
      centroCusto: v.centroCusto.trim() || null,
      resCep: v.resCep || null,
      resLogradouro: v.resLogradouro || null,
      resNumero: v.resNumero || null,
      resComplemento: v.resComplemento || null,
      resBairro: v.resBairro || null,
      resCidade: v.resCidade || null,
      resUf: v.resUf || null,
      trabNomeLocal: v.trabNomeLocal || null,
      trabCep: v.trabCep || null,
      trabLogradouro: v.trabLogradouro || null,
      trabNumero: v.trabNumero || null,
      trabComplemento: v.trabComplemento || null,
      trabBairro: v.trabBairro || null,
      trabCidade: v.trabCidade || null,
      trabUf: v.trabUf || null,
      situacao: v.situacao,
      motivoAfastamento: v.motivoAfastamento || null,
      jornada: v.jornada,
      jornadaDetalhe: v.jornada === 'EspecialCategoria' ? v.jornadaDetalhe.trim() : null,
      beneficios: [
        {
          codigoBeneficio: 'vale_transporte',
          ativo: v.vtAtivo,
          dataInicio: v.vtDataInicio || null,
          dataFim: v.vtDataFim || null,
          optIn: v.vtOptIn,
        },
      ],
    };

    const somenteLeitura = this.id() ? !this.podeEditar : !this.podeCriar;
    if (somenteLeitura) return;

    this.salvando.set(true);
    const onError = (err: { error?: { message?: string } }) => {
      const msg = err?.error?.message ?? 'Erro ao salvar funcionário.';
      this.snack.open(msg, 'Fechar', { duration: 5000 });
    };
    const onOk = () => {
      this.snack.open('Funcionário salvo.', 'Fechar', { duration: 2500 });
      void this.router.navigate(['/funcionarios']);
    };

    if (this.id()) {
      this.service
        .atualizar(this.id()!, payload)
        .pipe(finalize(() => this.salvando.set(false)))
        .subscribe({ next: onOk, error: onError });
    } else {
      this.service
        .criar(payload)
        .pipe(finalize(() => this.salvando.set(false)))
        .subscribe({ next: onOk, error: onError });
    }
  }

  private carregar(id: string): void {
    this.carregando.set(true);
    this.service
      .obterPorId(id)
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (f) => {
          const vt = f.beneficios?.find((b) => b.codigoBeneficio === 'vale_transporte');
          this.form.patchValue({
            nome: f.nome,
            cpf: f.cpf,
            matricula: f.matricula ?? '',
            dataAdmissao: f.dataAdmissao.substring(0, 10),
            dataDesligamento: f.dataDesligamento?.substring(0, 10) ?? '',
            cargo: f.cargo,
            salarioBase: f.salarioBase,
            tipoContrato: f.tipoContrato,
            centroCusto: f.centroCusto ?? '',
            resCep: this.maskCep(f.resCep),
            resLogradouro: f.resLogradouro ?? '',
            resNumero: f.resNumero ?? '',
            resComplemento: f.resComplemento ?? '',
            resBairro: f.resBairro ?? '',
            resCidade: f.resCidade ?? '',
            resUf: f.resUf ?? '',
            trabNomeLocal: f.trabNomeLocal ?? '',
            trabCep: this.maskCep(f.trabCep),
            trabLogradouro: f.trabLogradouro ?? '',
            trabNumero: f.trabNumero ?? '',
            trabComplemento: f.trabComplemento ?? '',
            trabBairro: f.trabBairro ?? '',
            trabCidade: f.trabCidade ?? '',
            trabUf: f.trabUf ?? '',
            situacao: f.situacao,
            motivoAfastamento: f.motivoAfastamento ?? '',
            jornada: f.jornada,
            jornadaDetalhe: f.jornadaDetalhe ?? '',
            vtAtivo: vt?.ativo ?? false,
            vtDataInicio: vt?.dataInicio?.substring(0, 10) ?? '',
            vtDataFim: vt?.dataFim?.substring(0, 10) ?? '',
            vtOptIn: vt?.optIn ?? false,
          });
          if (!this.podeEditar) this.form.disable();
        },
        error: () => this.snack.open('Funcionário não encontrado.', 'Fechar', { duration: 4000 }),
      });
  }

  private maskCep(cep: string | null | undefined): string {
    if (!cep) return '';
    const d = cep.replace(/\D/g, '').slice(0, 8);
    return d.length > 5 ? `${d.slice(0, 5)}-${d.slice(5)}` : d;
  }
}
