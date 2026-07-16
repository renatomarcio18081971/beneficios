export type TipoAfastamento =
  | 'Ferias'
  | 'LicencaMedica'
  | 'MaternidadePaternidade'
  | 'Acidente'
  | 'Suspensao'
  | 'Outros';

export const TIPOS_AFASTAMENTO: { valor: TipoAfastamento; label: string; codigo: string }[] = [
  { valor: 'Ferias', label: 'Férias', codigo: 'ferias' },
  { valor: 'LicencaMedica', label: 'Licença médica', codigo: 'licenca_medica' },
  { valor: 'MaternidadePaternidade', label: 'Maternidade/paternidade', codigo: 'maternidade_paternidade' },
  { valor: 'Acidente', label: 'Acidente', codigo: 'acidente' },
  { valor: 'Suspensao', label: 'Suspensão', codigo: 'suspensao' },
  { valor: 'Outros', label: 'Outros', codigo: 'outros' },
];

export function labelTipoAfastamento(tipoOuCodigo: string | null | undefined): string {
  if (!tipoOuCodigo) return '—';
  const byValor = TIPOS_AFASTAMENTO.find((t) => t.valor === tipoOuCodigo);
  if (byValor) return byValor.label;
  const byCodigo = TIPOS_AFASTAMENTO.find((t) => t.codigo === tipoOuCodigo);
  return byCodigo?.label ?? tipoOuCodigo;
}

export interface Afastamento {
  id: string;
  funcionarioId: string;
  funcionarioNome: string;
  tipo: TipoAfastamento;
  dataInicio: string;
  dataFim: string | null;
  observacao: string | null;
}

export interface AfastamentoFiltroRequest {
  funcionarioId?: string;
  tipo?: TipoAfastamento;
  dataInicio?: string;
  dataFim?: string;
}

export interface AfastamentoSalvarRequest {
  funcionarioId: string;
  tipo: TipoAfastamento;
  dataInicio: string;
  dataFim: string | null;
  observacao: string | null;
}

export interface AfastamentoAtualizarRequest {
  tipo: TipoAfastamento;
  dataInicio: string;
  dataFim: string | null;
  observacao: string | null;
}
