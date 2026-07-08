import { Injectable } from '@angular/core';

export interface MesGasto {
  label: string;
  valor: number;
}

export interface CategoriaGasto {
  label: string;
  percentual: number;
  valor: number;
}

export interface TransporteDashboard {
  total: number;
  meses: MesGasto[];
}

export interface AlimentacaoDashboard {
  total: number;
  categorias: CategoriaGasto[];
}

export interface DashboardCategoriaResumo {
  label: string;
  valor: number;
  cor: string;
}

export interface DashboardResumoBeneficio {
  titulo: string;
  icone: string;
  tema: 'combustivel' | 'cultura';
  diasParaVencimento: number;
  dataVencimento: string;
  valorTotal: number;
  periodoLabel: string;
  categorias: DashboardCategoriaResumo[];
  insight: string;
}

@Injectable({ providedIn: 'root' })
export class DashboardMockService {
  getTransporte(): TransporteDashboard {
    const meses: MesGasto[] = [
      { label: 'Jan', valor: 42_500 },
      { label: 'Fev', valor: 45_800 },
      { label: 'Mar', valor: 41_200 },
      { label: 'Abr', valor: 48_600 },
      { label: 'Mai', valor: 46_900 },
      { label: 'Jun', valor: 49_500 },
    ];

    return {
      meses,
      total: meses.reduce((acc, m) => acc + m.valor, 0),
    };
  }

  getAlimentacao(): AlimentacaoDashboard {
    const total = 128_400;
    const categorias: CategoriaGasto[] = [
      { label: 'Refeições', percentual: 52, valor: Math.round(total * 0.52) },
      { label: 'Mercado', percentual: 35, valor: Math.round(total * 0.35) },
      { label: 'Outros', percentual: 13, valor: Math.round(total * 0.13) },
    ];

    return { total, categorias };
  }

  getCombustivelResumo(): DashboardResumoBeneficio {
    const categorias: DashboardCategoriaResumo[] = [
      { label: 'Deslocamento', valor: 8_920, cor: '#43A047' },
      { label: 'Viagens', valor: 5_430, cor: '#7CB342' },
      { label: 'Entregas', valor: 3_100, cor: '#FDD835' },
      { label: 'Outros', valor: 1_300, cor: '#9E9E9E' },
    ];

    return {
      titulo: 'Vale Combustível',
      icone: 'local_gas_station',
      tema: 'combustivel',
      diasParaVencimento: 6,
      dataVencimento: '2026-07-14T00:00:00',
      valorTotal: categorias.reduce((acc, c) => acc + c.valor, 0),
      periodoLabel: 'Período 01.07–31.07',
      categorias,
      insight:
        'Este valor é R$ 1.240,00 maior que o mês passado, mas está alinhado com a média dos últimos 3 meses.',
    };
  }

  getCulturaResumo(): DashboardResumoBeneficio {
    const categorias: DashboardCategoriaResumo[] = [
      { label: 'Cinema', valor: 2_480, cor: '#7E57C2' },
      { label: 'Livros', valor: 1_920, cor: '#5C6BC0' },
      { label: 'Teatro', valor: 1_120, cor: '#42A5F5' },
      { label: 'Outros', valor: 800, cor: '#9E9E9E' },
    ];

    return {
      titulo: 'Vale Cultura',
      icone: 'theater_comedy',
      tema: 'cultura',
      diasParaVencimento: 12,
      dataVencimento: '2026-07-20T00:00:00',
      valorTotal: categorias.reduce((acc, c) => acc + c.valor, 0),
      periodoLabel: 'Período 01.07–31.07',
      categorias,
      insight:
        'O consumo caiu R$ 480,00 em relação ao mês anterior, com maior uso em cinema e livros.',
    };
  }
}
