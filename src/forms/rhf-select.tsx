import { Field, type SelectFieldProps } from '@components';
import type { SelectionOption } from '@services/core';
import { Controller, useFormContext } from 'react-hook-form';

export type RHFSelectProps<TData> = SelectFieldProps<TData> & {
  name: string;
};

export const RHFSelect = <TData extends SelectionOption>({ name, ...other }: RHFSelectProps<TData>) => {
  const { control } = useFormContext();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <Field.Select {...field} error={!!error} helperText={error?.message || other?.helperText} {...other} />
      )}
    />
  );
};

export default RHFSelect;
