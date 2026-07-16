import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { DashboardMockService } from '../dashboard-mock.service';
import { DashboardSummaryCardComponent } from './dashboard-summary-card.component';

registerLocaleData(localePt);

describe('DashboardSummaryCardComponent', () => {
  let fixture: ComponentFixture<DashboardSummaryCardComponent>;
  let mockService: DashboardMockService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardSummaryCardComponent],
      providers: [provideAnimationsAsync(), provideCharts(withDefaultRegisterables())],
    }).compileComponents();

    mockService = TestBed.inject(DashboardMockService);
    fixture = TestBed.createComponent(DashboardSummaryCardComponent);
    fixture.componentRef.setInput('resumo', mockService.getCombustivelResumo());
    fixture.detectChanges();
  });

  it('deve renderizar título e valor total', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.summary-card__title')?.textContent).toContain('Vale Combustível');
    expect(element.querySelector('.summary-card__chart-total')?.textContent).toContain('18.750');
  });

  it('deve renderizar todas as categorias na legenda', () => {
    const legendItems = fixture.nativeElement.querySelectorAll('.summary-card__legend-item');

    expect(legendItems.length).toBe(4);
  });

  it('deve exibir insight em português', () => {
    const insight = fixture.nativeElement.querySelector('.summary-card__insight-text');

    expect(insight?.textContent).toContain('mês passado');
  });
});
