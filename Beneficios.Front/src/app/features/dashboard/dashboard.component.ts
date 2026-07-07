import { CurrencyPipe } from '@angular/common';
import { AfterViewInit, Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { ChartConfiguration, ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardMockService } from './dashboard-mock.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, BaseChartDirective, CurrencyPipe],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements AfterViewInit {
  private readonly dashboardMock = inject(DashboardMockService);

  chartsVisible = true;

  private readonly transporte = this.dashboardMock.getTransporte();
  private readonly alimentacao = this.dashboardMock.getAlimentacao();

  readonly transporteTotal = this.transporte.total;
  readonly alimentacaoTotal = this.alimentacao.total;

  readonly barChartData: ChartData<'bar'> = {
    labels: this.transporte.meses.map((m) => m.label),
    datasets: [
      {
        data: this.transporte.meses.map((m) => m.valor),
        backgroundColor: '#1565C0',
        borderRadius: 4,
      },
    ],
  };

  readonly barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: {
        ticks: {
          callback: (value) =>
            Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 }),
        },
      },
    },
  };

  readonly pieChartData: ChartData<'pie'> = {
    labels: this.alimentacao.categorias.map((c) => c.label),
    datasets: [
      {
        data: this.alimentacao.categorias.map((c) => c.valor),
        backgroundColor: ['#43A047', '#FB8C00', '#29B6F6'],
      },
    ],
  };

  readonly pieChartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const categoria = this.alimentacao.categorias[context.dataIndex];
            return `${categoria.label}: ${categoria.percentual}%`;
          },
        },
      },
    },
  };

  ngAfterViewInit(): void {
    this.refreshCharts();
  }

  private refreshCharts(): void {
    this.chartsVisible = false;
    queueMicrotask(() => {
      this.chartsVisible = true;
    });
  }
}
