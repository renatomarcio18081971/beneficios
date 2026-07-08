import { DATA_INCLUSAO_DATE_PIPE_FORMAT, formatDataInclusao } from './date-format';

describe('date-format', () => {
  it('deve expor formato com data e hora', () => {
    expect(DATA_INCLUSAO_DATE_PIPE_FORMAT).toBe('dd/MM/yyyy HH:mm:ss');
  });

  it('deve formatar data de inclusão com hora em pt-BR', () => {
    const formatted = formatDataInclusao('2024-01-15T10:30:45');

    expect(formatted).toMatch(/15\/01\/2024/);
    expect(formatted).toMatch(/10:30:45/);
  });

  it('deve retornar string vazia para valor ausente', () => {
    expect(formatDataInclusao(null)).toBe('');
    expect(formatDataInclusao(undefined)).toBe('');
    expect(formatDataInclusao('')).toBe('');
  });
});
