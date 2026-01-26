import { Breadcrumbs, DataTable, Field, MainCard, TabFilter, TableFetchParams } from '@components';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import { Divider, Grid, Stack } from '@mui/material';
import type { PagedResult } from '@services/core';
import { useCallback, useMemo, useState } from 'react';

import { ProductionAssignmentLineStatus } from '../constants';
import { mockLineStaff, mockProductionLines } from '../mock';
import { mockWeighTickets } from '@pages/weigh-tickets/mock';
import LineStaffExpand from './components/LineStaffExpand';
import { buildProductionAssignmentColumns, type ProductionLineRow } from './columns';

const WARNING_MINUTES = 120; // mock threshold

const ProductionAssignmentListPage = () => {
  const [tab, setTab] = useState<ProductionAssignmentLineStatus>(ProductionAssignmentLineStatus.All);
  const [search, setSearch] = useState<string>('');

  const lineRows = useMemo<ProductionLineRow[]>(() => {
    const byTicketId = new Map(mockWeighTickets.map((t) => [t.id, t]));
    return mockProductionLines.map((l) => {
      const ticket = l.ticketId ? byTicketId.get(String(l.ticketId)) : null;
      const vehiclePlate = ticket?.vehicleGoods?.vehiclePlate ?? null;
      const startedAt = (l.startedAt as string) ?? null;

      const minutes = startedAt ? Math.max(0, Math.floor(dateHelper.now().diff(dateHelper.from(startedAt), 'minute'))) : 0;
      const isRunning = !!l.ticketId;
      const status = !isRunning
        ? ProductionAssignmentLineStatus.Idle
        : minutes >= WARNING_MINUTES
          ? ProductionAssignmentLineStatus.Warning
          : ProductionAssignmentLineStatus.Running;

      const staffCount = mockLineStaff.find((x) => x.lineId === l.id)?.staffIds?.length ?? 0;
      const note =
        status === ProductionAssignmentLineStatus.Warning
          ? [
              minutes >= WARNING_MINUTES ? `Quá thời gian: ${minutes} phút` : null,
              staffCount === 0 ? 'Chưa có nhân viên' : staffCount < 2 ? `Thiếu nhân sự (hiện có: ${staffCount})` : null
            ]
              .filter(Boolean)
              .join(' • ')
          : '-';

      return {
        id: l.id,
        lineNo: l.lineNo,
        vehiclePlate,
        startedAt,
        status,
        note
      };
    });
  }, []);

  const filteredForCounts = useMemo(() => {
    const q = search.trim().toLowerCase();
    return lineRows.filter((r) => {
      if (!q) return true;
      return `${r.lineNo} ${r.vehiclePlate ?? ''}`.toLowerCase().includes(q);
    });
  }, [lineRows, search]);

  const counts = useMemo(() => {
    const running = filteredForCounts.filter((x) => x.status === ProductionAssignmentLineStatus.Running).length;
    const warning = filteredForCounts.filter((x) => x.status === ProductionAssignmentLineStatus.Warning).length;
    const idle = filteredForCounts.filter((x) => x.status === ProductionAssignmentLineStatus.Idle).length;
    return { all: filteredForCounts.length, running, warning, idle };
  }, [filteredForCounts]);

  const getTabFiltered = useCallback(
    () => (tab === ProductionAssignmentLineStatus.All ? filteredForCounts : filteredForCounts.filter((x) => x.status === tab)),
    [filteredForCounts, tab]
  );

  const [data, setData] = useState<PagedResult<ProductionLineRow>>({
    items: [],
    pagination: {
      totalCount: lineRows.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const handleFetchData = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<ProductionLineRow>>((resolve) => {
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
        }, 120);
      });

      setData(await result);
    },
    [getTabFiltered]
  );

  const columns = useMemo(() => buildProductionAssignmentColumns(), []);

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.productionAssignments', to: routes.dashboard.productionAssignments.list }
  ];

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.productionAssignments" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <Stack spacing={2}>
              <TabFilter
                value={tab}
                onChange={setTab}
                options={[
                  { label: 'Tất cả', value: ProductionAssignmentLineStatus.All, count: counts.all, color: 'primary' },
                  { label: 'Đang chạy', value: ProductionAssignmentLineStatus.Running, count: counts.running, color: 'success' },
                  { label: 'Cảnh báo', value: ProductionAssignmentLineStatus.Warning, count: counts.warning, color: 'warning' },
                  { label: 'Trống', value: ProductionAssignmentLineStatus.Idle, count: counts.idle, color: 'info' }
                ]}
              />

              <Divider />

              <Field.Text fullWidth value={search} onChange={(v) => setSearch(String(v))} placeholder="Tìm theo số chuyền / biển số..." />
            </Stack>
          </MainCard>
        </Grid>

        <Grid size={12}>
          <MainCard>
            <DataTable
              data={data.items}
              columns={columns}
              totalPage={data.pagination?.totalPages || 1}
              onLoad={handleFetchData}
              slots={{
                expand: (row) => <LineStaffExpand lineId={(row.original as ProductionLineRow).id} />
              }}
            />
          </MainCard>
        </Grid>
      </Grid>
    </>
  );
};

export default ProductionAssignmentListPage;
