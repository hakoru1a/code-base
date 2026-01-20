import Typography from '@mui/material/Typography';
import type { InputLabelProps, InputLabelSlotProps } from './types';

const InputLabel = ({ label, required, ...otherProps }: InputLabelProps & InputLabelSlotProps) => {
  if (!label) return null;

  return (
    <Typography variant="body2" {...otherProps}>
      {label} {required && <strong style={{ color: 'red' }}>*</strong>}
    </Typography>
  );
};

export default InputLabel;
