import { TestBed } from '@angular/core/testing';
import { DashboardMockService } from './dashboard-mock.service';

describe('DashboardMockService', () => {
  let service: DashboardMockService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DashboardMockService);
  });

  describe('getTransporte', () => {
    it('deve retornar gastos dos últimos 6 meses', () => {
      const data = service.getTransporte();

      expect(data.meses).toHaveSize(6);
      expect(data.meses.every((m) => m.label.length > 0)).toBeTrue();
      expect(data.meses.every((m) => m.valor > 0)).toBeTrue();
    });

    it('total deve ser a soma dos valores mensais', () => {
      const data = service.getTransporte();
      const soma = data.meses.reduce((acc, m) => acc + m.valor, 0);

      expect(data.total).toBe(soma);
    });
  });

  describe('getAlimentacao', () => {
    it('deve retornar categorias Refeições, Mercado e Outros com percentuais 52/35/13', () => {
      const data = service.getAlimentacao();

      expect(data.categorias).toHaveSize(3);
      expect(data.categorias.map((c) => c.label)).toEqual(['Refeições', 'Mercado', 'Outros']);
      expect(data.categorias.map((c) => c.percentual)).toEqual([52, 35, 13]);
    });

    it('percentuais devem somar 100 e valores devem somar o total', () => {
      const data = service.getAlimentacao();
      const somaPercentuais = data.categorias.reduce((acc, c) => acc + c.percentual, 0);
      const somaValores = data.categorias.reduce((acc, c) => acc + c.valor, 0);

      expect(somaPercentuais).toBe(100);
      expect(somaValores).toBe(data.total);
    });
  });

  describe('getCombustivelResumo', () => {
    it('deve retornar 4 categorias com soma igual ao valor total', () => {
      const data = service.getCombustivelResumo();

      expect(data.titulo).toBe('Vale Combustível');
      expect(data.tema).toBe('combustivel');
      expect(data.categorias).toHaveSize(4);
      expect(data.categorias.map((c) => c.label)).toEqual([
        'Deslocamento',
        'Viagens',
        'Entregas',
        'Outros',
      ]);

      const soma = data.categorias.reduce((acc, c) => acc + c.valor, 0);
      expect(soma).toBe(data.valorTotal);
    });

    it('deve conter textos em português', () => {
      const data = service.getCombustivelResumo();

      expect(data.periodoLabel).toContain('Período');
      expect(data.insight).toContain('mês passado');
      expect(data.diasParaVencimento).toBe(6);
    });
  });

  describe('getCulturaResumo', () => {
    it('deve retornar 4 categorias com soma igual ao valor total', () => {
      const data = service.getCulturaResumo();

      expect(data.titulo).toBe('Vale Cultura');
      expect(data.tema).toBe('cultura');
      expect(data.categorias).toHaveSize(4);
      expect(data.categorias.map((c) => c.label)).toEqual(['Cinema', 'Livros', 'Teatro', 'Outros']);

      const soma = data.categorias.reduce((acc, c) => acc + c.valor, 0);
      expect(soma).toBe(data.valorTotal);
    });

    it('deve conter textos em português', () => {
      const data = service.getCulturaResumo();

      expect(data.periodoLabel).toContain('Período');
      expect(data.insight).toContain('mês anterior');
      expect(data.diasParaVencimento).toBe(12);
    });
  });
});
