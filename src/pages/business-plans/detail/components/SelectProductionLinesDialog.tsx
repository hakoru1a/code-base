import { CustomDialog, Field } from '@components';
import type { DialogRequest } from '@components';
import { Box, Button, Divider, Stack, Typography } from '@mui/material';
import { useMemo, useState } from 'react';

import { mockProductionLines } from '../../mock';
import type { ProductionLineMaster } from '../../types';

export type SelectProductionLinesDialogPayload = {
  lines: ProductionLineMaster[];
};

export type SelectProductionLinesDialogProps = DialogRequest & {
  title?: string;
  defaultSelectedIds?: string[];
};

const SelectProductionLinesDialog = ({ visible, onClose, title, defaultSelectedIds }: SelectProductionLinesDialogProps) => {
  const [search, setSearch] = useState<string>('');
  const [selectedIds, setSelectedIds] = useState<string[]>(defaultSelectedIds ?? []);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return mockProductionLines;
    return mockProductionLines.filter((l) => `${l.code} ${l.name}`.toLowerCase().includes(q));
  }, [search]);

  const toggle = (id: string) => {
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  };

  const selectedLines = useMemo(() => mockProductionLines.filter((l) => selectedIds.includes(l.id)), [selectedIds]);

  return (
    <CustomDialog
      visible={visible}
      onClose={() => onClose?.({ success: false })}
      title={title ?? 'Chọn chuyền sản xuất'}
      maxWidth="sm"
      action={
        <Stack direction="row" spacing={2} width="100%" justifyContent="flex-end">
          <Button variant="outlined" color="inherit" onClick={() => onClose?.({ success: false })}>
            Hủy
          </Button>
          <Button
            variant="contained"
            onClick={() =>
              onClose?.({
                success: true,
                payload: { lines: selectedLines } satisfies SelectProductionLinesDialogPayload
              })
            }
          >
            Chọn ({selectedLines.length})
          </Button>
        </Stack>
      }
    >
      <Stack spacing={2}>
        <Field.Text
          fullWidth
          label="Tìm chuyền"
          value={search}
          onChange={(v) => setSearch(String(v))}
          placeholder="Nhập mã/tên chuyền..."
        />

        <Divider />

        <Stack spacing={1}>
          {filtered.map((l) => {
            const checked = selectedIds.includes(l.id);
            return (
              <Box
                key={l.id}
                sx={(theme) => ({
                  p: 1,
                  borderRadius: 1,
                  border: `1px solid ${theme.palette.divider}`,
                  cursor: 'pointer',
                  ...(checked ? { bgcolor: theme.palette.action.hover } : {})
                })}
                onClick={() => toggle(l.id)}
              >
                <Stack spacing={0.5}>
                  <Stack direction="row" spacing={1} alignItems="baseline" justifyContent="space-between">
                    <Typography fontWeight={700}>
                      {l.code} - {l.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {checked ? 'Đã chọn' : 'Chưa chọn'}
                    </Typography>
                  </Stack>
                  <Typography variant="caption" color="text.secondary">
                    Công suất: {l.capacityTonPerDay} tấn/ngày • Hiệu năng: {(l.efficiency * 100).toFixed(0)}% • Motor: {l.motors} • Nhân sự:{' '}
                    {l.staff}
                  </Typography>
                </Stack>
              </Box>
            );
          })}
        </Stack>
      </Stack>
    </CustomDialog>
  );
};

export default SelectProductionLinesDialog;
