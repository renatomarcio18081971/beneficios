export interface LinhaOnibusDto {
  id: string;
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}

export interface LinhaOnibusSalvarDto {
  descricao: string;
  dataInicio: string;
  dataFim: string | null;
  valorTarifa: number;
}

export interface LinhaOnibusFiltroRequest {
  descricao?: string;
  somenteVigentes?: boolean;
  /** yyyy-MM-dd — data de referência para filtro de vigência (default: hoje no servidor). */
  referencia?: string;
}
