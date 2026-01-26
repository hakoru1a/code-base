import { Breadcrumbs, DataTable, MainCard, TableFetchParams, TabFilter, Field } from '@components';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import { Box, Button, Divider, Grid, IconButton, Stack, Tooltip } from '@mui/material';
import { useCallback, useMemo, useState } from 'react';
import { FilterOutlined, ExportOutlined } from '@ant-design/icons';
import type { PagedResult } from '@services/core';
import { useNavigate } from 'react-router-dom';

import { WeighTicketStatus } from '../constants';
import type { WeighTicket, WeighTicketAdvancedSearch } from '../types';
import { mockWeighTickets } from '../mock';
import AdvancedSearchPopover from './components/AdvancedSearchPopover';
import { buildWeighTicketColumns } from './columns';
import CancelWeighTicketDialog, { type CancelWeighTicketDialogPayload } from '../components/CancelWeighTicketDialog';

const initialAdvancedSearch: WeighTicketAdvancedSearch = {};

const WeighTicketListPage = () => {
  const navigate = useNavigate();

  const [tickets, setTickets] = useState<WeighTicket[]>(mockWeighTickets);
  const [cancelTarget, setCancelTarget] = useState<{ id: string; ticketCode: string } | null>(null);

  const [tab, setTab] = useState<WeighTicketStatus>(WeighTicketStatus.All);
  const [ticketCodeSearch, setTicketCodeSearch] = useState<string>('');

  const [advancedApplied, setAdvancedApplied] = useState<WeighTicketAdvancedSearch>(initialAdvancedSearch);
  const [advancedDraft, setAdvancedDraft] = useState<WeighTicketAdvancedSearch>(initialAdvancedSearch);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const [data, setData] = useState<PagedResult<WeighTicket>>({
    items: [],
    pagination: {
      totalCount: tickets.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const filteredForCounts = useMemo(() => {
    const code = ticketCodeSearch.trim().toLowerCase();
    return tickets.filter((t) => {
      if (code && !t.ticketCode.toLowerCase().includes(code)) return false;

      if (advancedApplied.ticketType && t.ticketType !== advancedApplied.ticketType) return false;

      const weighInAt = t.weighings?.[0]?.weighInAt ?? t.createdAt;
      if (advancedApplied.weighInFrom && advancedApplied.weighInTo) {
        if (!dateHelper.formatIsBetween(weighInAt, advancedApplied.weighInFrom, advancedApplied.weighInTo)) return false;
      } else if (advancedApplied.weighInFrom) {
        if (dateHelper.formatIsAfter(advancedApplied.weighInFrom, weighInAt)) return false;
      } else if (advancedApplied.weighInTo) {
        if (dateHelper.formatIsAfter(weighInAt, advancedApplied.weighInTo)) return false;
      }

      return true;
    });
  }, [advancedApplied, ticketCodeSearch, tickets]);

  const counts = useMemo(() => {
    const active = filteredForCounts.filter((t) => t.status === WeighTicketStatus.Active).length;
    const completed = filteredForCounts.filter((t) => t.status === WeighTicketStatus.Completed).length;
    const cancelled = filteredForCounts.filter((t) => t.status === WeighTicketStatus.Cancelled).length;
    return {
      all: filteredForCounts.length,
      active,
      completed,
      cancelled
    };
  }, [filteredForCounts]);

  const getTabFiltered = useCallback(
    () => (tab === WeighTicketStatus.All ? filteredForCounts : filteredForCounts.filter((t) => t.status === tab)),
    [filteredForCounts, tab]
  );

  const handleFetchData = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<WeighTicket>>((resolve) => {
        setTimeout(() => {
          const tabFiltered = getTabFiltered();

          const currentPage = params.page + 1;
          const totalCount = tabFiltered.length;
          const pageSize = params.pageSize;
          const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

          const startIndex = params.page * pageSize;
          const endIndex = startIndex + pageSize;
          const paginatedItems = tabFiltered.slice(startIndex, endIndex);

          resolve({
            items: paginatedItems,
            pagination: {
              totalCount,
              currentPage,
              pageSize,
              totalPages,
              hasNext: currentPage < totalPages,
              hasPrevious: currentPage > 1
            }
          });
        }, 150);
      });

      const data = await result;
      setData(data);
    },
    [getTabFiltered]
  );

  const columns = useMemo(
    () =>
      buildWeighTicketColumns({
        onView: (id) => navigate(routes.dashboard.weighTicket.detail.replace(':id', id)),
        onQc: (id) => navigate(routes.dashboard.weighTicket.qc.replace(':id', id)),
        onCancel: (payload) => setCancelTarget(payload)
      }),
    [navigate]
  );

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.weighTickets', to: routes.dashboard.weighTickets }
  ];

  const exportCsv = () => {
    const rows = getTabFiltered().map((t) => {
      const last = t.weighings[t.weighings.length - 1];
      return {
        ticketCode: t.ticketCode,
        vehiclePlate: t.vehicleGoods.vehiclePlate,
        customerName: t.customer.customerName,
        payableWeightKg: last?.payableWeightKg ?? 0,
        status: t.status
      };
    });

    const header = ['Mã phiếu cân', 'Biển số xe', 'Tên khách hàng', 'Thanh toán (KG)', 'Trạng thái'];
    const csv = [
      header.join(','),
      ...rows.map((r) =>
        [r.ticketCode, r.vehiclePlate, r.customerName, String(r.payableWeightKg), r.status]
          .map((x) => `"${String(x).replaceAll('"', '""')}"`)
          .join(',')
      )
    ].join('\n');

    const blob = new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `phieu-can_${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.weighTickets" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <Stack spacing={2}>
              <TabFilter
                value={tab}
                onChange={setTab}
                options={[
                  { label: 'Tất cả', value: WeighTicketStatus.All, count: counts.all, color: 'primary' },
                  { label: 'Đang trong xưởng', value: WeighTicketStatus.Active, count: counts.active, color: 'success' },
                  { label: 'Hoàn tất', value: WeighTicketStatus.Completed, count: counts.completed, color: 'info' },
                  { label: 'Đã hủy', value: WeighTicketStatus.Cancelled, count: counts.cancelled, color: 'error' }
                ]}
              />

              <Divider />

              <Stack direction={{ xs: 'column', md: 'row' }} alignItems="center" justifyContent="space-between">
                <Box sx={{ flex: 1, maxWidth: { md: 520 } }}>
                  <Field.Text
                    fullWidth
                    value={ticketCodeSearch}
                    onChange={(val) => setTicketCodeSearch(String(val))}
                    placeholder="Tìm theo mã phiếu cân..."
                  />
                </Box>

                <Stack direction="row" spacing={1} alignItems="center" justifyContent="flex-end">
                  <Button
                    variant="outlined"
                    startIcon={<FilterOutlined />}
                    onClick={(e) => {
                      setAnchorEl(e.currentTarget);
                      setAdvancedDraft(advancedApplied);
                    }}
                  >
                    Tìm nâng cao
                  </Button>

                  <Tooltip title="Export theo bộ lọc hiện tại">
                    <IconButton color="primary" onClick={exportCsv}>
                      <ExportOutlined />
                    </IconButton>
                  </Tooltip>
                </Stack>
              </Stack>
            </Stack>
          </MainCard>
        </Grid>

        <Grid size={12}>
          <MainCard>
            <DataTable data={data.items} columns={columns} totalPage={data.pagination?.totalPages || 1} onLoad={handleFetchData} />
          </MainCard>
        </Grid>
      </Grid>

      <AdvancedSearchPopover
        anchorEl={anchorEl}
        onClose={() => setAnchorEl(null)}
        value={advancedDraft}
        onChange={setAdvancedDraft}
        onApply={() => {
          setAdvancedApplied(advancedDraft);
          setAnchorEl(null);
        }}
        onReset={() => {
          setAdvancedDraft(initialAdvancedSearch);
          setAdvancedApplied(initialAdvancedSearch);
          setAnchorEl(null);
        }}
      />

      {cancelTarget && (
        <CancelWeighTicketDialog
          visible={!!cancelTarget}
          ticketCode={cancelTarget.ticketCode}
          onClose={(result) => {
            const success = result?.success;
            if (success) {
              const payload = result?.payload as CancelWeighTicketDialogPayload;
              setTickets((prev) => prev.map((t) => (t.id === cancelTarget.id ? { ...t, status: WeighTicketStatus.Cancelled } : t)));
              // payload.reason is available if you want to store/display it later (mock)
              void payload;
            }
            setCancelTarget(null);
          }}
        />
      )}
    </>
  );
};

export default WeighTicketListPage;
