import { extractObjectPath } from './utils';
import vi from './languages/vi.json';

export * from './i18n';
export * from './hook';
export * from './types';

export const locales = extractObjectPath({ ...vi });
