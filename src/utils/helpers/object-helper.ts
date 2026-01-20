const objectHelper = {
  pick<T extends object>(obj: T, keys: (keyof T)[]): T {
    return keys.reduce((result: Dynamic, key) => {
      if (key in obj) {
        result[key] = obj[key];
      }
      return result;
    }, {});
  },
  isEmpty<T extends object>(obj: T): boolean {
    if (obj === null || obj === undefined) {
      return true;
    }
    if (typeof obj !== 'object') {
      return false;
    }
    return Object.keys(obj).length === 0 && Object.getOwnPropertySymbols(obj).length === 0;
  }
};

export default objectHelper;
