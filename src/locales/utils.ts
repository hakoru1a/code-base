export const extractObjectPath = <ObjectPath extends object>(obj: ObjectPath): ObjectPath => {
  const result = {} as ObjectPath;

  const recursivePathCalculation = (source: Dynamic, rootPath: string[] = [], target: Dynamic = result) => {
    for (const key in source) {
      // eslint-disable-next-line no-prototype-builtins
      if (source.hasOwnProperty(key as string)) {
        const path = rootPath.slice();
        path.push(key);

        const value = source[key];
        if (value !== null && typeof value === 'object') {
          recursivePathCalculation(value, path, (target[key] = {}));
        } else {
          target[key] = path.join('.');
        }
      }
    }
  };
  recursivePathCalculation(obj);

  return result;
};
