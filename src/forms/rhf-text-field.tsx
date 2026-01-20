import { Controller, useFormContext } from 'react-hook-form';
import { Field, type InputFieldProps } from '@components';

export type RHFTextFieldProps = InputFieldProps & {
  name: string;
};

export function RHFTextField({ name, ...otherProps }: RHFTextFieldProps) {
  const { control } = useFormContext();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <Field.Text {...field} error={!!error?.message} helperText={error?.message || otherProps?.helperText} {...otherProps} />
      )}
    />
  );
}
