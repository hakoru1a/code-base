import React, { forwardRef, useCallback } from 'react';

import type { NumberFieldProps, NumberFormatProps } from './types';
import { type NumberFormatValues, NumericFormat } from 'react-number-format';
import InputField from './input-field.tsx';
import { numberHelper } from '@utils/helpers';
import { regex } from '@utils/constants';

const MAX_VALUE = 100_000_000_000;

const NumberFormat = forwardRef((props: NumberFormatProps, ref) => {
  const { onChange, max = MAX_VALUE, min, allowNegative = false, ...other } = props;

  const onValueChange = useCallback(
    (values: NumberFormatValues) => {
      onChange({
        target: {
          value: values.formattedValue
        }
      });
    },
    [onChange]
  );

  const onBlur = (event: Dynamic) => {
    const {
      target: { value }
    } = event;
    if (!numberHelper.isNumber(value)) {
      onChange({ target: { value: '' } });
      props?.onBlur?.(value);
      return;
    }

    const currentValue = Number(value.replace(regex.REPLACE_NUMBER, ''));

    const handleValueChange = (limit: number | string | undefined, comparator: (current: number, limitValue: number) => boolean) => {
      if (numberHelper.isNumber(limit || '')) {
        const limitValue = limit as number;
        if (comparator(currentValue, limitValue)) {
          onChange({ target: { value: limitValue.toString() } });
        }
      }
    };

    handleValueChange(max, (current, maxVal) => current > maxVal);
    handleValueChange(min, (current, minVal) => minVal > current);
    props?.onBlur?.(value);
  };

  return (
    <NumericFormat
      decimalScale={0}
      thousandSeparator
      allowLeadingZeros
      allowNegative={allowNegative}
      {...other}
      getInputRef={ref}
      onBlur={onBlur}
      onValueChange={onValueChange}
    />
  );
});

const NumberField = React.forwardRef(({ onChange, ...otherProps }: NumberFieldProps, ref) => {
  return (
    <InputField
      {...otherProps}
      inputRef={ref}
      onChange={(value) => onChange?.(`${value}`)}
      slotProps={{
        ...otherProps?.slotProps,
        input: {
          ...otherProps?.slotProps?.input,
          inputComponent: NumberFormat as Dynamic
        },
        htmlInput: {
          ...otherProps?.slotProps?.number,
          ...otherProps?.slotProps?.htmlInput
        }
      }}
    />
  );
});

export default NumberField;
