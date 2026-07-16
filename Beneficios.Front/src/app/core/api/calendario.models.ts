export type TipoExcecaoCalendario =
  | 'Nacional'
  | 'Estadual'
  | 'Municipal'
  | 'FeriasColetivas'
  | 'PontoFacultativo';

export type OrigemCalendarioDia = 'Geracao' | 'Nacional' | 'Manual';

export interface CalendarioDia {
  id: string;
  data: string;
  ehDiaUtil: boolean;
  tipoExcecao: TipoExcecaoCalendario | null;
  origem: OrigemCalendarioDia;
  observacao: string | null;
}

export interface CalendarioDiaAtualizarRequest {
  ehDiaUtil: boolean;
  tipoExcecao: TipoExcecaoCalendario | null;
  observacao: string | null;
}

export const TIPOS_EXCECAO_CALENDARIO: readonly {
  valor: TipoExcecaoCalendario;
  label: string;
}[] = [
  { valor: 'Nacional', label: 'Nacional' },
  { valor: 'Estadual', label: 'Estadual' },
  { valor: 'Municipal', label: 'Municipal' },
  { valor: 'FeriasColetivas', label: 'Férias coletivas' },
  { valor: 'PontoFacultativo', label: 'Ponto facultativo' },
] as const;
