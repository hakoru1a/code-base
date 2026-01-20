import type { AlertProps, SnackbarOrigin } from '@mui/material';

export type SnackbarProps = {
  action: boolean;
  open: boolean;
  message: string;
  anchorOrigin: SnackbarOrigin;
  variant: string;
  alert: AlertProps;
  transition: string;
  close: boolean;
  actionButton: boolean;
  dense: boolean;
  maxStack: number;
  iconVariant: string;
  hideIconVariant: boolean;
};
