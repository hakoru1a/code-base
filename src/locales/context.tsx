import { IntlProvider } from 'react-intl';
import { ReactNode } from 'react';
import { useConfig } from '../hooks';

type Props = {
  children: ReactNode;
};

export const LocalProvider = ({ children }: Props) => {
  const { i18n } = useConfig();

  return (
    <IntlProvider locale={i18n} defaultLocale="en">
      {children}
    </IntlProvider>
  );
};
