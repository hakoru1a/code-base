import { Controller, useFormContext } from 'react-hook-form';
import FieldHelperText from './field-helper-text';
import { Field, type NumberFieldProps } from '@components';
import { regex } from '@utils/constants';

export type RHFNumberFieldProps = NumberFieldProps & {
  name: string;
  rulesDisabledBlur?: string[];
};

export function RHFNumberField({ name, error, rulesDisabledBlur, ...otherProps }: RHFNumberFieldProps) {
  const { control } = useFormContext();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field: { onChange, onBlur, ...otherField }, fieldState: { error: errorState } }) => (
        <Field.Number
          error={!!errorState?.message || error}
          onBlur={() => {
            if (rulesDisabledBlur?.includes(errorState?.type || '')) {
              return;
            }
            onBlur();
          }}
          {...otherProps}
          {...otherField}
          helperText={<FieldHelperText error={errorState?.message} helperText={otherProps?.helperText} />}
          onChange={(value) => {
            if (!value) {
              onChange(null);
              return;
            }
            const number = Number(value.replace(regex.REPLACE_NUMBER, ''));
            onChange(number);
          }}
        />
      )}
    />
  );
}
