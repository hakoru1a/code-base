import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

type Props = {
  helperText?: ReactNode;
  error?: string;
};

const FieldHelperText = ({ helperText, error }: Props) => {
  if (!helperText && !error) {
    return null;
  }

  return (
    <Stack spacing={0.5}>
      {error && (
        <Typography color="error" variant="caption">
          {error}
        </Typography>
      )}
      {helperText && (
        <Typography color="textSecondary" variant="caption">
          {helperText}
        </Typography>
      )}
    </Stack>
  );
};

export default FieldHelperText;
