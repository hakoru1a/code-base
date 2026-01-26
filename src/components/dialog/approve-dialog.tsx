import { Button, Stack } from '@mui/material';
import type { DialogRequest } from './types';
import CustomDialog from './custom-dialog';
import { Field } from '@components';
import { useMemo, useState } from 'react';
import { locales, useTranslate } from '@locales';

export enum ApproveDecision {
  Approved = 'APPROVED',
  Rejected = 'REJECTED'
}

export type ApproveDialogPayload = {
  decision: ApproveDecision;
  note: string;
};

export type ApproveDialogProps = DialogRequest & {
  title?: string;
  defaultDecision?: ApproveDecision;
  defaultNote?: string;
};

const ApproveDialog = ({ visible, onClose, title, defaultDecision, defaultNote }: ApproveDialogProps) => {
  const { t } = useTranslate();

  const [decision, setDecision] = useState<ApproveDecision>(defaultDecision ?? ApproveDecision.Approved);
  const [note, setNote] = useState<string>(defaultNote ?? '');

  const canSubmit = useMemo(() => note.trim().length > 0, [note]);

  return (
    <CustomDialog
      visible={visible}
      onClose={() => onClose?.({ success: false })}
      title={title ?? t(locales.common.button.approve)}
      maxWidth="sm"
      action={
        <Stack direction="row" spacing={2} width="100%" justifyContent="flex-end">
          <Button variant="outlined" color="inherit" onClick={() => onClose?.({ success: false })}>
            {t(locales.common.button.cancel)}
          </Button>
          <Button
            variant="contained"
            onClick={() =>
              onClose?.({
                success: true,
                payload: { decision, note } satisfies ApproveDialogPayload
              })
            }
            disabled={!canSubmit}
          >
            {t(locales.common.button.accept)}
          </Button>
        </Stack>
      }
    >
      <Stack spacing={2}>
        <Field.Select
          fullWidth
          label="Trạng thái xét duyệt"
          value={decision}
          options={[
            { label: 'Phê duyệt', value: ApproveDecision.Approved },
            { label: 'Từ chối', value: ApproveDecision.Rejected }
          ]}
          labelOption="label"
          valueOption="value"
          onChange={(val) => setDecision(val as ApproveDecision)}
          renderLabel={(opt: { label: string }) => opt.label}
        />

        <Field.Text
          fullWidth
          label="Ghi chú"
          value={note}
          onChange={(val) => setNote(String(val))}
          multiline
          minRows={3}
          placeholder="Nhập ghi chú xét duyệt..."
        />
      </Stack>
    </CustomDialog>
  );
};

export default ApproveDialog;
