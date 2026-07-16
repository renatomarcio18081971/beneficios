import { CurrencyBrlInputDirective } from './currency-brl-input.directive';
import { formatCurrencyBrl, parseCurrencyBrlInput, currencyBrlToDigits } from './currency-brl';

describe('currency-brl utils', () => {
  it('formatCurrencyBrl formata no padrão pt-BR', () => {
    expect(formatCurrencyBrl(0)).toMatch(/R\$\s*0,00/);
    expect(formatCurrencyBrl(4.5)).toMatch(/R\$\s*4,50/);
    expect(formatCurrencyBrl(1234.56)).toMatch(/R\$\s*1\.234,56/);
  });

  it('parseCurrencyBrlInput interpreta dígitos como centavos', () => {
    expect(parseCurrencyBrlInput('')).toBe(0);
    expect(parseCurrencyBrlInput('450')).toBe(4.5);
    expect(parseCurrencyBrlInput('R$ 4,50')).toBe(4.5);
    expect(parseCurrencyBrlInput('R$ 1.234,56')).toBe(1234.56);
  });

  it('currencyBrlToDigits converte valor em centavos', () => {
    expect(currencyBrlToDigits(4.5)).toBe('450');
    expect(currencyBrlToDigits(0)).toBe('0');
  });
});

describe('CurrencyBrlInputDirective', () => {
  it('deve existir o seletor appCurrencyBrl', () => {
    expect(CurrencyBrlInputDirective).toBeTruthy();
  });
});
