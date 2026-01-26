import { dateHelper } from '@utils/helpers';
import { Box, Grid, LinearProgress, Stack, Typography } from '@mui/material';

import type { BusinessPlan } from '../../types';
import { BusinessPlanStatus } from '../../constants';

const DEFAULT_TOTAL_CAPACITY_TON_PER_DAY = 80;

const clamp = (v: number, min: number, max: number) => Math.max(min, Math.min(max, v));
const formatTon = (v: number) => Math.round(v).toLocaleString('vi-VN');

const TimelineExpandPanel = ({ plan }: { plan: BusinessPlan }) => {
  const start = plan.startDate ? dateHelper.from(plan.startDate) : null;
  const end = plan.endDate ? dateHelper.from(plan.endDate) : null;

  const days =
    start && end && start.isValid() && end.isValid()
      ? Math.max(1, Math.ceil(end.diff(start, 'day')) + 1)
      : Math.max(1, Math.ceil(plan.outputTon / DEFAULT_TOTAL_CAPACITY_TON_PER_DAY));

  const requiredTonPerDay = plan.outputTon / days;

  const progress = (() => {
    if (plan.status === BusinessPlanStatus.Completed) return 100;
    if (plan.status !== BusinessPlanStatus.Active) return 0;
    if (!start || !end || !start.isValid() || !end.isValid()) return 0;

    const now = dateHelper.now();
    const total = end.diff(start, 'minute');
    if (total <= 0) return 0;
    const passed = now.diff(start, 'minute');
    return clamp((passed / total) * 100, 0, 100);
  })();

  const outputTon = Number(plan.outputTon) || 0;
  const completedTon = outputTon * (progress / 100);

  return (
    <Box sx={{ p: 2 }}>
      <Stack spacing={1.5}>
        <Typography variant="subtitle1" fontWeight={700}>
          Timeline sản xuất
        </Typography>

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

        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Stack spacing={0.5}>
              <Typography variant="caption" color="text.secondary">
                Tổng thời gian
              </Typography>
              <Typography variant="body2" fontWeight={700}>
                {days} ngày
              </Typography>
            </Stack>
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Stack spacing={0.5}>
              <Typography variant="caption" color="text.secondary">
                Sản lượng cần đạt
              </Typography>
              <Typography variant="body2" fontWeight={700}>
                {requiredTonPerDay.toFixed(2)} tấn/ngày
              </Typography>
            </Stack>
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Stack spacing={0.5}>
              <Typography variant="caption" color="text.secondary">
                Thời gian
              </Typography>
              <Typography variant="body2" fontWeight={700}>
                {start && start.isValid() ? dateHelper.formatDate(start) : '—'} → {end && end.isValid() ? dateHelper.formatDate(end) : '—'}
              </Typography>
            </Stack>
          </Grid>
        </Grid>
      </Stack>
    </Box>
  );
};

export default TimelineExpandPanel;
