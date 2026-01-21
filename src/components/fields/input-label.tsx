import Typography from '@mui/material/Typography';
import type { InputLabelProps, InputLabelSlotProps } from './types';
import { useTranslate } from '@locales';

const InputLabel = ({ label, required, ...otherProps }: InputLabelProps & InputLabelSlotProps) => {
  const { t } = useTranslate();

  if (!label) return null;

  return (
    <Typography variant="body2" {...otherProps}>
      {t(label)} {required && <strong style={{ color: 'red' }}>*</strong>}
    </Typography>
  );
};

export default InputLabel;
