import { dateHelper } from '@utils/helpers';
import { Box, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material';

import { mockStaff, mockLineStaff } from '../../mock';

const getWorkTimeLabel = (shift: string) => {
  // shift is already a user-friendly time range (mock)
  return shift || '-';
};

const LineStaffExpand = ({ lineId }: { lineId: string }) => {
  const staffIds = mockLineStaff.find((x) => x.lineId === lineId)?.staffIds ?? [];
  const staff = staffIds.map((id) => mockStaff.find((s) => s.id === id)).filter(Boolean);

  if (!staff.length) {
    return (
      <Box sx={{ p: 1 }}>
        <Typography variant="body2" color="text.secondary">
          Chưa có nhân viên tham gia.
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 1 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 700 }}>Tên nhân viên</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Vai trò</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Thời gian làm việc</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {staff.map((s) => (
            <TableRow key={s!.id}>
              <TableCell>{s!.name}</TableCell>
              <TableCell>{s!.role}</TableCell>
              <TableCell>
                {getWorkTimeLabel(s!.shift)}{' '}
                <Typography component="span" variant="caption" color="text.secondary">
                  • {dateHelper.today('DD/MM/YYYY')}
                </Typography>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default LineStaffExpand;
