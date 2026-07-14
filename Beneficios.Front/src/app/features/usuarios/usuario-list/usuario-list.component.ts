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
import { UsuarioService } from '../../../core/api/usuario.service';
import { Usuario } from '../../../core/api/usuario.models';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ExportColumn } from '../../../shared/utils/export.models';
import { ExportService } from '../../../shared/utils/export.service';
import {
  ConfirmDeleteDialogComponent,
  ConfirmDeleteDialogData,
  CONFIRM_DELETE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-delete-dialog.component';
import { isTenantDefaultUser } from '../../../shared/constants/tenant-default-user';
import {
  DATA_INCLUSAO_DATE_PIPE_FORMAT,
  formatDataInclusao,
} from '../../../shared/utils/date-format';

function atLeastOneFilter(control: AbstractControl): ValidationErrors | null {
  const nome = control.get('nome')?.value?.trim();
  const email = control.get('email')?.value?.trim();
  return nome || email ? null : { atLeastOneFilter: true };
}

@Component({
  selector: 'app-usuario-list',
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
  templateUrl: './usuario-list.component.html',
  styleUrl: './usuario-list.component.scss',
})
export class UsuarioListComponent {
  @ViewChild(MatPaginator) set matPaginator(paginator: MatPaginator | undefined) {
    if (paginator) {
      this.dataSource.paginator = paginator;
    }
  }

  @ViewChild(MatTable) set matTable(table: MatTable<Usuario> | undefined) {
    if (table && this.dataSource.data.length > 0) {
      queueMicrotask(() => table.renderRows());
    }
  }

  private readonly fb = inject(FormBuilder);
  private readonly usuarioService = inject(UsuarioService);
  private readonly exportService = inject(ExportService);
  private readonly tenantService = inject(TenantService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly displayedColumns = ['nome', 'email', 'dataInclusao', 'acoes'];
  readonly dataInclusaoFormat = DATA_INCLUSAO_DATE_PIPE_FORMAT;
  readonly dataSource = new MatTableDataSource<Usuario>([]);

  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly hasActiveFilter = signal(false);
  isMobile = false;

  readonly filterForm = this.fb.nonNullable.group(
    {
      nome: [''],
      email: ['', Validators.email],
    },
    { validators: atLeastOneFilter },
  );

  private readonly exportColumns: ExportColumn[] = [
    { key: 'nome', label: 'Nome' },
    { key: 'email', label: 'Email' },
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

    this.loadUsuarios();
  }

  loadUsuarios(): void {
    this.hasActiveFilter.set(false);
    this.fetchUsuarios(() => this.usuarioService.list());
  }

  applyFilter(): void {
    if (this.filterForm.invalid) {
      this.filterForm.markAllAsTouched();
      return;
    }

    const { nome, email } = this.filterForm.getRawValue();
    this.hasActiveFilter.set(true);
    this.fetchUsuarios(() => this.usuarioService.filtrar({ nome, email }));
  }

  clearFilter(): void {
    this.filterForm.reset();
    this.loadUsuarios();
  }

  private fetchUsuarios(request: () => ReturnType<UsuarioService['list']>): void {
    this.loading.set(true);
    this.errorMessage.set('');

    request()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (usuarios) => {
          this.dataSource.data = usuarios;
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar os usuários.');
        },
      });
  }

  edit(usuario: Usuario): void {
    if (this.isProtectedUser(usuario)) {
      return;
    }

    void this.router.navigate(['/usuarios', usuario.id, 'editar']);
  }

  isProtectedUser(usuario: Usuario): boolean {
    return isTenantDefaultUser(usuario.email);
  }

  exportExcel(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('usuarios', subdomain, 'xlsx');
    void this.exportService.exportToExcel(this.dataSource.data, this.exportColumns, filename);
  }

  exportPdf(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('usuarios', subdomain, 'pdf');
    void this.exportService.exportToPdf(this.dataSource.data, this.exportColumns, filename, 'Usuários');
  }

  confirmDelete(usuario: Usuario): void {
    if (this.isProtectedUser(usuario)) {
      return;
    }

    const dialogRef = this.dialog.open<ConfirmDeleteDialogComponent, ConfirmDeleteDialogData, boolean>(
      ConfirmDeleteDialogComponent,
      {
        width: CONFIRM_DELETE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: { nome: usuario.nome },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.usuarioService.delete(usuario.id).subscribe({
        next: () => this.loadUsuarios(),
        error: () => {
          this.errorMessage.set('Não foi possível excluir o usuário.');
        },
      });
    });
  }

}
