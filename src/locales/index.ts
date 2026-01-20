import { extractObjectPath } from './utils';
import en from './languages/en.json';

export * from './i18n';
export * from './hook';
export * from './types';
export * from './context';

export const locales = extractObjectPath({ ...en });
