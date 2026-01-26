import { CustomDialog } from '@components';
import type { DialogRequest } from '@components';
import { Box, Button, Stack, Typography } from '@mui/material';

export type ViewImageDialogProps = DialogRequest & {
  title: string;
  imageUrl: string;
  description?: string;
};

const ViewImageDialog = ({ visible, onClose, title, imageUrl, description }: ViewImageDialogProps) => {
  return (
    <CustomDialog
      visible={visible}
      onClose={() => onClose?.({ success: false })}
      title={title}
      maxWidth="md"
      action={
        <Stack direction="row" spacing={2} width="100%" justifyContent="flex-end">
          <Button variant="outlined" color="inherit" onClick={() => onClose?.({ success: false })}>
            Đóng
          </Button>
        </Stack>
      }
    >
      <Stack spacing={1.5}>
        {description && (
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        )}
        <Box
          component="img"
          src={imageUrl}
          alt={title}
          sx={(theme) => ({
            width: '100%',
            maxHeight: '70vh',
            objectFit: 'contain',
            borderRadius: 1,
            border: `1px solid ${theme.palette.divider}`
          })}
        />
      </Stack>
    </CustomDialog>
  );
};

export default ViewImageDialog;
