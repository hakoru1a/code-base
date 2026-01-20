import { Controller, useFormContext } from 'react-hook-form';
import FieldHelperText from './field-helper-text';
import { Field, type DatePickerFieldProps } from '@components';

export type RHFDatePickerProps = DatePickerFieldProps & {
  name: string;
};

export function RHFDatePicker({ name, error, ...otherProps }: RHFDatePickerProps) {
  const { control } = useFormContext();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error: errorState } }) => (
        <Field.DatePicker
          {...field}
          error={!!errorState?.message || !!error}
          helperText={<FieldHelperText error={errorState?.message} helperText={otherProps?.helperText} />}
          {...otherProps}
        />
      )}
    />
  );
}
