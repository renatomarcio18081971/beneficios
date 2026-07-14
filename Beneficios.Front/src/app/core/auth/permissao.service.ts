import { Injectable, inject } from '@angular/core';
import { AuthService } from './auth.service';
import { TenantService } from '../tenant/tenant.service';
import { AcaoPermissao } from './modulos-sistema';
import { PermissaoMenu } from './auth.models';

@Injectable({ providedIn: 'root' })
export class PermissaoService {
  private readonly auth = inject(AuthService);
  private readonly tenant = inject(TenantService);

  possuiPermissao(codigoMenu: string, acao: AcaoPermissao): boolean {
    if (this.tenant.isAdminMode()) {
      return false;
    }

    const user = this.auth.currentUser();
    const linha = user?.permissoes?.find(
      (p) => p.codigoMenu.toLowerCase() === codigoMenu.toLowerCase(),
    );
    if (!linha) {
      return false;
    }

    return this.obterFlagAcao(linha, acao);
  }

  private obterFlagAcao(linha: PermissaoMenu, acao: AcaoPermissao): boolean {
    switch (acao) {
      case 'visualizar':
        return linha.visualizar;
      case 'criar':
        return linha.criar;
      case 'editar':
        return linha.editar;
      case 'excluir':
        return linha.excluir;
      default:
        return false;
    }
  }
}
