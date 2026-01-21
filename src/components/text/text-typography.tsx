import { Typography, TypographyProps } from '@mui/material';
import { ReactNode } from 'react';
import { useTranslate } from '@locales';

type Props = TypographyProps & {
  label: ReactNode;
};

const TextTypography = ({ label, ...otherProps }: Props) => {
  const { t } = useTranslate();
  if (typeof label === 'string') {
    return <Typography {...otherProps}>{t(label)}</Typography>;
  }

  return <Typography {...otherProps}>{label}</Typography>;
};

export default TextTypography;
