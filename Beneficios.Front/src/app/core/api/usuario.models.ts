export interface Usuario {
  id: string;
  nome: string;
  email: string;
  empresaId: string;
  empresaNome: string;
  dataInclusao: string;
  dataAlteracao: string | null;
}

export interface UsuarioSalvarRequest {
  nome: string;
  senha: string;
  email: string;
  empresaId: string;
}

export interface UsuarioAtualizarRequest {
  nome: string;
  email: string;
  empresaId: string;
}

export interface UsuarioCreateResponse {
  id: string;
}
