import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
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

function atLeastOneFilter(control: AbstractControl): ValidationErrors | null {
  const razaoSocial = control.get('razaoSocial')?.value?.trim();
  const dominio = control.get('dominio')?.value?.trim();
  return razaoSocial || dominio ? null : { atLeastOneFilter: true };
}

@Component({
  selector: 'app-empresa-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
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

  private readonly fb = inject(FormBuilder);
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
  readonly hasActiveFilter = signal(false);
  isMobile = false;

  readonly filterForm = this.fb.nonNullable.group(
    {
      razaoSocial: [''],
      dominio: ['', Validators.pattern(/^[a-zA-Z0-9]*$/)],
    },
    { validators: atLeastOneFilter },
  );

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
    this.hasActiveFilter.set(false);
    this.fetchEmpresas(() => this.empresaService.list());
  }

  applyFilter(): void {
    if (this.filterForm.invalid) {
      this.filterForm.markAllAsTouched();
      return;
    }

    const { razaoSocial, dominio } = this.filterForm.getRawValue();
    this.hasActiveFilter.set(true);
    this.fetchEmpresas(() => this.empresaService.filtrar({ razaoSocial, dominio }));
  }

  clearFilter(): void {
    this.filterForm.reset();
    this.loadEmpresas();
  }

  onDominioInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const sanitized = input.value.replace(/[^a-zA-Z0-9]/g, '');
    if (sanitized !== input.value) {
      this.filterForm.controls.dominio.setValue(sanitized);
    }
  }

  private fetchEmpresas(request: () => ReturnType<EmpresaService['list']>): void {
    this.loading.set(true);
    this.errorMessage.set('');

    request()
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
