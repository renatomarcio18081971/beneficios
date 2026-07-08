export function buildTenantSchemaName(razaoSocial: string): string {
  const normalized = razaoSocial.trim().toLowerCase();
  const sanitized = [...normalized]
    .map((character) => (/^[a-z0-9]$/.test(character) ? character : '_'))
    .join('');

  return `tenant_${sanitized}`;
}
