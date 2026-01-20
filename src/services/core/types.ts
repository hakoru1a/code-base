export type SelectionOption<T = DynamicObject> = {
  label: string;
  value: string | number;
  disabled?: boolean;
  metadata?: T;
};
