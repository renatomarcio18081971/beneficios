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
}
