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
}
