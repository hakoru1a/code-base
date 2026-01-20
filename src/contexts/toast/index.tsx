import { createContext, ReactNode } from 'react';
import { enqueueSnackbar } from 'notistack';

export type ToastContextType = {
  success: (message: string) => void;
  error: (message: string) => void;
};

export const ToastContext = createContext<ToastContextType | null>(null);

export const ToastProvider = ({ children }: { children: ReactNode }) => {
  const handleShowSuccess = (message: string) => {
    enqueueSnackbar(message, {
      variant: 'success',
      autoHideDuration: 3000,
      disableWindowBlurListener: true,
      anchorOrigin: { horizontal: 'right', vertical: 'top' }
    });
  };

  const handleShowError = (message: string) => {
    enqueueSnackbar(message, {
      variant: 'error',
      autoHideDuration: 3000,
      disableWindowBlurListener: true,
      anchorOrigin: { horizontal: 'right', vertical: 'top' }
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
