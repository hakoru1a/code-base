import { Field } from '@components';
import { Box, Button, Popover, Stack, Typography } from '@mui/material';

import type { SalesContractAdvancedSearch } from '../../types';

type Props = {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  value: SalesContractAdvancedSearch;
  onChange: (next: SalesContractAdvancedSearch) => void;
  onApply: () => void;
  onReset: () => void;
};

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

          <Field.Text
            fullWidth
            label="Khách hàng"
            value={value.customerName ?? ''}
            onChange={(v) => onChange({ ...value, customerName: String(v) })}
          />
          <Field.Text
            fullWidth
            label="Tên hàng hóa"
            value={value.productName ?? ''}
            onChange={(v) => onChange({ ...value, productName: String(v) })}
          />
          <Field.Number
            fullWidth
            label="Giá bán"
            value={value.salePrice ?? ''}
            onChange={(v) => onChange({ ...value, salePrice: String(v) })}
            placeholder="Nhập đúng giá bán để lọc (mock)..."
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <Field.DatePicker
              fullWidth
              label="Ngày hợp đồng (từ)"
              value={value.signedFrom ?? null}
              onChange={(v: Date) => onChange({ ...value, signedFrom: v })}
              slotProps={{ textField: { size: 'small' } }}
            />
            <Field.DatePicker
              fullWidth
              label="Ngày hợp đồng (đến)"
              value={value.signedTo ?? null}
              onChange={(v: Date) => onChange({ ...value, signedTo: v })}
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
