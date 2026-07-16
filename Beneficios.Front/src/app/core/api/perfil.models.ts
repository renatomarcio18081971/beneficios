import { PermissaoMenu } from '../auth/auth.models';

export interface Perfil {
  id: string;
  nome: string;
  ehSistema: boolean;
  dataInclusao: string;
  dataAlteracao: string | null;
  permissoes: PermissaoMenu[];
}

export interface PerfilSalvarRequest {
  nome: string;
  permissoes: PermissaoMenu[];
}

export interface PerfilAtualizarRequest {
  nome: string;
  permissoes: PermissaoMenu[];
}

export interface PerfilCreateResponse {
  id: string;
}

export interface PerfilFiltroRequest {
  nome?: string;
}
