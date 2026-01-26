import { Breadcrumbs, Field, MainCard } from '@components';
import { routes } from '@routes';
import { Box, Button, Chip, Grid, Stack, Typography } from '@mui/material';
import { useMemo, useState } from 'react';

import { mockLineStaff, mockProductionLines, mockStaff } from '../mock';
import type { LineStaffAssignment, ProductionLineCard, StaffCard } from '../types';

const AssignStaffPage = () => {
  const [selected, setSelected] = useState<StaffCard | null>(mockStaff[0] ?? null);
  const [search, setSearch] = useState<string>('');
  const [pendingLineId, setPendingLineId] = useState<string | null>(null);
  const [lines] = useState<ProductionLineCard[]>(mockProductionLines);
  const [lineStaff, setLineStaff] = useState<LineStaffAssignment[]>(mockLineStaff);

  const staffById = useMemo(() => new Map(mockStaff.map((s) => [s.id, s])), []);

  const getAssigned = (lineId: string) => lineStaff.find((x) => x.lineId === lineId)?.staffIds ?? [];
  const isAssignedToLine = (lineId: string, staffId: string) => getAssigned(lineId).includes(staffId);

  const removeStaffFromLine = (lineId: string, staffId: string) => {
    setLineStaff((prev) => {
      const exists = prev.find((x) => x.lineId === lineId);
      if (!exists) return prev;
      const next = exists.staffIds.filter((x) => x !== staffId);
      if (!next.length) return prev.filter((x) => x.lineId !== lineId);
      return prev.map((x) => (x.lineId === lineId ? { ...x, staffIds: next } : x));
    });
  };

  const confirmAssign = () => {
    if (!selected || !pendingLineId) return;
    setLineStaff((prev) => {
      const exists = prev.find((x) => x.lineId === pendingLineId);
      const current = exists?.staffIds ?? [];
      const next = current.includes(selected.id) ? current.filter((x) => x !== selected.id) : [...current, selected.id];
      if (exists) return prev.map((x) => (x.lineId === pendingLineId ? { ...x, staffIds: next } : x));
      return [...prev, { lineId: pendingLineId, staffIds: next }];
    });
    setPendingLineId(null);
  };

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.productionAssignments', to: routes.dashboard.productionAssignments.vehicles },
    { title: 'breadcrumbs.assignStaff' }
  ];

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.assignStaff" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 4 }}>
          <MainCard title="Nhân viên">
            <Stack spacing={1} sx={{ maxHeight: '70vh', overflow: 'auto', pr: 1 }}>
              <Field.Text fullWidth value={search} onChange={(v) => setSearch(String(v))} placeholder="Tìm theo tên / vai trò / ca..." />
              {mockStaff
                .filter((s) => {
                  const q = search.trim().toLowerCase();
                  if (!q) return true;
                  return `${s.name} ${s.role} ${s.shift}`.toLowerCase().includes(q);
                })
                .map((s) => {
                  const isSelected = selected?.id === s.id;
                  return (
                    <Box
                      key={s.id}
                      onClick={() => setSelected(s)}
                      sx={(theme) => ({
                        p: 1.25,
                        borderRadius: 1,
                        border: `1px solid ${isSelected ? theme.palette.success.main : theme.palette.divider}`,
                        cursor: 'pointer',
                        bgcolor: isSelected ? theme.palette.success.lighter : 'transparent'
                      })}
                    >
                      <Stack spacing={0.5}>
                        <Typography fontWeight={700}>{s.name}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          Vai trò: {s.role}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Ca làm: {s.shift}
                        </Typography>
                      </Stack>
                    </Box>
                  );
                })}
            </Stack>
          </MainCard>
        </Grid>

        <Grid size={{ xs: 12, md: 8 }}>
          <MainCard
            title={
              <Stack spacing={0.5}>
                <Stack
                  direction={{ xs: 'column', md: 'row' }}
                  spacing={1}
                  alignItems={{ xs: 'flex-start', md: 'center' }}
                  justifyContent="space-between"
                >
                  <Stack spacing={0.5}>
                    <Typography fontWeight={700}>Chuyền sản xuất (1 → 9)</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Chọn 1 nhân viên → click 1 chuyền để chọn đích → bấm <b>Confirm</b> để gán/bỏ gán (mock).
                    </Typography>
                  </Stack>
                  <Stack direction="row" spacing={1}>
                    <Button variant="outlined" color="inherit" onClick={() => setPendingLineId(null)} disabled={!pendingLineId}>
                      Hủy chọn
                    </Button>
                    <Button variant="contained" onClick={confirmAssign} disabled={!selected || !pendingLineId}>
                      Xác nhận
                    </Button>
                  </Stack>
                </Stack>
              </Stack>
            }
          >
            <Grid container spacing={2}>
              {lines.map((l) => {
                const isActive = !!l.ticketId;
                const assigned = getAssigned(l.id)
                  .map((id) => staffById.get(id))
                  .filter(Boolean) as StaffCard[];
                const selectedAssigned = selected ? isAssignedToLine(l.id, selected.id) : false;
                const isPending = pendingLineId === l.id;

                return (
                  <Grid key={l.id} size={{ xs: 12, sm: 6, md: 4 }}>
                    <Box
                      onClick={() => setPendingLineId(l.id)}
                      sx={(theme) => ({
                        p: 1.5,
                        borderRadius: 1,
                        border: `1px solid ${theme.palette.divider}`,
                        cursor: 'pointer',
                        bgcolor: isActive ? theme.palette.success.lighter : theme.palette.grey[100],
                        ...(selectedAssigned ? { outline: `2px solid ${theme.palette.primary.main}`, outlineOffset: 1 } : {}),
                        ...(isPending ? { boxShadow: `0 0 0 2px ${theme.palette.primary.main}` } : {})
                      })}
                    >
                      <Stack spacing={1}>
                        <Stack direction="row" alignItems="center" justifyContent="space-between">
                          <Typography fontWeight={800}>Chuyền {l.lineNo}</Typography>
                          <Chip
                            size="small"
                            label={isActive ? 'Đang chạy' : 'Trống'}
                            color={isActive ? 'success' : 'default'}
                            variant={isActive ? 'filled' : 'outlined'}
                          />
                        </Stack>

                        <Typography variant="caption" color="text.secondary">
                          Phiếu cân: {l.ticketCode ?? '-'}
                        </Typography>

                        <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
                          {assigned.length ? (
                            assigned.map((s) => (
                              <Chip
                                key={s.id}
                                size="small"
                                label={s.name}
                                variant="outlined"
                                onDelete={() => removeStaffFromLine(l.id, s.id)}
                              />
                            ))
                          ) : (
                            <Typography variant="caption" color="text.secondary">
                              Chưa có nhân viên
                            </Typography>
                          )}
                        </Stack>
                      </Stack>
                    </Box>
                  </Grid>
                );
              })}
            </Grid>
          </MainCard>
        </Grid>
      </Grid>
    </>
  );
};

export default AssignStaffPage;
