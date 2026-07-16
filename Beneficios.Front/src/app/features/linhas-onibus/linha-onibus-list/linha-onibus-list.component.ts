import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';
import { LinhaOnibusService } from '../../../core/api/linha-onibus.service';
import { LinhaOnibusDto } from '../../../core/api/linha-onibus.models';
import { PermissaoService } from '../../../core/auth/permissao.service';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ExportColumn } from '../../../shared/utils/export.models';
import { ExportService } from '../../../shared/utils/export.service';

@Component({
  selector: 'app-linha-onibus-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DecimalPipe,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
    MatTooltipModule,
  ],
  templateUrl: './linha-onibus-list.component.html',
  styleUrl: './linha-onibus-list.component.scss',
})
export class LinhaOnibusListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LinhaOnibusService);
  private readonly permissao = inject(PermissaoService);
  private readonly exportService = inject(ExportService);
  private readonly tenantService = inject(TenantService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly carregando = signal(false);
  readonly dataSource = signal<LinhaOnibusDto[]>([]);
  readonly displayedColumns = ['descricao', 'dataInicio', 'dataFim', 'valorTarifa', 'acoes'];
  readonly podeCriar = this.permissao.possuiPermissao('linhas_onibus', 'criar');
  readonly podeEditar = this.permissao.possuiPermissao('linhas_onibus', 'editar');
  isMobile = false;

  readonly filtro = this.fb.nonNullable.group({
    descricao: [''],
    vigencia: ['' as '' | 'vigentes'],
  });

  private readonly exportColumns: ExportColumn[] = [
    { key: 'descricao', label: 'Descrição' },
    { key: 'dataInicio', label: 'Data início' },
    {
      key: 'dataFim',
      label: 'Data fim',
      format: (value) => (value ? String(value) : 'Em aberto'),
    },
    {
      key: 'valorTarifa',
      label: 'Tarifa',
      format: (value) =>
        Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }),
    },
  ];

  constructor() {
    this.breakpointObserver
      .observe([Breakpoints.XSmall])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result) => {
        this.isMobile = result.matches;
      });
  }

  ngOnInit(): void {
    this.buscar();
  }

  buscar(): void {
    this.carregando.set(true);
    const v = this.filtro.getRawValue();
    this.service
      .filtrar({
        descricao: v.descricao.trim() || undefined,
        somenteVigentes: v.vigencia === 'vigentes' ? true : undefined,
      })
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (lista) => this.dataSource.set(lista),
        error: () => this.dataSource.set([]),
      });
  }

  limpar(): void {
    this.filtro.reset({ descricao: '', vigencia: '' });
    this.buscar();
  }

  exportExcel(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('linhas-onibus', subdomain, 'xlsx');
    void this.exportService.exportToExcel(this.dataSource(), this.exportColumns, filename);
  }

  exportPdf(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('linhas-onibus', subdomain, 'pdf');
    void this.exportService.exportToPdf(
      this.dataSource(),
      this.exportColumns,
      filename,
      'Linhas de Ônibus',
    );
  }
}
