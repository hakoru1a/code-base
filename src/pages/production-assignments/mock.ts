import { WeighTicketStatus, WeighTicketType } from '@pages/weigh-tickets/constants';
import { mockWeighTickets } from '@pages/weigh-tickets/mock';
import type { ActiveVehicle, ProductionLineCard, StaffCard, LineStaffAssignment } from './types';

export const mockActiveVehicles: ActiveVehicle[] = mockWeighTickets
  .filter((t) => t.status === WeighTicketStatus.Active && t.ticketType === WeighTicketType.In)
  .map((t) => ({
    ticketId: t.id,
    ticketCode: t.ticketCode,
    vehiclePlate: t.vehicleGoods.vehiclePlate,
    enteredFactoryAt: t.weighings?.[0]?.weighInAt ?? t.createdAt
  }));

export const mockProductionLines: ProductionLineCard[] = Array.from({ length: 9 }).map((_, idx) => {
  const lineNo = idx + 1;
  const assigned = lineNo === 2 ? mockActiveVehicles[0] : null;

  return {
    id: `LINE-${lineNo}`,
    lineNo,
    ticketId: assigned?.ticketId ?? null,
    ticketCode: assigned?.ticketCode ?? null,
    startedAt: assigned ? '2026-01-26T08:05:00' : null,
    endedAt: null
  };
});

export const mockStaff: StaffCard[] = [
  { id: 'S-001', name: 'Nguyễn Văn A', role: 'Tổ trưởng', shift: 'Ca 1 (07:00-15:00)' },
  { id: 'S-002', name: 'Trần Thị B', role: 'Vận hành', shift: 'Ca 1 (07:00-15:00)' },
  { id: 'S-003', name: 'Lê Văn C', role: 'Vận hành', shift: 'Ca 1 (07:00-15:00)' },
  { id: 'S-004', name: 'Phạm Văn D', role: 'Bốc xếp', shift: 'Ca 2 (15:00-23:00)' },
  { id: 'S-005', name: 'Hoàng Thị E', role: 'QC', shift: 'Ca 2 (15:00-23:00)' },
  { id: 'S-006', name: 'Đặng Văn F', role: 'Bảo trì', shift: 'Ca 3 (23:00-07:00)' },
  { id: 'S-007', name: 'Võ Thị G', role: 'Vận hành', shift: 'Ca 2 (15:00-23:00)' },
  { id: 'S-008', name: 'Ngô Văn H', role: 'Bốc xếp', shift: 'Ca 1 (07:00-15:00)' },
  { id: 'S-009', name: 'Phan Thị I', role: 'QC', shift: 'Ca 1 (07:00-15:00)' },
  { id: 'S-010', name: 'Bùi Văn K', role: 'Vận hành', shift: 'Ca 3 (23:00-07:00)' },
  { id: 'S-011', name: 'Lý Thị L', role: 'Kho', shift: 'Ca 2 (15:00-23:00)' },
  { id: 'S-012', name: 'Trương Văn M', role: 'An toàn', shift: 'Ca 1 (07:00-15:00)' }
];

export const mockLineStaff: LineStaffAssignment[] = [
  { lineId: 'LINE-2', staffIds: ['S-001', 'S-002'] },
  { lineId: 'LINE-3', staffIds: ['S-004', 'S-008'] },
  { lineId: 'LINE-4', staffIds: ['S-005'] },
  { lineId: 'LINE-5', staffIds: ['S-007', 'S-011'] }
];
