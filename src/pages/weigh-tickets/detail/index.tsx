import { ArrowLeftOutlined, DownOutlined, EyeOutlined } from '@ant-design/icons';
import { Breadcrumbs, DataTable, Field, MainCard } from '@components';
import { useDialog, useToggle } from '@hooks';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Button,
  Chip,
  Divider,
  Grid,
  IconButton,
  Stack,
  Tooltip,
  Typography
} from '@mui/material';
import { routes } from '@routes';
import type { ColumnDef } from '@tanstack/react-table';
import { dateHelper, themeHelper } from '@utils/helpers';
import { useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

import ViewImageDialog from '../components/ViewImageDialog';
import { PaymentStatus, WeighTicketMode, WeighTicketStatus, WeighTicketType } from '../constants';
import { mockWeighTickets } from '../mock';
import type { WeighTicket, WeighTicketPaymentRow, WeighTicketWeighingRow } from '../types';

const formatVnd = (value: number) =>
  new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value);

const resolveAssetUrl = (asset: string | null | undefined) => {
  if (!asset) return '';
  if (asset.startsWith('http') || asset.startsWith('data:')) return asset;
  const parts = asset.split('/');
  if (parts.length < 2) return asset;
  const name = parts[parts.length - 1];
  const path = parts.slice(0, -1).join('/');
  return themeHelper.getImageUrl(name, path);
};

const getModeFromPath = (pathname: string): WeighTicketMode => {
  if (pathname.endsWith('/qc')) return WeighTicketMode.Qc;
  return WeighTicketMode.View;
};

const WeighTicketDetailPage = () => {
  const dialog = useDialog();
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();

  const mode = useMemo(() => getModeFromPath(location.pathname), [location.pathname]);
  const isView = mode === WeighTicketMode.View;

  const ticketFromMock = useMemo<WeighTicket | null>(() => mockWeighTickets.find((t) => t.id === id) ?? null, [id]);
  const [ticket, setTicket] = useState<WeighTicket | null>(ticketFromMock);

  const breadcrumbLinks = [
    { title: 'breadcrumbs.home', to: routes.default },
    { title: 'breadcrumbs.dashboard', to: routes.dashboard.base },
    { title: 'breadcrumbs.weighTickets', to: routes.dashboard.weighTickets },
    { title: isView ? 'breadcrumbs.details' : 'breadcrumbs.qc' }
  ];

  const sectionCustomer = useToggle(true);
  const sectionVehicle = useToggle(true);
  const sectionWeighing = useToggle(true);
  const sectionPayment = useToggle(true);
  const sectionQc = useToggle(true);

  const [qcFilesError, setQcFilesError] = useState<string>('');
  const ticketCode = ticket?.ticketCode ?? '';
  const qcImages = useMemo(
    () => (ticket?.qc?.qcImages ?? []).filter((x) => typeof x === 'string' && x.trim().length > 0),
    [ticket?.qc?.qcImages]
  );

  const weighColumns = useMemo<ColumnDef<WeighTicketWeighingRow>[]>(() => {
    return [
      { header: 'Cân vào (KG)', accessorKey: 'weighInKg' },
      {
        header: 'Thời gian cân vào',
        accessorKey: 'weighInAt',
        cell: (info) => dateHelper.formatDateTime(info.getValue() as string)
      },
      { header: 'Cân ra (KG)', accessorKey: 'weighOutKg', cell: (info) => (info.getValue() ? String(info.getValue()) : '-') },
      {
        header: 'Thời gian cân ra',
        accessorKey: 'weighOutAt',
        cell: (info) => ((info.getValue() as string) ? dateHelper.formatDateTime(info.getValue() as string) : '-')
      },
      { header: 'Trọng lượng thực', accessorKey: 'actualWeightKg' },
      { header: 'Tạp chất (%)', accessorKey: 'impurityPercent' },
      { header: 'Khối lượng thanh toán', accessorKey: 'payableWeightKg' }
    ];
  }, []);

  const paymentColumns = useMemo<ColumnDef<WeighTicketPaymentRow>[]>(() => {
    return [
      { header: 'Đơn giá tham khảo', accessorKey: 'referenceUnitPriceVnd' },
      {
        header: 'Thành tiền tham khảo',
        accessorKey: 'referenceAmountVnd',
        cell: (info) => formatVnd(Number(info.getValue() || 0))
      },
      { header: 'Đơn giá thực tế', accessorKey: 'actualUnitPriceVnd' },
      {
        header: 'Thành tiền thực tế',
        accessorKey: 'actualAmountVnd',
        cell: (info) => formatVnd(Number(info.getValue() || 0))
      },
      {
        header: 'Trạng thái',
        accessorKey: 'status',
        cell: (info) => ((info.getValue() as PaymentStatus) === PaymentStatus.Paid ? 'Đã thanh toán' : 'Chưa thanh toán')
      },
      {
        header: 'Ngày thanh toán',
        accessorKey: 'paidAt',
        cell: (info) => ((info.getValue() as string) ? dateHelper.formatDate(info.getValue() as string) : '-')
      },
      {
        header: 'Hành động',
        id: 'actions',
        cell: ({ row }) => {
          const p = row.original;
          return (
            <Tooltip title={p.invoiceImageUrl ? 'Xem hóa đơn' : 'Chưa có hóa đơn (mock)'}>
              <span>
                <IconButton
                  onClick={async () => {
                    const url = resolveAssetUrl(p.invoiceImageUrl || '');
                    if (!url) return;
                    await dialog.show(ViewImageDialog, { title: `Hóa đơn - ${ticketCode}`, imageUrl: url });
                  }}
                  disabled={!p.invoiceImageUrl}
                >
                  <EyeOutlined />
                </IconButton>
              </span>
            </Tooltip>
          );
        }
      }
    ];
  }, [dialog, ticketCode]);

  const addQcImages = (files: FileList | null) => {
    if (!files?.length) return;
    setQcFilesError('');

    const images = Array.from(files).filter((f) => f.type.startsWith('image/'));
    if (!images.length) {
      setQcFilesError('Vui lòng chọn file hình ảnh.');
      return;
    }

    const urls = images.map((f) => URL.createObjectURL(f)).filter(Boolean);
    setTicket((prev) => (prev ? { ...prev, qc: { ...prev.qc, qcImages: [...prev.qc.qcImages, ...urls] } } : prev));
  };

  const removeQcImage = (url: string) => {
    setTicket((prev) => (prev ? { ...prev, qc: { ...prev.qc, qcImages: prev.qc.qcImages.filter((x) => x !== url) } } : prev));
    if (url.startsWith('blob:')) {
      try {
        URL.revokeObjectURL(url);
      } catch {
        // ignore
      }
    }
  };

  if (!ticket) {
    return (
      <>
        <Breadcrumbs heading="breadcrumbs.weighTickets" links={breadcrumbLinks} />
        <MainCard>
          <Typography>Không tìm thấy phiếu cân.</Typography>
        </MainCard>
      </>
    );
  }

  return (
    <>
      <Breadcrumbs heading="breadcrumbs.weighTickets" links={breadcrumbLinks} />

      {!isView ? (
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
                  <Typography variant="h4">Đánh giá (QC)</Typography>
                  <Typography color="text.secondary">{ticket.ticketCode}</Typography>
                </Stack>
                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button
                    variant="outlined"
                    startIcon={<ArrowLeftOutlined />}
                    onClick={() => navigate(routes.dashboard.weighTicket.detail.replace(':id', ticket.id))}
                  >
                    Quay lại
                  </Button>
                  <Button
                    variant="contained"
                    onClick={async () => {
                      await dialog.success({ title: 'Lưu QC', description: 'Đã lưu đánh giá QC (mock).' });
                      navigate(routes.dashboard.weighTicket.detail.replace(':id', ticket.id));
                    }}
                    disabled={!ticket.qc.qcDecision}
                  >
                    Lưu
                  </Button>
                </Stack>
              </Stack>
            </MainCard>
          </Grid>

          <Grid size={12}>
            <MainCard title="Thông tin đánh giá">
              <Stack spacing={2}>
                <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                  <Box sx={{ flex: 1, maxWidth: { md: 360 } }}>
                    <Field.Select
                      fullWidth
                      label="Đánh giá"
                      value={ticket.qc.qcDecision ?? ''}
                      onChange={(v) =>
                        setTicket((prev) =>
                          prev ? { ...prev, qc: { ...prev.qc, qcDecision: (String(v) as 'PASS' | 'FAIL') || null } } : prev
                        )
                      }
                      defaultOptionLabel="Chọn"
                      options={[
                        { label: 'Đạt', value: 'PASS' },
                        { label: 'Không đạt', value: 'FAIL' }
                      ]}
                    />
                  </Box>
                  <Box sx={{ flex: 1 }}>
                    <Field.Text
                      fullWidth
                      label="Ghi chú (note)"
                      value={ticket.qc.qcComment}
                      onChange={(v) => setTicket((prev) => (prev ? { ...prev, qc: { ...prev.qc, qcComment: String(v) } } : prev))}
                      placeholder="Nhập ghi chú..."
                      multiline
                      rows={3}
                      maxLength={400}
                    />
                  </Box>
                </Stack>

                <Divider />

                <Stack spacing={1}>
                  <Stack
                    direction={{ xs: 'column', sm: 'row' }}
                    spacing={1}
                    alignItems={{ xs: 'flex-start', sm: 'center' }}
                    justifyContent="space-between"
                  >
                    <Stack spacing={0.25}>
                      <Typography variant="body2" fontWeight={700}>
                        Hình ảnh
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Upload nhiều ảnh để minh họa đánh giá (mock).
                      </Typography>
                    </Stack>
                    <Button variant="outlined" component="label">
                      Upload ảnh
                      <input hidden type="file" accept="image/*" multiple onChange={(e) => addQcImages(e.currentTarget.files)} />
                    </Button>
                  </Stack>
                  {qcFilesError && (
                    <Typography variant="caption" color="error">
                      {qcFilesError}
                    </Typography>
                  )}

                  {qcImages.length ? (
                    <Grid container spacing={1}>
                      {qcImages.map((img, idx) => {
                        const url = resolveAssetUrl(img);
                        return (
                          <Grid key={`${img}-${idx}`} size={{ xs: 6, sm: 4, md: 3 }}>
                            <Box sx={{ position: 'relative' }}>
                              <Box
                                component="img"
                                src={url}
                                alt={`qc-${idx}`}
                                onClick={async () => {
                                  if (!url) return;
                                  await dialog.show(ViewImageDialog, { title: `Ảnh QC - ${ticket.ticketCode}`, imageUrl: url });
                                }}
                                sx={(theme) => ({
                                  width: '100%',
                                  height: 140,
                                  objectFit: 'cover',
                                  borderRadius: 1,
                                  border: `1px solid ${theme.palette.divider}`,
                                  cursor: 'pointer'
                                })}
                              />
                              <IconButton
                                size="small"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  removeQcImage(img);
                                }}
                                sx={(theme) => ({
                                  position: 'absolute',
                                  top: 6,
                                  right: 6,
                                  bgcolor: theme.palette.background.paper,
                                  border: `1px solid ${theme.palette.divider}`,
                                  '&:hover': { bgcolor: theme.palette.background.paper }
                                })}
                              >
                                ✕
                              </IconButton>
                            </Box>
                          </Grid>
                        );
                      })}
                    </Grid>
                  ) : (
                    <Box
                      sx={(theme) => ({
                        p: 3,
                        borderRadius: 1,
                        border: `1px dashed ${theme.palette.divider}`,
                        bgcolor: theme.palette.action.hover,
                        textAlign: 'center'
                      })}
                    >
                      <Typography variant="body2" fontWeight={600}>
                        Chưa có ảnh
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Bấm “Upload ảnh” để thêm ảnh minh họa.
                      </Typography>
                    </Box>
                  )}
                </Stack>
              </Stack>
            </MainCard>
          </Grid>
        </Grid>
      ) : (
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
                  <Typography variant="h4">{isView ? 'Xem phiếu cân' : 'Đánh giá (QC)'}</Typography>
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Typography color="text.secondary">{ticket.ticketCode}</Typography>
                    <Chip
                      size="small"
                      label={
                        ticket.status === WeighTicketStatus.Active
                          ? 'Đang trong xưởng'
                          : ticket.status === WeighTicketStatus.Completed
                            ? 'Hoàn tất'
                            : 'Đã hủy'
                      }
                      color={
                        ticket.status === WeighTicketStatus.Active
                          ? 'success'
                          : ticket.status === WeighTicketStatus.Completed
                            ? 'info'
                            : 'error'
                      }
                      variant="filled"
                    />
                    <Chip
                      size="small"
                      label={ticket.ticketType === WeighTicketType.In ? 'Phiếu nhập' : 'Phiếu xuất'}
                      color="primary"
                      variant="outlined"
                    />
                  </Stack>
                </Stack>

                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button variant="outlined" startIcon={<ArrowLeftOutlined />} onClick={() => navigate(routes.dashboard.weighTickets)}>
                    Quay lại
                  </Button>
                  {isView && (
                    <Button variant="contained" onClick={() => navigate(routes.dashboard.weighTicket.qc.replace(':id', ticket.id))}>
                      Đánh giá (QC)
                    </Button>
                  )}
                </Stack>
              </Stack>
            </MainCard>
          </Grid>

          <Grid size={12}>
            <Accordion expanded={sectionCustomer.value} onChange={() => sectionCustomer.onToggle()}>
              <AccordionSummary expandIcon={<DownOutlined />}>
                <Typography fontWeight={700}>1. Thông tin khách hàng</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Tên khách hàng" value={ticket.customer.customerName} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Số điện thoại" value={ticket.customer.phoneNumber} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Người nhận tiền" value={ticket.customer.payeeName} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Số tài khoản" value={ticket.customer.bankAccountNumber} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Ngân hàng" value={ticket.customer.bankName} disabled />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Grid>

          <Grid size={12}>
            <Accordion expanded={sectionVehicle.value} onChange={() => sectionVehicle.onToggle()}>
              <AccordionSummary expandIcon={<DownOutlined />}>
                <Typography fontWeight={700}>2. Thông tin xe và hàng hóa</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Biển số xe" value={ticket.vehicleGoods.vehiclePlate} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Loại hàng hóa" value={ticket.vehicleGoods.goodsType} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Phân loại FSC" value={ticket.vehicleGoods.fscCategory} disabled />
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Field.Text fullWidth label="Hồ sơ nguồn gốc" value={ticket.vehicleGoods.originProfile} disabled />
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Grid>

          <Grid size={12}>
            <Accordion expanded={sectionWeighing.value} onChange={() => sectionWeighing.onToggle()}>
              <AccordionSummary expandIcon={<DownOutlined />}>
                <Typography fontWeight={700}>3. Số liệu cân</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <DataTable data={ticket.weighings} columns={weighColumns} totalPage={1} onLoad={async () => {}} />
              </AccordionDetails>
            </Accordion>
          </Grid>

          <Grid size={12}>
            <Accordion expanded={sectionPayment.value} onChange={() => sectionPayment.onToggle()}>
              <AccordionSummary expandIcon={<DownOutlined />}>
                <Typography fontWeight={700}>4. Thanh toán</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={1.5}>
                  <DataTable data={ticket.payments} columns={paymentColumns} totalPage={1} onLoad={async () => {}} />

                  <Divider />

                  <Typography variant="caption" color="text.secondary">
                    Mock note: số liệu thanh toán hiển thị theo dữ liệu giả để khách hàng dễ hình dung.
                  </Typography>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grid>

          <Grid size={12}>
            <Accordion expanded={sectionQc.value} onChange={() => sectionQc.onToggle()}>
              <AccordionSummary expandIcon={<DownOutlined />}>
                <Typography fontWeight={700}>5. Đánh giá (QC)</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Stack spacing={2}>
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, md: 4 }}>
                      <Field.Select
                        fullWidth
                        label="Đánh giá"
                        value={ticket.qc.qcDecision ?? ''}
                        options={[
                          { label: 'Đạt', value: 'PASS' },
                          { label: 'Không đạt', value: 'FAIL' }
                        ]}
                        disabled
                      />
                    </Grid>
                    <Grid size={{ xs: 12, md: 8 }}>
                      <Field.Text
                        fullWidth
                        label="Ghi chú (note)"
                        value={ticket.qc.qcComment}
                        multiline
                        rows={3}
                        maxLength={400}
                        disabled
                      />
                    </Grid>
                  </Grid>

                  <Stack spacing={1}>
                    <Typography variant="body2" fontWeight={600}>
                      Ảnh sản phẩm
                    </Typography>
                    <Grid container spacing={2}>
                      {qcImages.length ? (
                        <Grid size={12}>
                          <Grid container spacing={1}>
                            {qcImages.map((img, idx) => {
                              const url = resolveAssetUrl(img);
                              return (
                                <Grid key={`${img}-${idx}`} size={{ xs: 6, sm: 4, md: 3 }}>
                                  <Box
                                    component="img"
                                    src={url}
                                    alt={`qc-${idx}`}
                                    onClick={async () => {
                                      if (!url) return;
                                      await dialog.show(ViewImageDialog, { title: `Ảnh QC - ${ticket.ticketCode}`, imageUrl: url });
                                    }}
                                    sx={(theme) => ({
                                      width: '100%',
                                      height: 140,
                                      objectFit: 'cover',
                                      borderRadius: 1,
                                      border: `1px solid ${theme.palette.divider}`,
                                      cursor: 'pointer'
                                    })}
                                  />
                                </Grid>
                              );
                            })}
                          </Grid>
                        </Grid>
                      ) : (
                        <Grid size={12}>
                          <Box
                            sx={(theme) => ({
                              p: 2.5,
                              borderRadius: 1,
                              border: `1px dashed ${theme.palette.divider}`,
                              bgcolor: theme.palette.action.hover
                            })}
                          >
                            <Typography variant="body2" color="text.secondary">
                              Chưa có ảnh.
                            </Typography>
                          </Box>
                        </Grid>
                      )}
                    </Grid>
                  </Stack>
                </Stack>
              </AccordionDetails>
            </Accordion>
          </Grid>
        </Grid>
      )}
    </>
  );
};

export default WeighTicketDetailPage;
