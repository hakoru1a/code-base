import Autocomplete from '@mui/material/Autocomplete';
import TextField from '@mui/material/TextField';
import Stack from '@mui/material/Stack';
import type { AutocompleteFieldProps } from './types';
import { inputBaseClasses, outlinedInputClasses } from '@mui/material';
import { type ForwardedRef, forwardRef, type JSX } from 'react';
import InputLabel from './input-label';

const AutocompleteFieldBase = <
  T,
  Multiple extends boolean | undefined = undefined,
  DisableClearable extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined
>(
  {
    onChange,
    placeholder,
    error,
    label,
    required,
    helperText,
    fullWidth,
    size,
    ...otherProps
  }: AutocompleteFieldProps<T, Multiple, DisableClearable, FreeSolo>,
  ref: ForwardedRef<HTMLDivElement>
) => {
  return (
    <Stack
      spacing={1}
      sx={{
        ...otherProps?.slotProps?.container?.sx,
        ...(fullWidth && {
          width: '100%'
        })
      }}
      {...otherProps?.slotProps?.container}
    >
      <InputLabel label={label} required={required} {...otherProps?.slotProps?.label} />
      <Autocomplete
        noOptionsText="Không tìm thấy kết quả"
        size={size}
        {...otherProps}
        sx={{
          ...otherProps?.sx,
          ...(size === 'small' && {
            [`.${outlinedInputClasses.root}.${inputBaseClasses.sizeSmall}`]: { py: '5px' }
          })
        }}
        onChange={(_, newValue) => onChange?.(newValue as T)}
        renderInput={(params) => (
          <TextField
            {...params}
            placeholder={placeholder || label}
            error={error}
            helperText={helperText}
            inputRef={ref}
            {...otherProps?.slotProps?.textfield}
          />
        )}
        slotProps={{
          popupIndicator: {
            title: 'Mở',
            ...otherProps?.slotProps?.popupIndicator
          },
          ...otherProps?.slotProps
        }}
      />
    </Stack>
  );
};

const AutocompleteField = forwardRef(AutocompleteFieldBase) as <
  T,
  Multiple extends boolean | undefined = undefined,
  DisableClearable extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined
>(
  props: AutocompleteFieldProps<T, Multiple, DisableClearable, FreeSolo> & {
    ref?: ForwardedRef<HTMLDivElement>;
  }
) => JSX.Element;

export default AutocompleteField;
