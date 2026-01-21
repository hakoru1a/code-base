import { LocalStorageKey } from '@utils/constants';

const localStorageHelper = {
  set: (key: LocalStorageKey, value: string): void => {
    localStorage.setItem(key, value);
  },
  get: (key: LocalStorageKey): string | null => {
    return localStorage.getItem(key);
  },
  getJson: <T>(key: LocalStorageKey): T | null => {
    const item = localStorage.getItem(key);
    if (item) {
      try {
        return JSON.parse(item) as T;
      } catch (error) {
        console.error(`Error parsing JSON from localStorage for key "${key}":`, error);
        return null;
      }
    }
    return null;
  },
  remove: (key: LocalStorageKey): void => {
    localStorage.removeItem(key);
  }
};

export default localStorageHelper;
