import DialogActions from '@mui/material/DialogActions';
import { DialogContent, Dialog as BaseDialog, Stack } from '@mui/material';

import type { CustomDialogProps } from './types';

const CustomDialog = ({
  title,
  children,
  visible,
  sx,
  onClose,
  action,
  canClickOutside = true,
  maxWidth = 'xs',
  slots,
  slotProps,
  ...otherProps
}: CustomDialogProps) => {
  return (
    <BaseDialog
      open={visible}
      fullWidth
      maxWidth={maxWidth}
      sx={sx}
      scroll="paper"
      aria-hidden={false}
      slots={slots}
      aria-labelledby={`dialog-title-${title?.toString()}`}
      aria-describedby={`column-description-${title?.toString()}`}
      slotProps={slotProps?.root}
      onClose={(_, reason) => {
        if ((!canClickOutside && reason === 'backdropClick') || slotProps?.close?.disableBtnClose) return;
        onClose();
      }}
      {...otherProps}
    >
      <Stack
        sx={{
          ...slotProps?.title?.sx,
          fontSize: 18,
          lineHeight: '28px',
          fontWeight: 700
        }}
        pb={title ? 3 : 0}
        direction="row"
        alignItems="center"
        {...slotProps?.title}
        {...(!!title && {
          py: '18px',
          px: 3,
          width: { xs: 300, sm: 'auto' }
        })}
      >
        {title}
      </Stack>

      <DialogContent
        {...slotProps?.content}
        sx={{
          p: 3,
          ...(!!title && { pt: 0.5 }),
          ...slotProps?.content?.sx,
          zIndex: 2
        }}
      >
        {children}
      </DialogContent>

      {action && (
        <DialogActions
          {...slotProps?.action}
          sx={{
            zIndex: 1,
            p: 3,
            ...slotProps?.action?.sx
          }}
        >
          {action}
        </DialogActions>
      )}
    </BaseDialog>
  );
};

export default CustomDialog;
