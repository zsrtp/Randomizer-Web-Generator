function filterKeys(
  obj: { [key: string]: any },
  keys: string[] | string
): object {
  if (
    !obj ||
    typeof obj !== 'object' ||
    !keys ||
    (typeof keys !== 'string' && !Array.isArray(keys))
  ) {
    return obj;
  }

  let willChange = false;

  const keysArr = typeof keys === 'string' ? [keys] : keys;
  const keysToFilterDict = keysArr.reduce(
    (acc: { [key: string]: any }, key: string) => {
      acc[key] = true;
      if (obj.hasOwnProperty(key)) {
        willChange = true;
      }
      return acc;
    },
    {}
  );

  if (!willChange) {
    return obj;
  }

  return Object.keys(obj).reduce(
    (newObj: { [key: string]: any }, key: string) => {
      if (!keysToFilterDict[key]) {
        newObj[key] = obj[key];
      }
      return newObj;
    },
    {}
  );
}

export { filterKeys };
