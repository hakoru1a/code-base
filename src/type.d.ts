/* eslint-disable @typescript-eslint/no-explicit-any */
type Dynamic = any;

type DynamicObject = Record<string, Dynamic>;

type ApiResultEmpty = DynamicObject;

type KeyedObject = {
  [key: string]: string | number | KeyedObject | any;
};
