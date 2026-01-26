import { Field } from '@components';
import { Box, Button, Popover, Stack, Typography } from '@mui/material';

import type { BusinessPlanAdvancedSearch } from '../../types';

type Props = {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  value: BusinessPlanAdvancedSearch;
  onChange: (next: BusinessPlanAdvancedSearch) => void;
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
            label="Mã hợp đồng bán"
            value={value.salesContractCode ?? ''}
            onChange={(v) => onChange({ ...value, salesContractCode: String(v) })}
          />

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <Field.Number
              fullWidth
              label="Output (tấn) từ"
              value={value.outputFrom ?? ''}
              onChange={(v) => onChange({ ...value, outputFrom: String(v) })}
              placeholder="Ví dụ: 100"
            />
            <Field.Number
              fullWidth
              label="Output (tấn) đến"
              value={value.outputTo ?? ''}
              onChange={(v) => onChange({ ...value, outputTo: String(v) })}
              placeholder="Ví dụ: 500"
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
