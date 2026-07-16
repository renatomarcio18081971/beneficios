export interface Usuario {
  id: string;
  nome: string;
  email: string;
  empresaId: string;
  perfilId?: string | null;
  perfilAcessoNome?: string | null;
  empresaNome: string;
  dataInclusao: string;
  dataAlteracao: string | null;
}

export interface UsuarioSalvarRequest {
  nome: string;
  senha: string;
  email: string;
  empresaId: string;
  perfilId: string;
}

export interface UsuarioAtualizarRequest {
  nome: string;
  email: string;
  empresaId: string;
  perfilId: string;
}

export interface UsuarioCreateResponse {
  id: string;
}

export interface UsuarioFiltroRequest {
  nome?: string;
  email?: string;
}
