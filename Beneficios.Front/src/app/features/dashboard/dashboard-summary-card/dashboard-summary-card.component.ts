import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ChartConfiguration, ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardResumoBeneficio } from '../dashboard-mock.service';

@Component({
  selector: 'app-dashboard-summary-card',
  standalone: true,
  imports: [MatCardModule, MatIconModule, BaseChartDirective, CurrencyPipe, DatePipe],
  templateUrl: './dashboard-summary-card.component.html',
  styleUrl: './dashboard-summary-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardSummaryCardComponent {
  readonly resumo = input.required<DashboardResumoBeneficio>();

  readonly chartVisible = true;

  readonly doughnutChartData = computed<ChartData<'doughnut'>>(() => {
    const data = this.resumo();
    return {
      labels: data.categorias.map((c) => c.label),
      datasets: [
        {
          data: data.categorias.map((c) => c.valor),
          backgroundColor: data.categorias.map((c) => c.cor),
          borderWidth: 0,
        },
      ],
    };
  });

  readonly doughnutChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '70%',
    layout: {
      padding: { top: 4, right: 4, bottom: 4, left: 4 },
    },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => {
            const valor = context.parsed;
            const formatted = valor.toLocaleString('pt-BR', {
              style: 'currency',
              currency: 'BRL',
            });
            return `${context.label}: ${formatted}`;
          },
        },
      },
    },
  };
}
