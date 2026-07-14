export type UsuarioPerfil = 'Admin' | 'Empresa';

export interface PermissaoMenu {
  codigoMenu: string;
  visualizar: boolean;
  criar: boolean;
  editar: boolean;
  excluir: boolean;
}

export interface LoginResponse {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  perfil: UsuarioPerfil;
  empresaId?: string | null;
  empresaDominio: string;
  perfilAcessoId?: string | null;
  perfilAcessoNome?: string;
  permissoes?: PermissaoMenu[];
}

export interface UserSession {
  usuarioId: string;
  nome: string;
  email: string;
  perfil: UsuarioPerfil;
  empresaId?: string | null;
  empresaDominio: string;
  perfilAcessoId?: string | null;
  perfilAcessoNome?: string;
  permissoes: PermissaoMenu[];
}

export interface AlterarSenhaRequest {
  codigo: string;
  novaSenha: string;
  confirmarNovaSenha: string;
}

export const TOKEN_KEY = 'beneficios_token';
export const USER_KEY = 'beneficios_user';
