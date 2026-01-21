import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';

import CustomDialog from './custom-dialog';
import { TransitionDialog } from './transition-dialog';

import type { ConfirmDialogProps, DialogRequest } from './types';
import { useBoolean } from '@hooks';
import { useMemo } from 'react';
import { Button } from '@mui/material';
import { locales, useTranslate } from '@locales';

type Props = DialogRequest & ConfirmDialogProps;

const ConfirmationDialog = (props: Props) => {
  const { t } = useTranslate();
  const { title, description, onAccept, onCancel, label, slots = {}, slotProps = {}, ...otherProps } = props;

  const submitting = useBoolean();

  const handleCancel = () => {
    onCancel?.();
    otherProps.onClose?.();
  };

  const onSubmit = async () => {
    try {
      submitting.onTrue();

      await onAccept?.();

      otherProps.onClose?.({ success: true });
    } finally {
      submitting.onFalse();
    }
  };

  const disabledAllButtonWhenSubmit = useMemo(
    () => slotProps?.submitting?.disabledAllButton || false,
    [slotProps?.submitting?.disabledAllButton]
  );

  return (
    <CustomDialog
      title={title}
      {...otherProps}
      maxWidth="xs"
      {...slotProps?.root}
      slots={{
        transition: TransitionDialog.Zoom
      }}
      slotProps={{
        content: {
          sx: {
            py: 0
          }
        },
        action: {
          sx: {
            pt: 2
          }
        },
        title: {
          justifyContent: 'center'
        }
      }}
      action={
        <Stack width="100%" direction="row" spacing={2}>
          <Button
            fullWidth
            variant="outlined"
            color="inherit"
            {...slotProps?.cancel}
            onClick={handleCancel}
            disabled={disabledAllButtonWhenSubmit && submitting.value}
          >
            {t(label?.cancel || locales.common.button.reject)}
          </Button>
          <Button
            fullWidth
            color="error"
            variant="contained"
            type="submit"
            {...slotProps?.accept}
            onClick={onSubmit}
            loading={!disabledAllButtonWhenSubmit && submitting.value}
            disabled={disabledAllButtonWhenSubmit && submitting.value}
          >
            {t(label?.accept || locales.common.button.accept)}
          </Button>
        </Stack>
      }
    >
      <Stack spacing={2}>
        {slots?.content && (
          <Typography variant="body2" fontWeight={600}>
            {slots?.content}
          </Typography>
        )}
        {description && (
          <Typography textAlign="center" variant="body2" fontSize={14} fontWeight={500}>
            {description}
          </Typography>
        )}
      </Stack>
    </CustomDialog>
  );
};

export default ConfirmationDialog;
