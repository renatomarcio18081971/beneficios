export interface Empresa {
  id: string;
  razaoSocial: string;
  dominio: string;
  dataInclusao: string;
  dataAlteracao: string | null;
}

export interface EmpresaSalvarRequest {
  razaoSocial: string;
  dominio: string;
  nomeBanco: string;
  usuarioBanco: string;
  senhaBanco: string;
}

export interface EmpresaAtualizarRequest {
  razaoSocial: string;
  dominio: string;
  nomeBanco: string;
  usuarioBanco: string;
  senhaBanco: string;
}

export interface EmpresaCreateResponse {
  id: string;
}
