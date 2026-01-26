import { Breadcrumbs, DataTable, MainCard, TableFetchParams, Field } from '@components';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  Stack,
  Tooltip,
  Typography
} from '@mui/material';
import { ColumnDef } from '@tanstack/react-table';
import { useMemo, useState, useCallback } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { DownOutlined, EditOutlined, ArrowLeftOutlined, EyeOutlined } from '@ant-design/icons';
import type { PagedResult } from '@services/core';

import { SalesContractMode } from '../constants';
import { SalesContractStatus } from '../constants';
import { mockBusinessPlansByContractId, mockSalesContracts } from '../mock';
import type { BusinessPlan, SalesContract } from '../types';
import { useToggle } from '@hooks';
import type { DatePickerFormat } from '@utils/helpers';

const formatVnd = (value: number) =>
  new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value);

const getModeFromPath = (pathname: string): SalesContractMode => {
  if (pathname.endsWith('/create')) return SalesContractMode.Create;
  if (pathname.endsWith('/edit')) return SalesContractMode.Update;
  return SalesContractMode.View;
};

const SalesContractDetailPage = () => {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();

  const mode = useMemo(() => getModeFromPath(location.pathname), [location.pathname]);
  const isView = mode === SalesContractMode.View;

  const contractFromMock = useMemo<SalesContract | null>(() => {
    if (mode === SalesContractMode.Create) {
      return {
        id: 'NEW',
        contractCode: '',
        customerName: '',
        productName: '',
        salePrice: 0,
        signedAt: null,
        status: SalesContractStatus.Active,
        approvalDecision: null,
        approvalNote: null
      } as SalesContract;
    }
    return mockSalesContracts.find((c) => c.id === id) ?? null;
  }, [id, mode]);

  const [contract, setContract] = useState<SalesContract | null>(contractFromMock);

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.salesContracts', to: routes.dashboard.sales },
    { title: 'breadcrumbs.details' }
  ];

  const sectionBasic = useToggle(true);
  const sectionPlans = useToggle(true);

  const plans = useMemo<BusinessPlan[]>(() => {
    if (!contract || contract.id === 'NEW') return [];
    const raw = mockBusinessPlansByContractId[contract.id] ?? [];
    const sorted = [...raw].sort(
      (a, b) => Number(dateHelper.formatTimestamp(b.createdAt)) - Number(dateHelper.formatTimestamp(a.createdAt))
    );
    return sorted.map((p, idx) => ({ ...p, active: idx === 0 }));
  }, [contract]);

  const [planData, setPlanData] = useState<PagedResult<BusinessPlan>>({
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

  const handleFetchPlans = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<BusinessPlan>>((resolve) => {
        setTimeout(() => {
          const currentPage = params.page + 1;
          const totalCount = plans.length;
          const pageSize = params.pageSize;
          const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

          const startIndex = params.page * pageSize;
          const endIndex = startIndex + pageSize;
          const paginatedItems = plans.slice(startIndex, endIndex);

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

      setPlanData(await result);
    },
    [plans]
  );

  const planColumns = useMemo<ColumnDef<BusinessPlan>[]>(() => {
    return [
      { header: 'Mã PAKD', accessorKey: 'planCode' },
      { header: 'Input', accessorKey: 'input' },
      { header: 'Output', accessorKey: 'output' },
      {
        header: 'Ngày tạo',
        accessorKey: 'createdAt',
        cell: (info) => <Typography variant="body2">{dateHelper.formatDate(info.getValue() as string)}</Typography>
      },
      { header: 'Người tạo', accessorKey: 'createdBy' },
      {
        header: 'Trạng thái active',
        accessorKey: 'active',
        cell: (info) => (
          <Chip
            label={(info.getValue() as boolean) ? 'Active' : 'Inactive'}
            color={(info.getValue() as boolean) ? 'success' : 'default'}
            variant={(info.getValue() as boolean) ? 'filled' : 'outlined'}
          />
        )
      },
      {
        header: 'Hành động',
        id: 'actions',
        cell: ({ row }) => {
          const item = row.original;
          return (
            <Tooltip title="Xem PAKD">
              <IconButton onClick={() => navigate(routes.dashboard.businessPlan.detail.replace(':id', item.id))}>
                <EyeOutlined />
              </IconButton>
            </Tooltip>
          );
        }
      }
    ];
  }, [navigate]);

  if (!contract) {
    return (
      <>
        <Breadcrumbs heading="breadcrumbs.salesContracts" links={breadcrumbLinks} />
        <MainCard>
          <Typography>Không tìm thấy hợp đồng.</Typography>
        </MainCard>
      </>
    );
  }

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.salesContracts" links={breadcrumbLinks} />

      <Grid container spacing={2}>
        <Grid size={12}>
          <MainCard>
            <Stack
              direction={{ xs: 'column', md: 'row' }}
              spacing={2}
              justifyContent="space-between"
              alignItems={{ xs: 'stretch', md: 'center' }}
            >
              <Stack spacing={0.5}>
                <Typography variant="h4">
                  {mode === SalesContractMode.Create
                    ? 'Tạo hợp đồng bán'
                    : mode === SalesContractMode.Update
                      ? 'Cập nhật hợp đồng bán'
                      : 'Chi tiết hợp đồng bán'}
                </Typography>
              </Stack>

              <Stack direction="row" spacing={1} justifyContent="flex-end">
                <Button variant="outlined" startIcon={<ArrowLeftOutlined />} onClick={() => navigate(routes.dashboard.sales)}>
                  Quay lại
                </Button>
                {mode === SalesContractMode.View && (
                  <Button
                    variant="contained"
                    startIcon={<EditOutlined />}
                    onClick={() => navigate(routes.dashboard.salesContract.edit.replace(':id', contract.id))}
                    disabled={contract.id === 'NEW'}
                  >
                    Sửa
                  </Button>
                )}
              </Stack>
            </Stack>
          </MainCard>
        </Grid>

        <Grid size={12}>
          <Accordion expanded={sectionBasic.value} onChange={() => sectionBasic.onToggle()}>
            <AccordionSummary expandIcon={<DownOutlined />}>
              <Typography fontWeight={700}>Section 1: Thông tin cơ bản hợp đồng</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text
                      fullWidth
                      label="Mã hợp đồng bán"
                      value={contract.contractCode}
                      onChange={(v) => setContract((prev) => (prev ? { ...prev, contractCode: String(v) } : prev))}
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text
                      fullWidth
                      label="Khách hàng"
                      value={contract.customerName}
                      onChange={(v) => setContract((prev) => (prev ? { ...prev, customerName: String(v) } : prev))}
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text
                      fullWidth
                      label="Tên hàng hóa"
                      value={contract.productName}
                      onChange={(v) => setContract((prev) => (prev ? { ...prev, productName: String(v) } : prev))}
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Number
                      fullWidth
                      label="Giá bán"
                      value={contract.salePrice?.toString() ?? ''}
                      onChange={(v) => {
                        const n = Number(v);
                        setContract((prev) => (prev ? { ...prev, salePrice: Number.isNaN(n) ? 0 : n } : prev));
                      }}
                      disabled={isView}
                      placeholder={isView ? formatVnd(contract.salePrice || 0) : 'Nhập giá bán...'}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.DatePicker
                      fullWidth
                      label="Ngày ký hợp đồng"
                      value={contract.signedAt}
                      onChange={(v: DatePickerFormat) => setContract((prev) => (prev ? { ...prev, signedAt: v } : prev))}
                      disabled={isView}
                      slotProps={{ textField: { size: 'small' } }}
                    />
                  </Grid>
                </Grid>
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        <Grid size={12}>
          <Accordion expanded={sectionPlans.value} onChange={() => sectionPlans.onToggle()}>
            <AccordionSummary expandIcon={<DownOutlined />}>
              <Typography fontWeight={700}>Section 2: List Phương án kinh doanh đang áp dụng</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Stack spacing={1}>
                <DataTable
                  data={planData.items}
                  columns={planColumns}
                  totalPage={planData.pagination?.totalPages || 1}
                  onLoad={handleFetchPlans}
                />
              </Stack>
            </AccordionDetails>
          </Accordion>
        </Grid>
      </Grid>
    </>
  );
};

export default SalesContractDetailPage;
