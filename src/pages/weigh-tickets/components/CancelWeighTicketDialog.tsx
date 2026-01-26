import { CustomDialog, Field } from '@components';
import type { DialogRequest } from '@components';
import { Button, Stack, Typography } from '@mui/material';
import { useState } from 'react';

export type CancelWeighTicketDialogPayload = {
  reason: string;
};

export type CancelWeighTicketDialogProps = DialogRequest & {
  ticketCode: string;
};

const CancelWeighTicketDialog = ({ visible, onClose, ticketCode }: CancelWeighTicketDialogProps) => {
  const [reason, setReason] = useState<string>('');

  return (
    <CustomDialog
      visible={visible}
      onClose={() => onClose?.({ success: false })}
      title="Hủy phiếu cân"
      maxWidth="sm"
      action={
        <Stack direction="row" spacing={2} width="100%" justifyContent="flex-end">
          <Button variant="outlined" color="inherit" onClick={() => onClose?.({ success: false })}>
            Đóng
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={() =>
              onClose?.({
                success: true,
                payload: { reason: reason.trim() } satisfies CancelWeighTicketDialogPayload
              })
            }
            disabled={!reason.trim()}
          >
            Xác nhận hủy
          </Button>
        </Stack>
      }
    >
      <Stack spacing={2}>
        <Typography variant="body2">
          Bạn đang hủy <b>{ticketCode}</b>. Vui lòng nhập lý do hủy để ghi nhận.
        </Typography>
        <Field.Rich
          value={reason}
          onChange={(e) => setReason(String(e.target.value))}
          placeholder="Nhập lý do hủy..."
          maxLength={300}
          rows={5}
        />
      </Stack>
    </CustomDialog>
  );
};

export default CancelWeighTicketDialog;
