import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { EmpresaService } from '../../../core/api/empresa.service';
import {
  ConfirmSaveDialogComponent,
  ConfirmSaveDialogData,
  CONFIRM_SAVE_DIALOG_WIDTH,
} from '../../../shared/dialogs/confirm-save-dialog.component';
import {
  EmpresaDefaultUserDialogComponent,
  EMPRESA_DEFAULT_USER_DIALOG_WIDTH,
} from '../../../shared/dialogs/empresa-default-user-dialog.component';
import { buildTenantSchemaName } from '../../../shared/utils/tenant-schema-name';

@Component({
  selector: 'app-empresa-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './empresa-form.component.html',
  styleUrl: './empresa-form.component.scss',
})
export class EmpresaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly empresaService = inject(EmpresaService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly loadingData = signal(false);
  readonly errorMessage = signal('');
  readonly isEdit = signal(false);
  readonly empresaId = signal('');

  readonly form = this.fb.nonNullable.group({
    razaoSocial: ['', [Validators.required, Validators.minLength(2)]],
    dominio: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9]+$/)]],
  });

  onDominioInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const sanitized = input.value.replace(/[^a-zA-Z0-9]/g, '');
    if (sanitized !== input.value) {
      this.form.controls.dominio.setValue(sanitized);
    }
  }

  private readonly razaoSocialValue = toSignal(this.form.controls.razaoSocial.valueChanges, {
    initialValue: this.form.controls.razaoSocial.value,
  });

  readonly tenantSchemaPreview = computed(() => buildTenantSchemaName(this.razaoSocialValue()));

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.empresaId.set(id);
      this.loadEmpresa(id);
    }
  }

  readonly title = computed(() =>
    this.isEdit() ? 'Editar empresa' : 'Nova empresa',
  );

  loadEmpresa(id: string): void {
    this.loadingData.set(true);
    this.errorMessage.set('');

    this.empresaService
      .getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingData.set(false)),
      )
      .subscribe({
        next: (empresa) => {
          this.form.patchValue({
            razaoSocial: empresa.razaoSocial,
            dominio: empresa.dominio,
          });
        },
        error: () => {
          this.errorMessage.set('Não foi possível carregar a empresa.');
        },
      });
  }

  submit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const dialogRef = this.dialog.open<ConfirmSaveDialogComponent, ConfirmSaveDialogData, boolean>(
      ConfirmSaveDialogComponent,
      {
        width: CONFIRM_SAVE_DIALOG_WIDTH,
        maxWidth: '90vw',
        data: {
          nome: this.form.controls.razaoSocial.value,
          entityLabel: 'a empresa',
          isEdit: this.isEdit(),
        },
      },
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.persist();
      }
    });
  }

  private persist(): void {
    const payload = this.form.getRawValue();
    this.loading.set(true);
    this.errorMessage.set('');

    if (this.isEdit()) {
      this.empresaService.update(this.empresaId(), payload).subscribe({
        next: () => void this.router.navigate(['/empresas']),
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Não foi possível atualizar a empresa.');
        },
      });
      return;
    }

    this.empresaService.create(payload).subscribe({
      next: () => {
        this.loading.set(false);
        const dialogRef = this.dialog.open(EmpresaDefaultUserDialogComponent, {
          width: EMPRESA_DEFAULT_USER_DIALOG_WIDTH,
          maxWidth: '90vw',
          disableClose: true,
          data: {
            dominio: payload.dominio,
            razaoSocial: payload.razaoSocial,
          },
        });

        dialogRef.afterClosed().subscribe(() => {
          void this.router.navigate(['/empresas']);
        });
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Não foi possível criar a empresa.');
      },
    });
  }
}
