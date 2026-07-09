import { isTenantDefaultUser } from '../constants/tenant-default-user';

describe('isTenantDefaultUser', () => {
  it('deve identificar o email do usuário padrão', () => {
    expect(isTenantDefaultUser('user@123.com')).toBe(true);
    expect(isTenantDefaultUser('USER@123.COM')).toBe(true);
  });

  it('não deve identificar outros emails', () => {
    expect(isTenantDefaultUser('outro@exemplo.com')).toBe(false);
  });
});
