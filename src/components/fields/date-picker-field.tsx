import Stack from '@mui/material/Stack';
import InputLabel from './input-label';
import { DatePicker } from '@mui/x-date-pickers';
import TextField from '@mui/material/TextField';
import type { DatePickerFieldProps } from './types';
import { dateHelper, formatPatterns, MAX_DATE, MIN_DATE } from '@utils/helpers';

const DatePickerField = ({
  label,
  value,
  onChange,
  required,
  maxDate,
  minDate,
  error,
  helperText,
  placeholder,
  fullWidth,
  slotProps,
  ...otherProps
}: DatePickerFieldProps) => {
  const { label: labelProps, textField: textFieldProps, container: containerProps, ...otherSlotProps } = slotProps || {};

  return (
    <Stack
      spacing={1}
      sx={{
        ...containerProps?.sx,
        ...(fullWidth && {
          width: '100%'
        })
      }}
      {...containerProps}
    >
      <InputLabel label={label} required={required} {...labelProps} />

      <DatePicker
        format={formatPatterns.date}
        {...otherProps}
        value={dateHelper.normalizeDateValue(value)}
        onChange={(newValue) => onChange?.(dateHelper.normalizeDateValue(newValue))}
        enableAccessibleFieldDOMStructure={false}
        minDate={minDate || dateHelper.from(MIN_DATE)}
        maxDate={maxDate || dateHelper.from(MAX_DATE)}
        slotProps={{
          ...otherSlotProps,
          textField: {
            fullWidth,
            helperText,
            error,
            placeholder,
            ...textFieldProps
          }
        }}
        slots={{
          textField: TextField
        }}
      />
    </Stack>
  );
};

export default DatePickerField;
