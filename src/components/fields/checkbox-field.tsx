import { Checkbox, FormControlLabel } from '@mui/material';
import type { CheckboxFieldProps } from './types';

const CheckboxField = ({ label, checked, onChange, acceptNull, controlProps, ...restProps }: CheckboxFieldProps) => {
  const handleChange = () => {
    if (checked === true) {
      onChange?.(false);
    } else if (checked === false && acceptNull) {
      onChange?.(null);
    } else {
      onChange?.(true);
    }
  };

  return (
    <FormControlLabel
      control={<Checkbox {...restProps} checked={!!checked} onClick={handleChange} indeterminate={acceptNull && checked === null} />}
      label={label}
      {...controlProps}
    />
  );
};

export default CheckboxField;
