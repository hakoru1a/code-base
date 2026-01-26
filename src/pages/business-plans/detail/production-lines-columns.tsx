import type { ColumnDef } from '@tanstack/react-table';
import { IconButton, Stack, Tooltip, Typography } from '@mui/material';
import { DeleteOutlined } from '@ant-design/icons';

import type { ProductionLineMaster } from '../types';

export const buildSelectedLineColumns = ({ onRemove }: { onRemove: (id: string) => void }): ColumnDef<ProductionLineMaster>[] => {
  return [
    { header: 'Mã chuyền', accessorKey: 'code' },
    { header: 'Tên chuyền', accessorKey: 'name' },
    {
      header: 'Công suất (tấn/ngày)',
      accessorKey: 'capacityTonPerDay',
      cell: (info) => <Typography variant="body2">{Number(info.getValue() || 0).toLocaleString('vi-VN')}</Typography>
    },
    {
      header: 'Hiệu năng',
      accessorKey: 'efficiency',
      cell: (info) => <Typography variant="body2">{(Number(info.getValue() || 0) * 100).toFixed(0)}%</Typography>
    },
    { header: 'Motor', accessorKey: 'motors' },
    { header: 'Nhân sự', accessorKey: 'staff' },
    {
      header: 'Hành động',
      id: 'actions',
      cell: ({ row }) => {
        const item = row.original;
        return (
          <Stack direction="row" spacing={0.5}>
            <Tooltip title="Gỡ khỏi danh sách">
              <IconButton
                color="error"
                onClick={(e) => {
                  e.stopPropagation();
                  onRemove(item.id);
                }}
              >
                <DeleteOutlined />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      }
    }
  ];
};
