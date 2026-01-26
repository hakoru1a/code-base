import { ApproveDialog, ApproveDecision } from '@components';
import type { DialogContextProps } from '@contexts/dialog';
import { dateHelper } from '@utils/helpers';
import type { ColumnDef } from '@tanstack/react-table';
import { IconButton, Stack, Tooltip, Typography } from '@mui/material';
import { CheckCircleOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';

import type { SalesContract } from '../types';

const formatVnd = (value: number) =>
  new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value);

export const buildSalesContractColumns = ({
  dialog,
  onView,
  onDeleteSuccess,
  onApproveSuccess
}: {
  dialog: DialogContextProps;
  onView: (id: string) => void;
  onDeleteSuccess: (id: string) => void;
  onApproveSuccess: (id: string, payload: { decision: ApproveDecision; note: string }) => void;
}): ColumnDef<SalesContract>[] => {
  return [
    { header: 'Mã hợp đồng bán', accessorKey: 'contractCode' },
    { header: 'Khách hàng', accessorKey: 'customerName' },
    { header: 'Tên hàng hóa', accessorKey: 'productName' },
    {
      header: 'Giá bán',
      accessorKey: 'salePrice',
      cell: (info) => <Typography variant="body2">{formatVnd(Number(info.getValue() || 0))}</Typography>
    },
    {
      header: 'Ngày ký hợp đồng',
      accessorKey: 'signedAt',
      cell: (info) => <Typography variant="body2">{dateHelper.formatDate(info.getValue() as string)}</Typography>
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
            <Tooltip title="Xóa">
              <IconButton
                color="error"
                onClick={async () => {
                  const result = await dialog.confirm({
                    title: 'Xóa hợp đồng bán',
                    description: `Bạn chắc chắn muốn xóa "${item.contractCode}"?`
                  });
                  if (!result?.success) return;
                  onDeleteSuccess(item.id);
                }}
              >
                <DeleteOutlined />
              </IconButton>
            </Tooltip>
            <Tooltip title="Xét duyệt">
              <IconButton
                color="primary"
                onClick={async () => {
                  const result = await dialog.show(ApproveDialog, { title: `Xét duyệt ${item.contractCode}` });
                  if (!result?.success) return;
                  const payload = result.payload as { decision: ApproveDecision; note: string };
                  onApproveSuccess(item.id, payload);
                }}
              >
                <CheckCircleOutlined />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      }
    }
  ];
};
