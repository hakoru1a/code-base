import type { ColumnDef } from '@tanstack/react-table';
import { Chip, Typography } from '@mui/material';
import { dateHelper } from '@utils/helpers';

import { ProductionAssignmentLineStatus } from '../constants';

export type ProductionLineRow = {
  id: string;
  lineNo: number;
  vehiclePlate: string | null;
  startedAt: string | null;
  status: Exclude<ProductionAssignmentLineStatus, ProductionAssignmentLineStatus.All>;
  note: string;
};

const formatDuration = (startedAt: string | null) => {
  if (!startedAt) return '-';
  const now = dateHelper.now();
  const start = dateHelper.from(startedAt);
  const minutes = Math.max(0, Math.floor(now.diff(start, 'minute')));
  if (minutes < 60) return `${minutes} phút`;
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return `${h}h ${m}m`;
};

export const buildProductionAssignmentColumns = (): ColumnDef<ProductionLineRow>[] => {
  return [
    { header: 'Số chuyền', accessorKey: 'lineNo' },
    {
      header: 'Biển số xe',
      accessorKey: 'vehiclePlate',
      cell: (info) => <Typography variant="body2">{(info.getValue() as string) || '-'}</Typography>
    },
    {
      header: 'Thời gian hoạt động',
      accessorKey: 'startedAt',
      cell: ({ row }) => {
        const item = row.original;
        return (
          <Typography variant="body2">
            {item.startedAt ? dateHelper.formatDateTime(item.startedAt) : '-'}{' '}
            <Typography component="span" variant="caption" color="text.secondary">
              • {formatDuration(item.startedAt)}
            </Typography>
          </Typography>
        );
      }
    },
    {
      header: 'Trạng thái',
      accessorKey: 'status',
      cell: (info) => {
        const v = info.getValue() as ProductionLineRow['status'];
        if (v === ProductionAssignmentLineStatus.Running) return <Chip size="small" color="success" label="Đang chạy" />;
        if (v === ProductionAssignmentLineStatus.Warning) return <Chip size="small" color="warning" label="Cảnh báo" />;
        return <Chip size="small" variant="outlined" label="Trống" />;
      }
    },
    {
      header: 'Note',
      accessorKey: 'note',
      cell: (info) => {
        const note = String(info.getValue() ?? '').trim();
        return (
          <Typography variant="body2" color={note ? 'text.primary' : 'text.secondary'}>
            {note || '-'}
          </Typography>
        );
      }
    }
  ];
};
