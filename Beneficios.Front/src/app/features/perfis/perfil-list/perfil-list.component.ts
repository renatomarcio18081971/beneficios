import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
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
import { PerfilService } from '../../../core/api/perfil.service';
import { Perfil } from '../../../core/api/perfil.models';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ExportColumn } from '../../../shared/utils/export.models';
import { ExportService } from '../../../shared/utils/export.service';
import {
  ConfirmDeleteDialogComponent,
  ConfirmDeleteDialogData,
  CONFIRM_DELETE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-delete-dialog.component';
import { PermissionService } from '../../../core/auth/permission.service';
import {
  DATA_INCLUSAO_DATE_PIPE_FORMAT,
  formatDataInclusao,
} from '../../../shared/utils/date-format';

function atLeastOneFilter(control: AbstractControl): ValidationErrors | null {
  const nome = control.get('nome')?.value?.trim();
  return nome ? null : { atLeastOneFilter: true };
}

@Component({
  selector: 'app-perfil-list',
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
  templateUrl: './perfil-list.component.html',
  styleUrl: './perfil-list.component.scss',
})
export class PerfilListComponent {
  @ViewChild(MatPaginator) set matPaginator(paginator: MatPaginator | undefined) {
    if (paginator) {
      this.dataSource.paginator = paginator;
    }
  }

  @ViewChild(MatTable) set matTable(table: MatTable<Perfil> | undefined) {
    if (table && this.dataSource.data.length > 0) {
      queueMicrotask(() => table.renderRows());
    }
  }

  private readonly fb = inject(FormBuilder);
  private readonly perfilService = inject(PerfilService);
  private readonly exportService = inject(ExportService);
  private readonly tenantService = inject(TenantService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);
  private readonly permission = inject(PermissionService);

  get canCriar(): boolean {
    return this.permission.can('perfis', 'criar');
  }

  get canEditar(): boolean {
    return this.permission.can('perfis', 'editar');
  }

  get canExcluir(): boolean {
    return this.permission.can('perfis', 'excluir');
  }

  get showAcoes(): boolean {
    return this.canEditar || this.canExcluir;
  }

  get displayedColumns(): string[] {
    return this.showAcoes
      ? ['nome', 'ehSistema', 'dataInclusao', 'acoes']
      : ['nome', 'ehSistema', 'dataInclusao'];
  }

  readonly dataInclusaoFormat = DATA_INCLUSAO_DATE_PIPE_FORMAT;
  readonly dataSource = new MatTableDataSource<Perfil>([]);

  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly hasActiveFilter = signal(false);
  isMobile = false;

  readonly filterForm = this.fb.nonNullable.group(
    { nome: [''] },
    { validators: atLeastOneFilter },
  );

  private readonly exportColumns: ExportColumn[] = [
    { key: 'nome', label: 'Nome' },
    {
      key: 'ehSistema',
      label: 'Sistema',
      format: (value) => (value ? 'Sim' : 'Não'),
    },
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

    this.loadPerfis();
  }

  loadPerfis(): void {
    this.hasActiveFilter.set(false);
    this.fetchPerfis(() => this.perfilService.list());
  }

  applyFilter(): void {
    if (this.filterForm.invalid) {
      this.filterForm.markAllAsTouched();
      return;
    }

    const { nome } = this.filterForm.getRawValue();
    this.hasActiveFilter.set(true);
    this.fetchPerfis(() => this.perfilService.filtrar({ nome }));
  }

  clearFilter(): void {
    this.filterForm.reset();
    this.loadPerfis();
  }

  private fetchPerfis(request: () => ReturnType<PerfilService['list']>): void {
    this.loading.set(true);
    this.errorMessage.set('');

    request()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (perfis) => {
          this.dataSource.data = perfis;
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar os perfis.');
        },
      });
  }

  edit(perfil: Perfil): void {
    void this.router.navigate(['/perfis', perfil.id, 'editar']);
  }

  exportExcel(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('perfis', subdomain, 'xlsx');
    void this.exportService.exportToExcel(this.dataSource.data, this.exportColumns, filename);
  }

  exportPdf(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('perfis', subdomain, 'pdf');
    void this.exportService.exportToPdf(this.dataSource.data, this.exportColumns, filename, 'Perfis');
  }

  confirmDelete(perfil: Perfil): void {
    if (perfil.ehSistema) {
      return;
    }

    const dialogRef = this.dialog.open<ConfirmDeleteDialogComponent, ConfirmDeleteDialogData, boolean>(
      ConfirmDeleteDialogComponent,
      {
        width: CONFIRM_DELETE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: { nome: perfil.nome },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.perfilService.delete(perfil.id).subscribe({
        next: () => this.loadPerfis(),
        error: () => {
          this.errorMessage.set('Não foi possível excluir o perfil.');
        },
      });
    });
  }
}
