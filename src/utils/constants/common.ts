export const DEFAULT_PAGE_SIZE = 10;
export const DEFAULT_PAGE = 0;

export const CURRENCIES: Record<string, { name: string; symbol: string; locale: string }> = {
  USD: { name: 'Đô la Mỹ', symbol: '$', locale: 'en-US' },
  VND: { name: 'Việt Nam Đồng', symbol: '₫', locale: 'vi-VN' },
  EUR: { name: 'Euro', symbol: '€', locale: 'de-DE' },
  JPY: { name: 'Yên Nhật', symbol: '¥', locale: 'ja-JP' },
  GBP: { name: 'Bảng Anh', symbol: '£', locale: 'en-GB' },
  CNY: { name: 'Nhân dân tệ', symbol: '¥', locale: 'zh-CN' },
  KRW: { name: 'Won Hàn Quốc', symbol: '₩', locale: 'ko-KR' },
  SGD: { name: 'Đô la Singapore', symbol: 'S$', locale: 'en-SG' },
  AUD: { name: 'Đô la Australia', symbol: 'A$', locale: 'en-AU' }
};

export const HORIZONTAL_MAX_ITEM = 7;
export const DRAWER_WIDTH = 260;
export const MINI_DRAWER_WIDTH = 60;
