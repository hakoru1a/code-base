import { MenuItem } from '@mui/material';
import type { SelectionOption } from '@services/core';
import type { SelectFieldProps } from './types';
import InputField from './input-field';

const SelectField = <TData extends SelectionOption>({
  options,
  defaultOptionLabel,
  labelOption = 'label',
  valueOption = 'value',
  onChange,
  renderLabel,
  slotProps = {},
  ...otherProps
}: SelectFieldProps<TData>) => {
  const handleChange = (value: string | number) => onChange?.(value);

  const { item: itemSlotProps, ...inputProps } = slotProps || {};

  return (
    <InputField onChange={handleChange} slotProps={inputProps} {...otherProps} select>
      {defaultOptionLabel && (
        <MenuItem value="">
          <em>{defaultOptionLabel}</em>
        </MenuItem>
      )}
      {options.map((option: Dynamic) => (
        <MenuItem key={option[valueOption]} value={option[valueOption]} disabled={option.disabled} {...itemSlotProps}>
          {renderLabel ? renderLabel(option) : option[labelOption]}
        </MenuItem>
      ))}
    </InputField>
  );
};

export default SelectField;
