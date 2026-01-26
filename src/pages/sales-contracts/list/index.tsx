import { Breadcrumbs, DataTable, MainCard, TableFetchParams, TabFilter, Field } from '@components';
import { useDialog } from '@hooks';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import { Box, Button, Divider, Grid, Stack } from '@mui/material';
import { useCallback, useMemo, useState } from 'react';
import { FilterOutlined, PlusOutlined } from '@ant-design/icons';
import type { PagedResult } from '@services/core';
import { useNavigate } from 'react-router-dom';

import { SalesContractStatus } from '../constants';
import type { SalesContract, SalesContractAdvancedSearch } from '../types';
import { mockSalesContracts } from '../mock';
import AdvancedSearchPopover from './components/AdvancedSearchPopover';
import { buildSalesContractColumns } from './columns';

const initialAdvancedSearch: SalesContractAdvancedSearch = {};

const SalesContractListPage = () => {
  const dialog = useDialog();
  const navigate = useNavigate();

  const [contracts, setContracts] = useState<SalesContract[]>(mockSalesContracts);

  const [tab, setTab] = useState<SalesContractStatus>(SalesContractStatus.All);
  const [contractCodeSearch, setContractCodeSearch] = useState<string>('');

  const [advancedApplied, setAdvancedApplied] = useState<SalesContractAdvancedSearch>(initialAdvancedSearch);
  const [advancedDraft, setAdvancedDraft] = useState<SalesContractAdvancedSearch>(initialAdvancedSearch);

  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const [data, setData] = useState<PagedResult<SalesContract>>({
    items: [],
    pagination: {
      totalCount: contracts.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const filteredForCounts = useMemo(() => {
    const code = contractCodeSearch.trim().toLowerCase();
    return contracts.filter((c) => {
      if (code && !c.contractCode.toLowerCase().includes(code)) return false;
      if (advancedApplied.customerName && !c.customerName.toLowerCase().includes(advancedApplied.customerName.toLowerCase())) return false;
      if (advancedApplied.productName && !c.productName.toLowerCase().includes(advancedApplied.productName.toLowerCase())) return false;

      const price = advancedApplied.salePrice ? Number(advancedApplied.salePrice) : null;
      if (price !== null && !Number.isNaN(price) && c.salePrice !== price) return false;

      if (advancedApplied.signedFrom && advancedApplied.signedTo) {
        if (!dateHelper.formatIsBetween(c.signedAt, advancedApplied.signedFrom, advancedApplied.signedTo)) return false;
      } else if (advancedApplied.signedFrom) {
        if (dateHelper.formatIsAfter(advancedApplied.signedFrom, c.signedAt)) return false;
      } else if (advancedApplied.signedTo) {
        if (dateHelper.formatIsAfter(c.signedAt, advancedApplied.signedTo)) return false;
      }

      return true;
    });
  }, [advancedApplied, contractCodeSearch, contracts]);

  const counts = useMemo(() => {
    const active = filteredForCounts.filter((c) => c.status === SalesContractStatus.Active).length;
    const completed = filteredForCounts.filter((c) => c.status === SalesContractStatus.Completed).length;
    const stopped = filteredForCounts.filter((c) => c.status === SalesContractStatus.Stopped).length;
    return {
      all: filteredForCounts.length,
      active,
      completed,
      stopped
    };
  }, [filteredForCounts]);

  const handleFetchData = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<SalesContract>>((resolve) => {
        setTimeout(() => {
          const tabFiltered = tab === SalesContractStatus.All ? filteredForCounts : filteredForCounts.filter((c) => c.status === tab);

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
    [filteredForCounts, tab]
  );

  const columns = useMemo(
    () =>
      buildSalesContractColumns({
        dialog,
        onView: (id) => navigate(routes.dashboard.salesContract.detail.replace(':id', id)),
        onDeleteSuccess: (id) => setContracts((prev) => prev.filter((c) => c.id !== id)),
        onApproveSuccess: (id, payload) =>
          setContracts((prev) =>
            prev.map((c) => (c.id === id ? { ...c, approvalDecision: payload.decision, approvalNote: payload.note } : c))
          )
      }),
    [dialog, navigate]
  );

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.salesContracts', to: routes.dashboard.sales }
  ];

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.salesContracts" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <Stack spacing={2}>
              <TabFilter
                value={tab}
                onChange={setTab}
                options={[
                  { label: 'Tất cả', value: SalesContractStatus.All, count: counts.all, color: 'primary' },
                  { label: 'Hoạt động', value: SalesContractStatus.Active, count: counts.active, color: 'success' },
                  { label: 'Hoàn thành', value: SalesContractStatus.Completed, count: counts.completed, color: 'info' },
                  { label: 'Dừng', value: SalesContractStatus.Stopped, count: counts.stopped, color: 'error' }
                ]}
              />

              <Divider />

              <Stack direction={{ xs: 'column', md: 'row' }} alignItems="center" justifyContent="space-between">
                <Box sx={{ flex: 1, maxWidth: { md: 520 } }}>
                  <Field.Text
                    fullWidth
                    value={contractCodeSearch}
                    onChange={(val) => setContractCodeSearch(String(val))}
                    placeholder="Tìm theo mã hợp đồng..."
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
                      navigate(routes.dashboard.salesContract.create);
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
    </>
  );
};

export default SalesContractListPage;
