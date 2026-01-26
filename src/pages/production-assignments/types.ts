import type { DatePickerFormat } from '@utils/helpers';

export type ActiveVehicle = {
  ticketId: string;
  ticketCode: string;
  vehiclePlate: string;
  enteredFactoryAt: DatePickerFormat;
};

export type ProductionLineCard = {
  id: string; // "LINE-1"
  lineNo: number; // 1..9
  ticketId: string | null;
  ticketCode: string | null;
  startedAt: DatePickerFormat;
  endedAt: DatePickerFormat;
};

export type StaffCard = {
  id: string;
  name: string;
  role: string;
  shift: string;
};

export type LineStaffAssignment = {
  lineId: string;
  staffIds: string[];
};
