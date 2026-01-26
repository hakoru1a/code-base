import { Breadcrumbs, DataTable, Field, MainCard, TableFetchParams } from '@components';
import { useDialog, useToggle } from '@hooks';
import { routes } from '@routes';
import { dateHelper } from '@utils/helpers';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Button,
  Divider,
  Grid,
  LinearProgress,
  Stack,
  Typography
} from '@mui/material';
import type { ColumnDef } from '@tanstack/react-table';
import { ArrowLeftOutlined, DownOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import type { PagedResult } from '@services/core';
import { useCallback, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

import { BusinessPlanMode } from '../constants';
import { mockBusinessPlans, mockDefaultParams, mockProductionLines } from '../mock';
import type { BusinessPlanDetail, BusinessPlan, ProductionLineMaster } from '../types';
import SelectProductionLinesDialog, { SelectProductionLinesDialogPayload } from './components/SelectProductionLinesDialog';
import { buildSelectedLineColumns } from './production-lines-columns';

const formatVnd = (value: number) =>
  new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value);

const formatTon = (v: number) => Math.round(v).toLocaleString('vi-VN');

const getModeFromPath = (pathname: string): BusinessPlanMode => {
  if (pathname.endsWith('/create')) return BusinessPlanMode.Create;
  if (pathname.endsWith('/edit')) return BusinessPlanMode.Update;
  return BusinessPlanMode.View;
};

const buildMockComputed = ({
  outputTon,
  params,
  selectedLines
}: {
  outputTon: number;
  params: typeof mockDefaultParams;
  selectedLines: ProductionLineMaster[];
}) => {
  const totalMotors = selectedLines.reduce((sum, l) => sum + (Number(l.motors) || 0), 0) || 12;
  const totalStaff = selectedLines.reduce((sum, l) => sum + (Number(l.staff) || 0), 0) || 24;
  const totalEffectiveCapacityTonPerDay =
    selectedLines.reduce((sum, l) => sum + (Number(l.capacityTonPerDay) || 0) * (Number(l.efficiency) || 0), 0) || 80;

  const timelineDays = Math.max(1, Math.ceil((Number(outputTon) || 0) / Math.max(1, totalEffectiveCapacityTonPerDay)));
  const requiredTonPerDay = (Number(outputTon) || 0) / timelineDays;

  const totalFixedCostVnd =
    (Number(params.fixedCosts.logistic) || 0) +
    (Number(params.fixedCosts.customs) || 0) +
    (Number(params.fixedCosts.finance) || 0) +
    (Number(params.fixedCosts.management) || 0);

  const totalIndirectCostVnd =
    (Number(params.indirectCosts.electricity) || 0) + (Number(params.indirectCosts.vehicle) || 0) + (Number(params.indirectCosts.hr) || 0);

  const electricityCostVnd = totalMotors * timelineDays * 8 * 12 * 2500; // mock

  return {
    ageFactor: 1, // mock
    requiredInputTon: (Number(outputTon) || 0) * 1.15, // mock
    totalMotors,
    totalStaff,
    totalEffectiveCapacityTonPerDay,
    timelineDays,
    requiredTonPerDay,
    electricityCostVnd,
    totalFixedCostVnd,
    totalIndirectCostVnd,
    totalCostVnd: totalFixedCostVnd + totalIndirectCostVnd
  };
};

const buildMockDetailFromList = (plan: BusinessPlan): BusinessPlanDetail => {
  const defaultLines =
    plan.id === 'BP-001'
      ? [mockProductionLines[0], mockProductionLines[1]]
      : plan.id === 'BP-002'
        ? [mockProductionLines[1], mockProductionLines[2]]
        : [mockProductionLines[0]];

  return {
    id: plan.id,
    planCode: plan.planCode,
    salesContractCode: plan.salesContractCode,
    qualityCommitment: 'Độ ẩm ≤ 12%, tạp chất ≤ 1%',
    outputTon: plan.outputTon,
    params: mockDefaultParams,
    selectedLines: defaultLines.filter(Boolean),
    computed: buildMockComputed({ outputTon: plan.outputTon, params: mockDefaultParams, selectedLines: defaultLines.filter(Boolean) }),
    startDate: plan.startDate,
    endDate: plan.endDate
  };
};

const BusinessPlanDetailPage = () => {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const dialog = useDialog();

  const mode = useMemo(() => getModeFromPath(location.pathname), [location.pathname]);
  const isView = mode === BusinessPlanMode.View;

  const detailFromMock = useMemo<BusinessPlanDetail | null>(() => {
    if (mode === BusinessPlanMode.Create) {
      return {
        id: 'NEW',
        planCode: '',
        salesContractCode: '',
        qualityCommitment: '',
        outputTon: 0,
        params: mockDefaultParams,
        selectedLines: [],
        computed: null,
        startDate: null,
        endDate: null
      };
    }
    const listItem = mockBusinessPlans.find((p) => p.id === id) ?? null;
    return listItem ? buildMockDetailFromList(listItem) : null;
  }, [id, mode]);

  const [plan, setPlan] = useState<BusinessPlanDetail | null>(detailFromMock);

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.businessPlans', to: routes.dashboard.businessPlans },
    { title: 'breadcrumbs.details' }
  ];

  const sectionBasic = useToggle(true);
  const sectionParams = useToggle(true);
  const sectionComputed = useToggle(true);

  const selectedLines = useMemo(() => plan?.selectedLines ?? [], [plan?.selectedLines]);

  const [lineData, setLineData] = useState<PagedResult<ProductionLineMaster>>({
    items: [],
    pagination: {
      totalCount: selectedLines.length,
      currentPage: 1,
      pageSize: 10,
      totalPages: 1,
      hasNext: false,
      hasPrevious: false
    }
  });

  const handleFetchLines = useCallback(
    async (params: TableFetchParams): Promise<void> => {
      const result = new Promise<PagedResult<ProductionLineMaster>>((resolve) => {
        setTimeout(() => {
          const currentPage = params.page + 1;
          const totalCount = selectedLines.length;
          const pageSize = params.pageSize;
          const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

          const startIndex = params.page * pageSize;
          const endIndex = startIndex + pageSize;
          const paginatedItems = selectedLines.slice(startIndex, endIndex);

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

      setLineData(await result);
    },
    [selectedLines]
  );

  const lineColumns = useMemo<ColumnDef<ProductionLineMaster>[]>(() => {
    return buildSelectedLineColumns({
      onRemove: (lineId) => {
        setPlan((prev) => (prev ? { ...prev, selectedLines: prev.selectedLines.filter((l) => l.id !== lineId) } : prev));
      }
    });
  }, []);

  const handleCalculate = () => {
    if (!plan) return;
    const computed = buildMockComputed({ outputTon: plan.outputTon, params: plan.params, selectedLines: plan.selectedLines });

    const startDate = dateHelper.today(dateHelper.formatPatterns.iso.date);
    const endDate = dateHelper.addDay(startDate, Math.max(0, computed.timelineDays - 1));

    setPlan((prev) =>
      prev
        ? {
            ...prev,
            startDate,
            endDate: endDate ?? null,
            computed
          }
        : prev
    );
  };

  const renderTimeline = () => {
    if (!plan?.computed) return null;
    const start = plan.startDate ? dateHelper.from(plan.startDate) : null;
    const end = plan.endDate ? dateHelper.from(plan.endDate) : null;

    const progress = (() => {
      if (!start || !end || !start.isValid() || !end.isValid()) return 0;
      const now = dateHelper.now();
      const total = end.diff(start, 'minute');
      if (total <= 0) return 0;
      const passed = now.diff(start, 'minute');
      return Math.max(0, Math.min(100, (passed / total) * 100));
    })();

    const outputTon = Number(plan.outputTon) || 0;
    const completedTon = outputTon * (progress / 100);

    return (
      <Stack spacing={1}>
        <Stack direction="row" spacing={1} alignItems="center">
          <Box sx={{ flex: 1, position: 'relative' }}>
            <LinearProgress
              variant="determinate"
              value={progress}
              sx={(theme) => ({
                height: 10,
                borderRadius: 99,
                bgcolor: theme.palette.action.hover,
                '& .MuiLinearProgress-bar': { borderRadius: 99, backgroundColor: theme.palette.warning.main }
              })}
            />
            <Typography
              variant="caption"
              sx={(theme) => ({
                position: 'absolute',
                inset: 0,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: theme.palette.text.primary,
                fontWeight: 700
              })}
            >
              {`${formatTon(completedTon)}/${formatTon(outputTon)} tấn`}
            </Typography>
          </Box>

          <Typography variant="caption" fontWeight={700} sx={{ minWidth: 42, textAlign: 'right' }}>
            {`${Math.round(progress)}%`}
          </Typography>
        </Stack>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <Typography variant="caption" color="text.secondary">
            Tổng thời gian: <b>{plan.computed.timelineDays} ngày</b>
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Cần đạt: <b>{plan.computed.requiredTonPerDay.toFixed(2)} tấn/ngày</b>
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Thời gian:{' '}
            <b>
              {start?.isValid() ? dateHelper.formatDate(start) : '—'} → {end?.isValid() ? dateHelper.formatDate(end) : '—'}
            </b>
          </Typography>
        </Stack>
      </Stack>
    );
  };

  if (!plan) {
    return (
      <>
        <Breadcrumbs heading="breadcrumbs.businessPlans" links={breadcrumbLinks} />
        <MainCard>
          <Typography>Không tìm thấy PAKD.</Typography>
        </MainCard>
      </>
    );
  }

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.businessPlans" links={breadcrumbLinks} />

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
                  {mode === BusinessPlanMode.Create
                    ? 'Tạo phương án kinh doanh'
                    : mode === BusinessPlanMode.Update
                      ? 'Cập nhật phương án kinh doanh'
                      : 'Chi tiết phương án kinh doanh'}
                </Typography>
              </Stack>

              <Stack direction="row" spacing={1} justifyContent="flex-end">
                <Button variant="outlined" startIcon={<ArrowLeftOutlined />} onClick={() => navigate(routes.dashboard.businessPlans)}>
                  Quay lại
                </Button>
                {mode === BusinessPlanMode.View && (
                  <Button
                    variant="contained"
                    startIcon={<EditOutlined />}
                    onClick={() => navigate(routes.dashboard.businessPlan.edit.replace(':id', plan.id))}
                    disabled={plan.id === 'NEW'}
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
              <Typography fontWeight={700}>Section 1: Thông tin cơ bản</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Box>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Text
                      fullWidth
                      label="Mã PAKD"
                      value={plan.planCode}
                      onChange={(v) => setPlan((prev) => (prev ? { ...prev, planCode: String(v) } : prev))}
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Text
                      fullWidth
                      label="Mã hợp đồng bán"
                      value={plan.salesContractCode}
                      onChange={(v) => setPlan((prev) => (prev ? { ...prev, salesContractCode: String(v) } : prev))}
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Number
                      fullWidth
                      label="Số lượng sản phẩm cần (Output - tấn)"
                      value={plan.outputTon?.toString() ?? ''}
                      onChange={(v) => {
                        const n = Number(v);
                        setPlan((prev) => (prev ? { ...prev, outputTon: Number.isNaN(n) ? 0 : n } : prev));
                      }}
                      disabled={isView}
                      placeholder="Nhập output (tấn)..."
                    />
                  </Grid>
                  <Grid size={12}>
                    <Field.Text
                      fullWidth
                      label="Chất lượng cam kết"
                      value={plan.qualityCommitment}
                      onChange={(v) => setPlan((prev) => (prev ? { ...prev, qualityCommitment: String(v) } : prev))}
                      disabled={isView}
                      multiline
                      minRows={2}
                      placeholder="Ví dụ: độ ẩm ≤ 12%..."
                    />
                  </Grid>
                </Grid>
              </Box>
            </AccordionDetails>
          </Accordion>
        </Grid>

        <Grid size={12}>
          <Accordion expanded={sectionParams.value} onChange={() => sectionParams.onToggle()}>
            <AccordionSummary expandIcon={<DownOutlined />}>
              <Typography fontWeight={700}>Section 2: Tham số</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Stack spacing={2}>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Tỉ lệ chuyển đổi (Output/Input)"
                      value={plan.params.conversionRate?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) => (prev ? { ...prev, params: { ...prev.params, conversionRate: Number(v) || 0 } } : prev))
                      }
                      disabled={isView}
                      placeholder="0.9"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Độ ẩm (%)"
                      value={plan.params.moisturePercent?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) => (prev ? { ...prev, params: { ...prev.params, moisturePercent: Number(v) || 0 } } : prev))
                      }
                      disabled={isView}
                      placeholder="12"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Tuổi cây (năm)"
                      value={plan.params.treeAge?.toString() ?? ''}
                      onChange={(v) => setPlan((prev) => (prev ? { ...prev, params: { ...prev.params, treeAge: Number(v) || 0 } } : prev))}
                      disabled={isView}
                      placeholder="6"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Hệ số mùa vụ"
                      value={plan.params.seasonFactor?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) => (prev ? { ...prev, params: { ...prev.params, seasonFactor: Number(v) || 0 } } : prev))
                      }
                      disabled={isView}
                      placeholder="1.05"
                    />
                  </Grid>
                </Grid>

                <Divider />

                <Typography fontWeight={700}>Chi phí cố định (VND)</Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Logistic"
                      value={plan.params.fixedCosts.logistic?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? { ...prev, params: { ...prev.params, fixedCosts: { ...prev.params.fixedCosts, logistic: Number(v) || 0 } } }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Hải quan"
                      value={plan.params.fixedCosts.customs?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? { ...prev, params: { ...prev.params, fixedCosts: { ...prev.params.fixedCosts, customs: Number(v) || 0 } } }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Tài chính"
                      value={plan.params.fixedCosts.finance?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? { ...prev, params: { ...prev.params, fixedCosts: { ...prev.params.fixedCosts, finance: Number(v) || 0 } } }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 3 }}>
                    <Field.Number
                      fullWidth
                      label="Quản lí"
                      value={plan.params.fixedCosts.management?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? { ...prev, params: { ...prev.params, fixedCosts: { ...prev.params.fixedCosts, management: Number(v) || 0 } } }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                </Grid>

                <Divider />

                <Typography fontWeight={700}>Chi phí gián tiếp (VND)</Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Number
                      fullWidth
                      label="Điện"
                      value={plan.params.indirectCosts.electricity?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? {
                                ...prev,
                                params: { ...prev.params, indirectCosts: { ...prev.params.indirectCosts, electricity: Number(v) || 0 } }
                              }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Number
                      fullWidth
                      label="Xe"
                      value={plan.params.indirectCosts.vehicle?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? {
                                ...prev,
                                params: { ...prev.params, indirectCosts: { ...prev.params.indirectCosts, vehicle: Number(v) || 0 } }
                              }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <Field.Number
                      fullWidth
                      label="Nhân sự"
                      value={plan.params.indirectCosts.hr?.toString() ?? ''}
                      onChange={(v) =>
                        setPlan((prev) =>
                          prev
                            ? { ...prev, params: { ...prev.params, indirectCosts: { ...prev.params.indirectCosts, hr: Number(v) || 0 } } }
                            : prev
                        )
                      }
                      disabled={isView}
                    />
                  </Grid>
                </Grid>

                <Divider />

                <Stack
                  direction={{ xs: 'column', md: 'row' }}
                  spacing={1}
                  justifyContent="space-between"
                  alignItems={{ xs: 'stretch', md: 'center' }}
                >
                  <Typography fontWeight={700}>Danh sách chuyền sản xuất</Typography>
                  <Button
                    variant="outlined"
                    startIcon={<PlusOutlined />}
                    onClick={async () => {
                      const result = await dialog.show(SelectProductionLinesDialog, {
                        defaultSelectedIds: plan.selectedLines.map((l) => l.id)
                      });
                      if (!result?.success) return;
                      const payload = result.payload as SelectProductionLinesDialogPayload;
                      setPlan((prev) => (prev ? { ...prev, selectedLines: payload.lines } : prev));
                    }}
                    disabled={isView}
                  >
                    Chọn chuyền
                  </Button>
                </Stack>

                <DataTable
                  data={lineData.items}
                  columns={lineColumns}
                  totalPage={lineData.pagination?.totalPages || 1}
                  onLoad={handleFetchLines}
                />

                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button variant="contained" onClick={handleCalculate} disabled={isView}>
                    Tính toán
                  </Button>
                </Stack>
              </Stack>
            </AccordionDetails>
          </Accordion>
        </Grid>

        <Grid size={12}>
          <Accordion expanded={sectionComputed.value} onChange={() => sectionComputed.onToggle()}>
            <AccordionSummary expandIcon={<DownOutlined />}>
              <Typography fontWeight={700}>Section 3: Kết quả tính toán</Typography>
            </AccordionSummary>
            <AccordionDetails>
              {plan.computed ? (
                <Stack spacing={2}>
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Nguyên liệu đầu vào (tấn)
                        </Typography>
                        <Typography variant="h5">{plan.computed.requiredInputTon.toFixed(2)}</Typography>
                      </MainCard>
                    </Grid>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Tổng công suất hiệu dụng (tấn/ngày)
                        </Typography>
                        <Typography variant="h5">{plan.computed.totalEffectiveCapacityTonPerDay.toFixed(2)}</Typography>
                      </MainCard>
                    </Grid>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Nhân sự cần thiết
                        </Typography>
                        <Typography variant="h5">{plan.computed.totalStaff}</Typography>
                      </MainCard>
                    </Grid>
                  </Grid>

                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Số motor (ước tính)
                        </Typography>
                        <Typography variant="h5">{plan.computed.totalMotors}</Typography>
                      </MainCard>
                    </Grid>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Chi phí điện (mock)
                        </Typography>
                        <Typography variant="h5">{formatVnd(plan.computed.electricityCostVnd)}</Typography>
                      </MainCard>
                    </Grid>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <MainCard>
                        <Typography variant="caption" color="text.secondary">
                          Tổng chi phí (cố định + gián tiếp)
                        </Typography>
                        <Typography variant="h5">{formatVnd(plan.computed.totalCostVnd)}</Typography>
                      </MainCard>
                    </Grid>
                  </Grid>

                  <Divider />

                  <Typography fontWeight={700}>Timeline hoàn thành</Typography>
                  {renderTimeline()}
                </Stack>
              ) : (
                <Typography color="text.secondary">Chưa có dữ liệu tính toán. Bấm “Tính toán” ở Section 2 để xem kết quả.</Typography>
              )}
            </AccordionDetails>
          </Accordion>
        </Grid>
      </Grid>
    </>
  );
};

export default BusinessPlanDetailPage;
