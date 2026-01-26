import type { ColumnDef } from '@tanstack/react-table';
import { IconButton, Stack, Tooltip, Typography } from '@mui/material';
import { EyeOutlined, SafetyCertificateOutlined, StopOutlined } from '@ant-design/icons';

import { WeighTicketStatus } from '../constants';
import type { WeighTicket } from '../types';

const formatKg = (value: number) => new Intl.NumberFormat('vi-VN').format(value);

export const buildWeighTicketColumns = ({
  onView,
  onQc,
  onCancel
}: {
  onView: (id: string) => void;
  onQc: (id: string) => void;
  onCancel: (payload: { id: string; ticketCode: string }) => void;
}): ColumnDef<WeighTicket>[] => {
  return [
    { header: 'Mã phiếu cân', accessorKey: 'ticketCode' },
    { header: 'Biển số xe', accessorKey: 'vehicleGoods.vehiclePlate' },
    { header: 'Tên khách hàng', accessorKey: 'customer.customerName' },
    {
      header: 'Thanh toán (KG)',
      id: 'payableWeightKg',
      cell: ({ row }) => {
        const item = row.original;
        const last = item.weighings[item.weighings.length - 1];
        return <Typography variant="body2">{formatKg(Number(last?.payableWeightKg ?? 0))}</Typography>;
      }
    },
    {
      header: 'Hành động',
      id: 'actions',
      cell: ({ row }) => {
        const item = row.original;
        return (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Xem">
              <IconButton onClick={() => onView(item.id)}>
                <EyeOutlined />
              </IconButton>
            </Tooltip>
            <Tooltip title="Đánh giá (QC)">
              <IconButton color="primary" onClick={() => onQc(item.id)}>
                <SafetyCertificateOutlined />
              </IconButton>
            </Tooltip>
            <Tooltip title="Hủy phiếu cân">
              <IconButton
                color="error"
                onClick={() => onCancel({ id: item.id, ticketCode: item.ticketCode })}
                disabled={item.status === WeighTicketStatus.Cancelled}
              >
                <StopOutlined />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      }
    }
  ];
};
