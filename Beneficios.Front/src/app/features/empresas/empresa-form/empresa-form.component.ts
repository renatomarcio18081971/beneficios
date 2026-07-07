import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EmpresaService } from '../../../core/api/empresa.service';

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
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './empresa-form.component.scss',
})
export class EmpresaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly empresaService = inject(EmpresaService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  loading = false;
  loadingData = false;
  errorMessage = '';
  isEdit = false;
  empresaId = '';

  readonly form = this.fb.nonNullable.group({
    razaoSocial: ['', [Validators.required, Validators.minLength(2)]],
    dominio: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
    nomeBanco: ['', [Validators.required]],
    usuarioBanco: ['', [Validators.required]],
    senhaBanco: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.empresaId = id;
      this.form.controls.senhaBanco.clearValidators();
      this.loadEmpresa(id);
      return;
    }

    this.form.controls.senhaBanco.setValidators([Validators.required, Validators.minLength(4)]);
    this.form.controls.senhaBanco.updateValueAndValidity();
  }

  get title(): string {
    return this.isEdit ? 'Editar empresa' : 'Nova empresa';
  }

  loadEmpresa(id: string): void {
    this.loadingData = true;
    this.empresaService.getById(id).subscribe({
      next: (empresa) => {
        this.form.patchValue({
          razaoSocial: empresa.razaoSocial,
          dominio: empresa.dominio,
        });
        this.loadingData = false;
      },
      error: () => {
        this.loadingData = false;
        this.errorMessage = 'Não foi possível carregar a empresa.';
      },
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    this.loading = true;
    this.errorMessage = '';

    if (this.isEdit) {
      this.empresaService.update(this.empresaId, payload).subscribe({
        next: () => void this.router.navigate(['/empresas']),
        error: () => {
          this.loading = false;
          this.errorMessage = 'Não foi possível atualizar a empresa.';
        },
      });
      return;
    }

    this.empresaService.create(payload).subscribe({
      next: () => void this.router.navigate(['/empresas']),
      error: () => {
        this.loading = false;
        this.errorMessage = 'Não foi possível criar a empresa.';
      },
    });
  }
}
