/** Formatação e parse de valores monetários no padrão brasileiro (BRL). */

const BRL_FORMAT = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/** Formata um número como moeda BRL (ex.: 1234.5 → "R$ 1.234,50"). */
export function formatCurrencyBrl(value: number | null | undefined): string {
  const n = typeof value === 'number' && Number.isFinite(value) ? value : 0;
  return BRL_FORMAT.format(n);
}

/**
 * Converte texto digitado (com ou sem máscara) em número.
 * Usa apenas dígitos como centavos (ex.: "450" / "R$ 4,50" → 4.5).
 */
export function parseCurrencyBrlInput(raw: string | null | undefined): number {
  const digits = String(raw ?? '').replace(/\D/g, '');
  if (!digits) return 0;
  return Number(digits) / 100;
}

/** Extrai só os dígitos (centavos) de um valor numérico para reaplicar a máscara. */
export function currencyBrlToDigits(value: number | null | undefined): string {
  const n = typeof value === 'number' && Number.isFinite(value) ? value : 0;
  return Math.round(Math.abs(n) * 100).toString();
}
