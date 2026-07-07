import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { AfterViewInit, Component, DestroyRef, ViewChild, inject, ChangeDetectionStrategy } from '@angular/core';
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
import { UsuarioService } from '../../../core/api/usuario.service';
import { Usuario } from '../../../core/api/usuario.models';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ExportColumn } from '../../../shared/utils/export.models';
import { ExportService } from '../../../shared/utils/export.service';
import { applyTableData, syncTableRows } from '../../../shared/utils/list-page.helpers';
import {
  ConfirmDeleteDialogComponent,
  ConfirmDeleteDialogData,
} from '../../../shared/dialogs/confirm-delete-dialog.component';

@Component({
  selector: 'app-usuario-list',
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
  templateUrl: './usuario-list.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './usuario-list.component.scss',
})
export class UsuarioListComponent implements AfterViewInit {
  @ViewChild(MatPaginator) set matPaginator(paginator: MatPaginator | undefined) {
    if (paginator) {
      this.dataSource.paginator = paginator;
    }
  }

  @ViewChild(MatTable) set matTable(table: MatTable<Usuario> | undefined) {
    this.table = table;
    this.syncTableRows();
  }

  private table?: MatTable<Usuario>;

  private readonly usuarioService = inject(UsuarioService);
  private readonly exportService = inject(ExportService);
  private readonly tenantService = inject(TenantService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly displayedColumns = ['nome', 'email', 'dataInclusao', 'acoes'];
  readonly dataSource = new MatTableDataSource<Usuario>([]);

  loading = true;
  errorMessage = '';
  isMobile = false;

  private readonly exportColumns: ExportColumn[] = [
    { key: 'nome', label: 'Nome' },
    { key: 'email', label: 'Email' },
    {
      key: 'dataInclusao',
      label: 'Data inclusão',
      format: (value) => this.formatDate(value),
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

  ngAfterViewInit(): void {
    this.loadUsuarios();
  }

  loadUsuarios(): void {
    this.loading = true;
    this.errorMessage = '';

    this.usuarioService.list().subscribe({
      next: (usuarios) => {
        applyTableData(this.dataSource, usuarios);
        this.loading = false;
        this.syncTableRows();
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Não foi possível carregar os usuários.';
      },
    });
  }

  edit(usuario: Usuario): void {
    void this.router.navigate(['/usuarios', usuario.id, 'editar']);
  }

  exportExcel(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('usuarios', subdomain, 'xlsx');
    this.exportService.exportToExcel(this.dataSource.data, this.exportColumns, filename);
  }

  exportPdf(): void {
    const subdomain = this.tenantService.getSubdomain();
    const filename = this.exportService.buildFilename('usuarios', subdomain, 'pdf');
    this.exportService.exportToPdf(this.dataSource.data, this.exportColumns, filename, 'Usuários');
  }

  confirmDelete(usuario: Usuario): void {
    const dialogRef = this.dialog.open<ConfirmDeleteDialogComponent, ConfirmDeleteDialogData, boolean>(
      ConfirmDeleteDialogComponent,
      {
        width: '360px',
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
          this.errorMessage = 'Não foi possível excluir o usuário.';
        },
      });
    });
  }

  private syncTableRows(): void {
    if (this.dataSource.data.length > 0) {
      syncTableRows(this.table);
    }
  }

  private formatDate(value: unknown): string {
    if (!value) {
      return '';
    }

    return new Date(String(value)).toLocaleDateString('pt-BR');
  }
}
