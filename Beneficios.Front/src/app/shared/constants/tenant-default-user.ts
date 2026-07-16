export const TENANT_DEFAULT_USER = {
  nome: 'Usuário cadastrar',
  email: 'user@123.com',
  senha: 'user@123',
} as const;

export function isTenantDefaultUser(email: string): boolean {
  return email.trim().toLowerCase() === TENANT_DEFAULT_USER.email.toLowerCase();
}
