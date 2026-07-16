import { CurrencyPipe } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ChartConfiguration, ChartData } from 'chart.js';
import { BaseChartDirective, provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { DashboardMockService } from './dashboard-mock.service';
import { DashboardSummaryCardComponent } from './dashboard-summary-card/dashboard-summary-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, MatIconModule, BaseChartDirective, CurrencyPipe, DashboardSummaryCardComponent],
  providers: [provideCharts(withDefaultRegisterables())],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements AfterViewInit {
  private readonly dashboardMock = inject(DashboardMockService);

  chartsVisible = true;

  private readonly transporte = this.dashboardMock.getTransporte();
  private readonly alimentacao = this.dashboardMock.getAlimentacao();

  readonly combustivelResumo = this.dashboardMock.getCombustivelResumo();
  readonly culturaResumo = this.dashboardMock.getCulturaResumo();

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
    layout: {
      padding: { top: 2, right: 2, bottom: 0, left: 0 },
    },
    plugins: {
      legend: { display: false },
    },
    scales: {
      x: {
        ticks: {
          maxRotation: 0,
          autoSkip: true,
          font: { size: 10 },
        },
        grid: { display: false },
      },
      y: {
        ticks: {
          maxTicksLimit: 4,
          font: { size: 10 },
          callback: (value) => this.formatAxisCurrency(Number(value)),
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
    layout: {
      padding: { top: 0, right: 0, bottom: 0, left: 0 },
    },
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          boxWidth: 10,
          padding: 8,
          font: { size: 11 },
        },
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

  private formatAxisCurrency(value: number): string {
    if (value >= 1_000) {
      return `R$ ${Math.round(value / 1_000)}k`;
    }

    return value.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      maximumFractionDigits: 0,
    });
  }
}
