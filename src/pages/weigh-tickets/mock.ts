import { WeighTicketStatus, WeighTicketType, PaymentStatus } from './constants';
import type { WeighTicket } from './types';

export const mockWeighTickets: WeighTicket[] = [
  {
    id: 'WT-001',
    ticketCode: 'PC-2026-0001',
    status: WeighTicketStatus.Active,
    ticketType: WeighTicketType.In,
    customer: {
      customerName: 'Công ty An Phú',
      phoneNumber: '0901 234 567',
      payeeName: 'Nguyễn Văn A',
      bankAccountNumber: '0123456789',
      bankName: 'Vietcombank'
    },
    vehicleGoods: {
      vehiclePlate: '43C-123.45',
      goodsType: 'Dăm gỗ keo',
      fscCategory: 'FSC 100%',
      originProfile: 'HSNG-AP-2026-01',
      ticketType: WeighTicketType.In
    },
    weighings: [
      {
        weighInKg: 24200,
        weighInAt: '2026-01-26T07:42:00',
        weighOutKg: null,
        weighOutAt: null,
        actualWeightKg: 0,
        impurityPercent: 0,
        payableWeightKg: 0
      }
    ],
    payments: [
      {
        referenceUnitPriceVnd: 1180000,
        referenceAmountVnd: 0,
        actualUnitPriceVnd: 0,
        actualAmountVnd: 0,
        status: PaymentStatus.Unpaid,
        paidAt: null,
        invoiceImageUrl: null
      }
    ],
    qc: {
      qcDecision: null,
      qcComment: '',
      qcImages: [],
      updatedAt: null,
      updatedBy: ''
    },
    createdAt: '2026-01-26'
  },
  {
    id: 'WT-004',
    ticketCode: 'PC-2026-0004',
    status: WeighTicketStatus.Active,
    ticketType: WeighTicketType.In,
    customer: {
      customerName: 'Công ty Minh Long',
      phoneNumber: '0912 333 444',
      payeeName: 'Nguyễn Thị N',
      bankAccountNumber: '222233334444',
      bankName: 'Techcombank'
    },
    vehicleGoods: {
      vehiclePlate: '75C-567.89',
      goodsType: 'Dăm gỗ keo',
      fscCategory: 'FSC Mix',
      originProfile: 'HSNG-ML-2026-01',
      ticketType: WeighTicketType.In
    },
    weighings: [
      {
        weighInKg: 25500,
        weighInAt: '2026-01-26T06:58:00',
        weighOutKg: null,
        weighOutAt: null,
        actualWeightKg: 0,
        impurityPercent: 0,
        payableWeightKg: 0
      }
    ],
    payments: [
      {
        referenceUnitPriceVnd: 1200000,
        referenceAmountVnd: 0,
        actualUnitPriceVnd: 0,
        actualAmountVnd: 0,
        status: PaymentStatus.Unpaid,
        paidAt: null,
        invoiceImageUrl: null
      }
    ],
    qc: {
      qcDecision: null,
      qcComment: '',
      qcImages: [],
      updatedAt: null,
      updatedBy: ''
    },
    createdAt: '2026-01-26'
  },
  {
    id: 'WT-005',
    ticketCode: 'PC-2026-0005',
    status: WeighTicketStatus.Active,
    ticketType: WeighTicketType.In,
    customer: {
      customerName: 'Công ty Thịnh Vượng',
      phoneNumber: '0909 777 888',
      payeeName: 'Trần Văn P',
      bankAccountNumber: '1010101010',
      bankName: 'MB'
    },
    vehicleGoods: {
      vehiclePlate: '49C-222.11',
      goodsType: 'Mùn cưa',
      fscCategory: 'Controlled Wood',
      originProfile: 'HSNG-TV-2026-02',
      ticketType: WeighTicketType.In
    },
    weighings: [
      {
        weighInKg: 24010,
        weighInAt: '2026-01-26T09:12:00',
        weighOutKg: null,
        weighOutAt: null,
        actualWeightKg: 0,
        impurityPercent: 0,
        payableWeightKg: 0
      }
    ],
    payments: [
      {
        referenceUnitPriceVnd: 780000,
        referenceAmountVnd: 0,
        actualUnitPriceVnd: 0,
        actualAmountVnd: 0,
        status: PaymentStatus.Unpaid,
        paidAt: null,
        invoiceImageUrl: null
      }
    ],
    qc: {
      qcDecision: null,
      qcComment: '',
      qcImages: [],
      updatedAt: null,
      updatedBy: ''
    },
    createdAt: '2026-01-26'
  },
  {
    id: 'WT-002',
    ticketCode: 'PC-2026-0002',
    status: WeighTicketStatus.Completed,
    ticketType: WeighTicketType.In,
    customer: {
      customerName: 'Nhà máy giấy Hòa Bình',
      phoneNumber: '0908 111 222',
      payeeName: 'Trần Thị B',
      bankAccountNumber: '1234567890',
      bankName: 'BIDV'
    },
    vehicleGoods: {
      vehiclePlate: '92C-888.88',
      goodsType: 'Mùn cưa',
      fscCategory: 'Controlled Wood',
      originProfile: 'HSNG-HB-2026-01',
      ticketType: WeighTicketType.In
    },
    weighings: [
      {
        weighInKg: 28150,
        weighInAt: '2026-01-25T09:15:00',
        weighOutKg: 15320,
        weighOutAt: '2026-01-25T11:42:00',
        actualWeightKg: 12830,
        impurityPercent: 1.2,
        payableWeightKg: 12676
      }
    ],
    payments: [
      {
        referenceUnitPriceVnd: 760000,
        referenceAmountVnd: 9633760000,
        actualUnitPriceVnd: 750000,
        actualAmountVnd: 9507000000,
        status: PaymentStatus.Paid,
        paidAt: '2026-01-25',
        invoiceImageUrl: null
      }
    ],
    qc: {
      qcDecision: 'PASS',
      qcComment: 'Tạp chất thấp, đạt tiêu chuẩn nhập kho.',
      qcImages: [],
      updatedAt: '2026-01-25T10:05:00',
      updatedBy: 'QC - Lê Văn C'
    },
    createdAt: '2026-01-25'
  },
  {
    id: 'WT-003',
    ticketCode: 'PC-2026-0003',
    status: WeighTicketStatus.Cancelled,
    ticketType: WeighTicketType.Out,
    customer: {
      customerName: 'Công ty Thịnh Vượng',
      phoneNumber: '0932 456 789',
      payeeName: 'Lê Văn D',
      bankAccountNumber: '9999999999',
      bankName: 'ACB'
    },
    vehicleGoods: {
      vehiclePlate: '51D-456.78',
      goodsType: 'Viên nén sinh khối',
      fscCategory: 'FSC Mix',
      originProfile: 'HSNG-TV-2026-01',
      ticketType: WeighTicketType.Out
    },
    weighings: [
      {
        weighInKg: 19850,
        weighInAt: '2026-01-24T14:10:00',
        weighOutKg: 5200,
        weighOutAt: '2026-01-24T15:20:00',
        actualWeightKg: 14650,
        impurityPercent: 0.8,
        payableWeightKg: 14533
      }
    ],
    payments: [
      {
        referenceUnitPriceVnd: 2100000,
        referenceAmountVnd: 30519300000,
        actualUnitPriceVnd: 0,
        actualAmountVnd: 0,
        status: PaymentStatus.Unpaid,
        paidAt: null,
        invoiceImageUrl: null
      }
    ],
    qc: {
      qcDecision: 'FAIL',
      qcComment: 'Ảnh hàng hóa không rõ, cần kiểm tra lại hồ sơ.',
      qcImages: [],
      updatedAt: '2026-01-24T14:30:00',
      updatedBy: 'QC - Nguyễn Văn A'
    },
    createdAt: '2026-01-24'
  }
];
