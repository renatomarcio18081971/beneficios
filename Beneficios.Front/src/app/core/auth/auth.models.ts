export type UsuarioPerfil = 'Admin' | 'Empresa';

export interface LoginResponse {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  perfil: UsuarioPerfil;
  empresaId?: string | null;
  empresaDominio: string;
}

export interface UserSession {
  usuarioId: string;
  nome: string;
  email: string;
  perfil: UsuarioPerfil;
  empresaId?: string | null;
  empresaDominio: string;
}

export interface AlterarSenhaRequest {
  codigo: string;
  novaSenha: string;
  confirmarNovaSenha: string;
}

export const TOKEN_KEY = 'beneficios_token';
export const USER_KEY = 'beneficios_user';
