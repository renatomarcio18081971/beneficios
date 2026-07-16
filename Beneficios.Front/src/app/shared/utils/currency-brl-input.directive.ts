import { Directive, ElementRef, HostListener, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { currencyBrlToDigits, formatCurrencyBrl, parseCurrencyBrlInput } from './currency-brl';

/**
 * Input de moeda BRL (pt-BR) para reactive forms.
 * O controle do formulário guarda `number`; a UI exibe "R$ 1.234,56".
 *
 * Uso: `<input matInput appCurrencyBrl formControlName="valorTarifa" />`
 */
@Directive({
  selector: 'input[appCurrencyBrl]',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CurrencyBrlInputDirective),
      multi: true,
    },
  ],
})
export class CurrencyBrlInputDirective implements ControlValueAccessor {
  private readonly el = inject(ElementRef<HTMLInputElement>);
  private onChange: (value: number) => void = () => undefined;
  private onTouched: () => void = () => undefined;
  private disabled = false;

  writeValue(value: number | null): void {
    this.el.nativeElement.value = formatCurrencyBrl(value ?? 0);
  }

  registerOnChange(fn: (value: number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.el.nativeElement.disabled = isDisabled;
  }

  @HostListener('input')
  onInput(): void {
    if (this.disabled) return;
    const parsed = parseCurrencyBrlInput(this.el.nativeElement.value);
    this.el.nativeElement.value = formatCurrencyBrl(parsed);
    this.onChange(parsed);
  }

  @HostListener('blur')
  onBlur(): void {
    this.onTouched();
    const parsed = parseCurrencyBrlInput(this.el.nativeElement.value);
    this.el.nativeElement.value = formatCurrencyBrl(parsed);
  }

  @HostListener('focus')
  onFocus(): void {
    if (this.disabled) return;
    // Mantém só dígitos selecionáveis para facilitar edição (centavos).
    const digits = currencyBrlToDigits(parseCurrencyBrlInput(this.el.nativeElement.value));
    this.el.nativeElement.value = digits === '0' ? '' : digits;
    this.el.nativeElement.select();
  }
}
