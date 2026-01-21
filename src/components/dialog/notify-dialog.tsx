import { Button, Stack, Typography } from '@mui/material';
import CustomDialog from './custom-dialog';
import { DialogRequest, NotifyDialogProps } from './types';
import { locales, useTranslate } from '@locales';

type Props = DialogRequest & NotifyDialogProps;

const NotifyDialog = (props: Props) => {
  const { t } = useTranslate();
  const {
    description,
    label: { accept, cancel } = {},
    onAccept,
    onCancel,
    slots,
    slotProps = {
      accept: { show: true },
      cancel: { show: true }
    },
    ...otherProps
  } = props;

  return (
    <CustomDialog
      {...otherProps}
      slotProps={{
        root: {
          paper: { sx: { width: { xs: 'auto', sm: 480 } } }
        },
        close: {
          hiddenBtnClose: true
        },
        content: {
          sx: {
            py: 0
          }
        }
      }}
      action={
        <>
          {slots?.action || (
            <Stack direction="row" justifyContent="start" justifyItems="center" spacing={1.5}>
              {slotProps?.cancel?.show && (
                <Button
                  {...slotProps?.cancel}
                  onClick={() => {
                    onCancel?.();
                    otherProps.onClose?.();
                  }}
                >
                  {t(cancel || locales.common.button.cancel)}
                </Button>
              )}
              {slotProps?.accept?.show && (
                <Button
                  {...slotProps?.accept}
                  variant="contained"
                  color="primary"
                  onClick={() => {
                    onAccept?.();
                    otherProps.onClose?.({ success: true });
                  }}
                >
                  {t(accept || locales.common.button.accept)}
                </Button>
              )}
            </Stack>
          )}
        </>
      }
    >
      <Typography fontWeight={500} fontSize={14} lineHeight="20px">
        {t(description || '')}
      </Typography>
    </CustomDialog>
  );
};

export default NotifyDialog;
