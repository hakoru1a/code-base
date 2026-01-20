import { Theme } from '@mui/material';
import { ColorProps } from '@themes';

const themeHelper = {
  getColors(theme: Theme, color?: ColorProps) {
    switch (color!) {
      case 'secondary':
        return theme.palette.secondary;
      case 'error':
        return theme.palette.error;
      case 'warning':
        return theme.palette.warning;
      case 'info':
        return theme.palette.info;
      case 'success':
        return theme.palette.success;
      default:
        return theme.palette.primary;
    }
  },
  getImageUrl(name: string, path: string) {
    return new URL(`/src/assets/images/${path}/${name}`, import.meta.url).href;
  },
  getShadow(theme: Theme, shadow: string) {
    switch (shadow) {
      case 'secondary':
        return theme.customShadows.secondary;
      case 'error':
        return theme.customShadows.error;
      case 'warning':
        return theme.customShadows.warning;
      case 'info':
        return theme.customShadows.info;
      case 'success':
        return theme.customShadows.success;
      case 'primaryButton':
        return theme.customShadows.primaryButton;
      case 'secondaryButton':
        return theme.customShadows.secondaryButton;
      case 'errorButton':
        return theme.customShadows.errorButton;
      case 'warningButton':
        return theme.customShadows.warningButton;
      case 'infoButton':
        return theme.customShadows.infoButton;
      case 'successButton':
        return theme.customShadows.successButton;
      default:
        return theme.customShadows.primary;
    }
  }
};

export default themeHelper;
