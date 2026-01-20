import { CURRENCIES } from '@utils/constants';

export type InputNumberValue = string | number | null | undefined;

type Options = {
  maxDigits?: number;
  minDigits?: number;
  hasSign?: boolean;
  zeroFilled?: boolean;
  defaultValue?: string;
  currency?: string;
  locale?: string;
};

const DEFAULT_NUMBER = '---';
const DEFAULT_CURRENCY = { locale: 'en-US', code: 'USD' };

const CURRENCY_NO_DECIMALS = ['JPY', 'KRW', 'VND'];

const processInput = (inputValue: InputNumberValue): number | null => {
  if (inputValue == null || Number.isNaN(inputValue)) return null;
  return Number(inputValue);
};

const buildNumberFormat = (style: 'decimal' | 'currency' | 'percent', options: Options = {}): Intl.NumberFormat => {
  const locale = DEFAULT_CURRENCY;

  const { maxDigits = 2, minDigits = 0 } = options;
  const currentCode = options?.currency || locale.code;
  const currency = CURRENCIES[currentCode];

  const formatter: Intl.NumberFormatOptions = {
    style,
    minimumFractionDigits: minDigits,
    maximumFractionDigits: maxDigits
  };

  if (style === 'currency') {
    formatter.minimumFractionDigits = CURRENCY_NO_DECIMALS.includes(currentCode) ? 0 : minDigits;
    formatter.maximumFractionDigits = CURRENCY_NO_DECIMALS.includes(currentCode) ? 0 : maxDigits;
    formatter.currency = currentCode;
  }

  const localeToUse = options?.locale || currency?.locale || locale.locale;

  return new Intl.NumberFormat(localeToUse, formatter);
};

const numberHelper = {
  isNumber(value: number | string) {
    if (typeof value === 'string') return !isNaN(parseInt(value));

    return !isNaN(value);
  },
  formatNumber(inputValue: InputNumberValue, options?: Options) {
    const number = processInput(inputValue);
    if (number === null) return options?.defaultValue || DEFAULT_NUMBER;

    return buildNumberFormat('decimal', options).format(number);
  },
  formatCurrency(inputValue: InputNumberValue, options?: Options) {
    const number = processInput(inputValue);
    if (number === null) return options?.defaultValue || DEFAULT_NUMBER;

    const result = buildNumberFormat('currency', {
      ...options
    }).format(Math.abs(number));

    return options?.hasSign ? `${this.getNumberSign(number)}${result}` : result;
  },
  formatPercent(inputValue: InputNumberValue, options?: Omit<Options, 'maxDigits'>) {
    const number = processInput(inputValue);
    if (number === null) return options?.defaultValue || DEFAULT_NUMBER;

    const result = buildNumberFormat('decimal', {
      maxDigits: 2,
      minDigits: 2,
      ...options
    }).format(Math.abs(number));

    return options?.hasSign ? `${this.getNumberSign(number)}${result}%` : `${result}%`;
  },
  formatNumberUnit(value: string) {
    if (!value || value.trim() === '') return null;
    // Remove all non-numeric characters except decimal point and comma
    const cleanValue = value.replace(/[^\d.,-]/g, '').replace(/,/g, '');
    const parsed = parseFloat(cleanValue);
    return isNaN(parsed) ? null : parsed;
  },
  getNumberSign(value: number | string) {
    return this.getValueForZero(value, '', '+', '-');
  },
  getNumberColor(value: number | string) {
    return this.getValueForZero(value, '#262626', 'success.main', 'error.main');
  },
  getValueForZero(value: number | string, equals: string, greater: string, less: string) {
    if (typeof value === 'string') {
      value = parseFloat(value);
    }
    return value === 0 ? equals : value > 0 ? greater : less;
  }
};

export default numberHelper;
