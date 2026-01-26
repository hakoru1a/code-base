import { Field } from '@components';
import { Box, Button, Popover, Stack, Typography } from '@mui/material';

import { WeighTicketType } from '../../constants';
import type { WeighTicketAdvancedSearch } from '../../types';

type Props = {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  value: WeighTicketAdvancedSearch;
  onChange: (next: WeighTicketAdvancedSearch) => void;
  onApply: () => void;
  onReset: () => void;
};

const ticketTypeOptions = [
  { label: 'Phiếu nhập', value: WeighTicketType.In },
  { label: 'Phiếu xuất', value: WeighTicketType.Out }
];

const AdvancedSearchPopover = ({ anchorEl, onClose, value, onChange, onApply, onReset }: Props) => {
  return (
    <Popover
      open={!!anchorEl}
      anchorEl={anchorEl}
      onClose={onClose}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      transformOrigin={{ vertical: 'top', horizontal: 'right' }}
    >
      <Box sx={{ p: 2, width: 420 }}>
        <Stack spacing={2}>
          <Typography variant="h6">Tìm kiếm nâng cao</Typography>

          <Field.Select
            fullWidth
            label="Loại phiếu"
            value={value.ticketType ?? ''}
            onChange={(v) => onChange({ ...value, ticketType: (String(v) as WeighTicketType) || null })}
            defaultOptionLabel="Tất cả"
            options={ticketTypeOptions}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <Field.DatePicker
              fullWidth
              label="Ngày cân vào (từ)"
              value={value.weighInFrom ?? null}
              onChange={(v) => onChange({ ...value, weighInFrom: v })}
              slotProps={{ textField: { size: 'small' } }}
            />
            <Field.DatePicker
              fullWidth
              label="Ngày cân vào (đến)"
              value={value.weighInTo ?? null}
              onChange={(v) => onChange({ ...value, weighInTo: v })}
              slotProps={{ textField: { size: 'small' } }}
            />
          </Stack>

          <Stack direction="row" spacing={1} justifyContent="flex-end">
            <Button variant="outlined" color="inherit" onClick={onReset}>
              Reset
            </Button>
            <Button variant="contained" onClick={onApply}>
              Tìm
            </Button>
          </Stack>
        </Stack>
      </Box>
    </Popover>
  );
};

export default AdvancedSearchPopover;
