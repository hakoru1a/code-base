import { ApproveDialog, ApproveDecision } from '@components';
import type { DialogContextProps } from '@contexts/dialog';
import { dateHelper } from '@utils/helpers';
import type { ColumnDef } from '@tanstack/react-table';
import { Chip, IconButton, Stack, Tooltip, Typography } from '@mui/material';
import { CheckCircleOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';

import type { BusinessPlan } from '../types';
import { BusinessPlanStatus } from '../constants';

export const buildBusinessPlanColumns = ({
  dialog,
  onView,
  onDeleteSuccess,
  onApproveSuccess
}: {
  dialog: DialogContextProps;
  onView: (id: string) => void;
  onDeleteSuccess: (id: string) => void;
  onApproveSuccess: (id: string, payload: { decision: ApproveDecision; note: string }) => void;
}): ColumnDef<BusinessPlan>[] => {
  return [
    { header: 'Mã PAKD', accessorKey: 'planCode' },
    {
      header: 'Trạng thái',
      accessorKey: 'status',
      cell: (info) => {
        const v = info.getValue() as BusinessPlan['status'];
        const cfg =
          v === BusinessPlanStatus.Active
            ? { label: 'Hoạt động', color: 'warning' as const }
            : v === BusinessPlanStatus.Completed
              ? { label: 'Hoàn thành', color: 'success' as const }
              : { label: 'Dừng', color: 'error' as const };

        return <Chip label={cfg.label} color={cfg.color} size="small" variant="outlined" />;
      }
    },
    {
      header: 'Số lượng (tấn)',
      accessorKey: 'outputTon',
      cell: (info) => <Typography variant="body2">{Number(info.getValue() || 0).toLocaleString('vi-VN')}</Typography>
    },
    {
      header: 'Timeline',
      id: 'timeline',
      cell: ({ row }) => {
        const item = row.original;
        const start = item.startDate ? dateHelper.from(item.startDate) : null;
        const end = item.endDate ? dateHelper.from(item.endDate) : null;
        const days =
          start && end && start.isValid() && end.isValid()
            ? Math.max(1, Math.ceil(end.diff(start, 'day')) + 1)
            : Math.max(1, Math.ceil((row.original.outputTon || 0) / 80));
        const required = (row.original.outputTon || 0) / days;
        return (
          <Stack spacing={0.25}>
            <Typography variant="body2" fontWeight={700}>
              {days} ngày
            </Typography>
            <Typography variant="caption" color="text.secondary">
              ~{required.toFixed(2)} tấn/ngày
            </Typography>
            {item.status === BusinessPlanStatus.Active && start?.isValid() && end?.isValid() && (
              <Typography variant="caption" color="text.secondary">
                {dateHelper.formatDate(start)} → {dateHelper.formatDate(end)}
              </Typography>
            )}
          </Stack>
        );
      }
    },
    { header: 'Input đầu vào', accessorKey: 'inputSummary' },
    { header: 'Output đầu ra', accessorKey: 'outputSummary' },
    {
      header: 'Ngày tạo',
      accessorKey: 'createdAt',
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
              <IconButton
                onClick={(e) => {
                  e.stopPropagation();
                  onView(item.id);
                }}
              >
                <EyeOutlined />
              </IconButton>
            </Tooltip>
            <Tooltip title="Xóa">
              <IconButton
                color="error"
                onClick={async (e) => {
                  e.stopPropagation();
                  const result = await dialog.confirm({
                    title: 'Xóa phương án kinh doanh',
                    description: `Bạn chắc chắn muốn xóa "${item.planCode}"?`
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
                onClick={async (e) => {
                  e.stopPropagation();
                  const result = await dialog.show(ApproveDialog, { title: `Xét duyệt ${item.planCode}` });
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
