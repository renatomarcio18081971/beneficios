export interface FuncionarioLinhaDto {
  id: string;
  funcionarioId: string;
  funcionarioNome: string;
  linhaOnibusId: string;
  linhaDescricao: string;
  quantidade: number;
  dataInicio: string;
  dataFim: string | null;
}

export interface FuncionarioLinhaFiltroRequest {
  funcionarioId?: string;
  somenteVigentes?: boolean;
}

export interface FuncionarioLinhaSalvarRequest {
  funcionarioId: string;
  linhaOnibusId: string;
  quantidade: number;
  dataInicio: string;
  dataFim: string | null;
}

export interface FuncionarioLinhaAtualizarRequest {
  linhaOnibusId: string;
  quantidade: number;
  dataInicio: string;
  dataFim: string | null;
}
