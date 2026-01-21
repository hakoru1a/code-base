import { Link, LinkProps } from '@mui/material';
import { ReactNode } from 'react';
import { useTranslate } from '@locales';

type Props = Omit<LinkProps, 'children'> & {
  label: ReactNode;
};

const TextLink = ({ label, ...otherProps }: Props) => {
  const { t } = useTranslate();
  if (typeof label === 'string') {
    return <Link {...otherProps}>{t(label)}</Link>;
  }

  return <Link {...otherProps}>{label}</Link>;
};

export default TextLink;
