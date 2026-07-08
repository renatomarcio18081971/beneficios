/** Formato de exibição da data de inclusão nas listagens (pt-BR). */
export const DATA_INCLUSAO_DATE_PIPE_FORMAT = 'dd/MM/yyyy HH:mm:ss';

export function formatDataInclusao(value: unknown): string {
  if (!value) {
    return '';
  }

  return new Date(String(value)).toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
}
