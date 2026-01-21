import { Button as MuiButton, ButtonProps as MuiButtonProps } from '@mui/material';
import { ReactNode } from 'react';
import { useTranslate } from '@locales';

type Props = Omit<MuiButtonProps, 'children'> & {
  label: ReactNode;
};

const BaseButton = ({ label, ...otherProps }: Props) => {
  const { t } = useTranslate();
  if (typeof label === 'string') {
    return <MuiButton {...otherProps}>{t(label)}</MuiButton>;
  }

  return <MuiButton {...otherProps}>{label}</MuiButton>;
};

export default BaseButton;
