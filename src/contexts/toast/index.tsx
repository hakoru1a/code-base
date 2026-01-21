import { createContext, ReactNode } from 'react';
import { enqueueSnackbar, OptionsWithExtraProps } from 'notistack';
import { locales, useTranslate } from '@locales';

export type ToastContextType = {
  success: (message: string) => void;
  error: (message: string) => void;
};

const DEFAULT_TOAST_SETTING = {
  autoHideDuration: 3000,
  disableWindowBlurListener: true,
  anchorOrigin: { horizontal: 'right', vertical: 'top' }
} as OptionsWithExtraProps<Dynamic>;

export const ToastContext = createContext<ToastContextType | null>(null);

export const ToastProvider = ({ children }: { children: ReactNode }) => {
  const { t } = useTranslate();
  const handleShowSuccess = (message: string) => {
    enqueueSnackbar(t(message || locales.notifications.submitSuccess), {
      ...DEFAULT_TOAST_SETTING,
      variant: 'success'
    });
  };

  const handleShowError = (message: string) => {
    enqueueSnackbar(t(message || locales.notifications.submitError), {
      ...DEFAULT_TOAST_SETTING,
      variant: 'error'
    });
  };

  return (
    <ToastContext.Provider
      value={{
        success: handleShowSuccess,
        error: handleShowError
      }}
    >
      {children}
    </ToastContext.Provider>
  );
};
