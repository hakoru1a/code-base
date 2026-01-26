import { Breadcrumbs, DataTable, Field, MainCard, TableFetchParams, TabFilter } from '@components';
import { useDialog } from '@hooks';
import { routes } from '@routes';
import { Box, Button, Divider, Grid, Stack } from '@mui/material';
import { FilterOutlined, PlusOutlined } from '@ant-design/icons';
import type { PagedResult } from '@services/core';
import { useCallback, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import type { BusinessPlan, BusinessPlanAdvancedSearch } from '../types';
import { mockBusinessPlans } from '../mock';
import AdvancedSearchPopover from './components/AdvancedSearchPopover';
import TimelineExpandPanel from './components/TimelineExpandPanel';
import { buildBusinessPlanColumns } from './columns';
import { BusinessPlanStatus } from '../constants';
import type { ApproveDecision } from '@components';

const initialAdvancedSearch: BusinessPlanAdvancedSearch = {};

const BusinessPlanListPage = () => {
  const dialog = useDialog();
  const navigate = useNavigate();

  const [plans, setPlans] = useState<BusinessPlan[]>(mockBusinessPlans);
  const [tab, setTab] = useState<BusinessPlanStatus>(BusinessPlanStatus.All);
  const [planCodeSearch, setPlanCodeSearch] = useState<string>('');

  const [advancedApplied, setAdvancedApplied] = useState<BusinessPlanAdvancedSearch>(initialAdvancedSearch);
  const [advancedDraft, setAdvancedDraft] = useState<BusinessPlanAdvancedSearch>(initialAdvancedSearch);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const filteredForCounts = useMemo(() => {
    const code = planCodeSearch.trim().toLowerCase();
    return plans.filter((p) => {
      if (code && !p.planCode.toLowerCase().includes(code)) return false;
      if (advancedApplied.salesContractCode && !p.salesContractCode.toLowerCase().includes(advancedApplied.salesContractCode.toLowerCase()))
        return false;

      const from = advancedApplied.outputFrom ? Number(advancedApplied.outputFrom) : null;
      const to = advancedApplied.outputTo ? Number(advancedApplied.outputTo) : null;
      if (from !== null && !Number.isNaN(from) && p.outputTon < from) return false;
      if (to !== null && !Number.isNaN(to) && p.outputTon > to) return false;

      return true;
    });
  }, [advancedApplied, planCodeSearch, plans]);

  const counts = useMemo(() => {
    const active = filteredForCounts.filter((p) => p.status === BusinessPlanStatus.Active).length;
    const completed = filteredForCounts.filter((p) => p.status === BusinessPlanStatus.Completed).length;
    const stopped = filteredForCounts.filter((p) => p.status === BusinessPlanStatus.Stopped).length;
    return {
      all: filteredForCounts.length,
      active,
      completed,
      stopped
    };
  }, [filteredForCounts]);

  const [data, setData] = useState<PagedResult<BusinessPlan>>({
    items: [],
    pagination: {
      totalCount: plans.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const handleFetchData = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<BusinessPlan>>((resolve) => {
        setTimeout(() => {
          const tabFiltered = tab === BusinessPlanStatus.All ? filteredForCounts : filteredForCounts.filter((p) => p.status === tab);

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

      setData(await result);
    },
    [filteredForCounts, tab]
  );

  const columns = useMemo(
    () =>
      buildBusinessPlanColumns({
        dialog,
        onView: (id) => navigate(routes.dashboard.businessPlan.detail.replace(':id', id)),
        onDeleteSuccess: (id) => setPlans((prev) => prev.filter((p) => p.id !== id)),
        onApproveSuccess: (id, payload: { decision: ApproveDecision; note: string }) =>
          setPlans((prev) =>
            prev.map((p) => {
              if (p.id !== id) return p;
              return {
                ...p,
                approvalDecision: payload.decision,
                approvalNote: payload.note,
                status: payload.decision === 'APPROVED' ? BusinessPlanStatus.Active : BusinessPlanStatus.Stopped
              } satisfies BusinessPlan;
            })
          )
      }),
    [dialog, navigate]
  );

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.businessPlans', to: routes.dashboard.businessPlans }
  ];

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.businessPlans" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <Stack spacing={2}>
              <TabFilter
                value={tab}
                onChange={setTab}
                options={[
                  { label: 'Tất cả', value: BusinessPlanStatus.All, count: counts.all, color: 'primary' },
                  { label: 'Hoạt động', value: BusinessPlanStatus.Active, count: counts.active, color: 'warning' },
                  { label: 'Hoàn thành', value: BusinessPlanStatus.Completed, count: counts.completed, color: 'success' },
                  { label: 'Dừng', value: BusinessPlanStatus.Stopped, count: counts.stopped, color: 'error' }
                ]}
              />

              <Divider />

              <Stack direction={{ xs: 'column', md: 'row' }} alignItems="center" justifyContent="space-between">
                <Box sx={{ flex: 1, maxWidth: { md: 520 } }}>
                  <Field.Text
                    fullWidth
                    value={planCodeSearch}
                    onChange={(val) => setPlanCodeSearch(String(val))}
                    placeholder="Tìm theo mã PAKD..."
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
                  <Button
                    variant="contained"
                    startIcon={<PlusOutlined />}
                    onClick={() => {
                      navigate(routes.dashboard.businessPlan.create);
                    }}
                  >
                    Tạo mới
                  </Button>
                </Stack>
              </Stack>
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
                expand: (row) => <TimelineExpandPanel plan={row.original as BusinessPlan} />
              }}
            />
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
    </>
  );
};

export default BusinessPlanListPage;
