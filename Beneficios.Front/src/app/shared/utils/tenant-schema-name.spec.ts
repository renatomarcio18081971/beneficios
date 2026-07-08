import { buildTenantSchemaName } from './tenant-schema-name';

describe('buildTenantSchemaName', () => {
  it('deve gerar schema a partir da razao social', () => {
    expect(buildTenantSchemaName('Empresa Exemplo LTDA')).toBe('tenant_empresa_exemplo_ltda');
  });

  it('deve sanitizar caracteres especiais', () => {
    expect(buildTenantSchemaName('Empresa-1')).toBe('tenant_empresa_1');
  });

  it('deve retornar prefixo tenant_ para valor vazio', () => {
    expect(buildTenantSchemaName('')).toBe('tenant_');
  });
});
