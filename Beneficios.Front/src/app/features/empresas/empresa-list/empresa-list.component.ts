import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTable, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';
import { EmpresaService } from '../../../core/api/empresa.service';
import { Empresa } from '../../../core/api/empresa.models';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ExportColumn } from '../../../shared/utils/export.models';
import { ExportService } from '../../../shared/utils/export.service';
import {
  ConfirmDeleteDialogComponent,
  ConfirmDeleteDialogData,
  CONFIRM_DELETE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-delete-dialog.component';
import {
  DATA_INCLUSAO_DATE_PIPE_FORMAT,
  formatDataInclusao,
} from '../../../shared/utils/date-format';

@Component({
  selector: 'app-empresa-list',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './empresa-list.component.html',
  styleUrl: './empresa-list.component.scss',
})
export class EmpresaListComponent {
  @ViewChild(MatPaginator) set matPaginator(paginator: MatPaginator | undefined) {
    if (paginator) {
      this.dataSource.paginator = paginator;
    }
  }

  @ViewChild(MatTable) set matTable(table: MatTable<Empresa> | undefined) {
    if (table && this.dataSource.data.length > 0) {
      queueMicrotask(() => table.renderRows());
    }
  }

  private readonly empresaService = inject(EmpresaService);
  private readonly exportService = inject(ExportService);
  private readonly tenantService = inject(TenantService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly displayedColumns = ['razaoSocial', 'dominio', 'dataInclusao', 'acoes'];
  readonly dataInclusaoFormat = DATA_INCLUSAO_DATE_PIPE_FORMAT;
  readonly dataSource = new MatTableDataSource<Empresa>([]);

  readonly loading = signal(true);
  readonly errorMessage = signal('');
  isMobile = false;

  private readonly exportColumns: ExportColumn[] = [
    { key: 'razaoSocial', label: 'Razão Social' },
    { key: 'dominio', label: 'Domínio' },
    {
      key: 'dataInclusao',
      label: 'Data inclusão',
      format: (value) => formatDataInclusao(value),
    },
  ];

  constructor() {
    this.breakpointObserver
      .observe([Breakpoints.XSmall])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result) => {
        this.isMobile = result.matches;
      });

    this.loadEmpresas();
  }

  loadEmpresas(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.empresaService
      .list()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (empresas) => {
          this.dataSource.data = empresas;
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar as empresas.');
        },
      });
  }

  edit(empresa: Empresa): void {
    void this.router.navigate(['/empresas', empresa.id, 'editar']);
  }

  exportExcel(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('empresas', subdomain, 'xlsx');
    void this.exportService.exportToExcel(this.dataSource.data, this.exportColumns, filename);
  }

  exportPdf(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('empresas', subdomain, 'pdf');
    void this.exportService.exportToPdf(this.dataSource.data, this.exportColumns, filename, 'Empresas');
  }

  confirmDelete(empresa: Empresa): void {
    const dialogRef = this.dialog.open<ConfirmDeleteDialogComponent, ConfirmDeleteDialogData, boolean>(
      ConfirmDeleteDialogComponent,
      {
        width: CONFIRM_DELETE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: { nome: empresa.razaoSocial, entityLabel: 'a empresa' },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.empresaService.delete(empresa.id).subscribe({
        next: () => this.loadEmpresas(),
        error: () => {
          this.errorMessage.set('Não foi possível excluir a empresa.');
        },
      });
    });
  }

}
