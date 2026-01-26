import { Breadcrumbs, Field, MainCard } from '@components';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import { Box, Button, Chip, Grid, Stack, Typography } from '@mui/material';
import { useMemo, useState } from 'react';

import { mockActiveVehicles, mockProductionLines } from '../mock';
import type { ActiveVehicle, ProductionLineCard } from '../types';

const calcDurationText = (startedAt: ProductionLineCard['startedAt']) => {
  if (!startedAt) return '-';
  const minutes = Math.max(
    0,
    Math.floor((dateHelper.formatTimestamp(dateHelper.now()) as number) - (dateHelper.formatTimestamp(startedAt) as number)) / 60000
  );
  if (minutes < 60) return `${minutes} phút`;
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return `${hours}h ${rest}m`;
};

const AssignVehiclesPage = () => {
  const [selected, setSelected] = useState<ActiveVehicle | null>(mockActiveVehicles[0] ?? null);
  const [search, setSearch] = useState<string>('');
  const [pendingLineId, setPendingLineId] = useState<string | null>(null);
  const [lines, setLines] = useState<ProductionLineCard[]>(mockProductionLines);

  const assignedTicketIds = useMemo(() => new Set(lines.filter((l) => !!l.ticketId).map((l) => String(l.ticketId))), [lines]);

  const vehicles = useMemo(
    () =>
      mockActiveVehicles
        .filter((v) => {
          const q = search.trim().toLowerCase();
          if (!q) return true;
          return `${v.vehiclePlate} ${v.ticketCode}`.toLowerCase().includes(q);
        })
        .map((v) => ({
          ...v,
          isAssigned: assignedTicketIds.has(v.ticketId)
        })),
    [assignedTicketIds, search]
  );

  const removeFromLine = (lineId: string) => {
    setLines((prev) => prev.map((l) => (l.id === lineId ? { ...l, ticketId: null, ticketCode: null, startedAt: null, endedAt: null } : l)));
  };

  const confirmAssign = () => {
    if (!selected || !pendingLineId) return;

    setLines((prev) => {
      // remove selected from any line
      const cleared = prev.map((l) => (l.ticketId === selected.ticketId ? { ...l, ticketId: null, ticketCode: null, startedAt: null } : l));
      return cleared.map((l) => {
        if (l.id !== pendingLineId) return l;
        return { ...l, ticketId: selected.ticketId, ticketCode: selected.ticketCode, startedAt: dateHelper.now().format() };
      });
    });
    setPendingLineId(null);
  };

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.productionAssignments', to: routes.dashboard.productionAssignments.vehicles },
    { title: 'breadcrumbs.assignVehicles' }
  ];

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.assignVehicles" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 4 }}>
          <MainCard title="Xe đang active (còn trong xưởng)">
            <Stack spacing={1} sx={{ maxHeight: '70vh', overflow: 'auto', pr: 1 }}>
              <Field.Text
                fullWidth
                value={search}
                onChange={(v) => setSearch(String(v))}
                placeholder="Tìm theo biển số / mã phiếu cân..."
              />
              {vehicles.map((v) => {
                const isSelected = selected?.ticketId === v.ticketId;
                return (
                  <Box
                    key={v.ticketId}
                    onClick={() => setSelected(v)}
                    sx={(theme) => ({
                      p: 1.25,
                      borderRadius: 1,
                      border: `1px solid ${isSelected ? theme.palette.success.main : theme.palette.divider}`,
                      cursor: 'pointer',
                      bgcolor: isSelected ? theme.palette.success.lighter : 'transparent'
                    })}
                  >
                    <Stack spacing={0.5}>
                      <Stack direction="row" spacing={1} alignItems="center" justifyContent="space-between">
                        <Typography fontWeight={700}>{v.vehiclePlate}</Typography>
                        <Chip
                          size="small"
                          label={v.isAssigned ? 'Đã phân công' : 'Chưa phân công'}
                          color={v.isAssigned ? 'success' : 'default'}
                          variant={v.isAssigned ? 'filled' : 'outlined'}
                        />
                      </Stack>
                      <Typography variant="caption" color="text.secondary">
                        Giờ vào xưởng: {dateHelper.formatDateTime(v.enteredFactoryAt)}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Mã phiếu cân: {v.ticketCode}
                      </Typography>
                    </Stack>
                  </Box>
                );
              })}
              {!vehicles.length && <Typography variant="body2">Không có xe active.</Typography>}
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
                      Chọn 1 xe → click 1 chuyền để chọn đích → bấm <b>Confirm</b> để phân công (mock).
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
                const warn = isActive && l.startedAt ? (dateHelper.formatToNow(l.startedAt) ?? '') : '';
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
                        ...(isActive ? { borderColor: theme.palette.success.light } : {}),
                        ...(isPending ? { outline: `2px solid ${theme.palette.primary.main}`, outlineOffset: 2 } : {})
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

                        <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
                          <Typography variant="body2">Phiếu cân:</Typography>
                          {l.ticketCode ? (
                            <Chip
                              size="small"
                              label={l.ticketCode}
                              color="success"
                              variant="outlined"
                              onDelete={() => removeFromLine(l.id)}
                            />
                          ) : (
                            <Typography variant="body2">
                              <b>-</b>
                            </Typography>
                          )}
                        </Stack>

                        <Typography variant="caption" color="text.secondary">
                          Bắt đầu: {l.startedAt ? dateHelper.formatDateTime(l.startedAt) : '-'} • Tổng thời gian:{' '}
                          {calcDurationText(l.startedAt)}
                        </Typography>

                        {isActive && (
                          <Stack direction="row" spacing={1} alignItems="center">
                            <Chip size="small" color="warning" variant="outlined" label="Warning (mock)" />
                            <Typography variant="caption" color="text.secondary">
                              Đang chạy {warn}
                            </Typography>
                          </Stack>
                        )}
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

export default AssignVehiclesPage;
