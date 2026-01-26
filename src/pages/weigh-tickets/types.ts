import type { DatePickerFormat } from '@utils/helpers';
import type { WeighTicketStatus, WeighTicketType, PaymentStatus } from './constants';

export type WeighTicketCustomerInfo = {
  customerName: string;
  phoneNumber: string;
  payeeName: string;
  bankAccountNumber: string;
  bankName: string;
};

export type WeighTicketVehicleGoodsInfo = {
  vehiclePlate: string;
  goodsType: string;
  fscCategory: string;
  originProfile: string;
  ticketType: WeighTicketType;
};

export type WeighTicketWeighingRow = {
  weighInKg: number;
  weighInAt: DatePickerFormat;
  weighOutKg: number | null;
  weighOutAt: DatePickerFormat;
  actualWeightKg: number;
  impurityPercent: number;
  payableWeightKg: number;
};

export type WeighTicketPaymentRow = {
  referenceUnitPriceVnd: number;
  referenceAmountVnd: number;
  actualUnitPriceVnd: number;
  actualAmountVnd: number;
  status: PaymentStatus;
  paidAt: DatePickerFormat;
  invoiceImageUrl?: string | null;
};

export type WeighTicketQc = {
  qcDecision: 'PASS' | 'FAIL' | null;
  qcComment: string;
  qcImages: string[];
  updatedAt: DatePickerFormat;
  updatedBy: string;
};

export type WeighTicket = {
  id: string;
  ticketCode: string;
  status: Exclude<WeighTicketStatus, WeighTicketStatus.All>;
  ticketType: WeighTicketType;

  customer: WeighTicketCustomerInfo;
  vehicleGoods: WeighTicketVehicleGoodsInfo;
  weighings: WeighTicketWeighingRow[];
  payments: WeighTicketPaymentRow[];
  qc: WeighTicketQc;

  createdAt: DatePickerFormat;
};

export type WeighTicketAdvancedSearch = {
  ticketType?: WeighTicketType | null;
  weighInFrom?: DatePickerFormat;
  weighInTo?: DatePickerFormat;
};
