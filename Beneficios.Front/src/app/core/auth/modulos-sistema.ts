export type AcaoPermissao = 'visualizar' | 'criar' | 'editar' | 'excluir';

export interface ModuloSistema {
  codigo: string;
  nomeExibicao: string;
  rota: string;
  icone: string;
  acoesSuportadas: AcaoPermissao[];
}

export const MODULOS_SISTEMA: readonly ModuloSistema[] = [
  {
    codigo: 'dashboard',
    nomeExibicao: 'Dashboard',
    rota: '/dashboard',
    icone: 'dashboard',
    acoesSuportadas: ['visualizar'],
  },
  {
    codigo: 'usuarios',
    nomeExibicao: 'Usuários',
    rota: '/usuarios',
    icone: 'people',
    acoesSuportadas: ['visualizar', 'criar', 'editar', 'excluir'],
  },
  {
    codigo: 'perfis',
    nomeExibicao: 'Perfis',
    rota: '/perfis',
    icone: 'admin_panel_settings',
    acoesSuportadas: ['visualizar', 'criar', 'editar', 'excluir'],
  },
] as const;
