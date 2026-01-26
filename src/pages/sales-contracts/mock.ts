import { SalesContractStatus } from './constants';
import type { BusinessPlan, SalesContract } from './types';

export const mockSalesContracts: SalesContract[] = [
  {
    id: 'SC-001',
    contractCode: 'HDB-2026-0001',
    customerName: 'Công ty An Phú',
    productName: 'Dăm gỗ keo',
    salePrice: 1250000,
    signedAt: '2026-01-10',
    status: SalesContractStatus.Active,
    approvalDecision: null,
    approvalNote: null
  },
  {
    id: 'SC-002',
    contractCode: 'HDB-2026-0002',
    customerName: 'Công ty Minh Long',
    productName: 'Viên nén sinh khối',
    salePrice: 2100000,
    signedAt: '2026-01-03',
    status: SalesContractStatus.Completed,
    approvalDecision: null,
    approvalNote: null
  },
  {
    id: 'SC-003',
    contractCode: 'HDB-2025-0128',
    customerName: 'Nhà máy giấy Hòa Bình',
    productName: 'Mùn cưa',
    salePrice: 760000,
    signedAt: '2025-12-20',
    status: SalesContractStatus.Stopped,
    approvalDecision: null,
    approvalNote: null
  },
  {
    id: 'SC-004',
    contractCode: 'HDB-2026-0003',
    customerName: 'Công ty Thịnh Vượng',
    productName: 'Gỗ bóc',
    salePrice: 1680000,
    signedAt: '2026-01-18',
    status: SalesContractStatus.Active,
    approvalDecision: null,
    approvalNote: null
  }
];

export const mockBusinessPlansByContractId: Record<string, BusinessPlan[]> = {
  'SC-001': [
    {
      id: 'PAKD-001-3',
      planCode: 'PAKD-001-0003',
      input: '500 tấn/tháng',
      output: '450 tấn/tháng',
      createdAt: '2026-01-15',
      createdBy: 'Nguyễn Văn A',
      active: true
    },
    {
      id: 'PAKD-001-2',
      planCode: 'PAKD-001-0002',
      input: '450 tấn/tháng',
      output: '410 tấn/tháng',
      createdAt: '2026-01-05',
      createdBy: 'Trần Thị B',
      active: false
    },
    {
      id: 'PAKD-001-1',
      planCode: 'PAKD-001-0001',
      input: '400 tấn/tháng',
      output: '370 tấn/tháng',
      createdAt: '2025-12-25',
      createdBy: 'Lê Văn C',
      active: false
    }
  ],
  'SC-002': [
    {
      id: 'PAKD-002-1',
      planCode: 'PAKD-002-0001',
      input: '300 tấn/tháng',
      output: '295 tấn/tháng',
      createdAt: '2026-01-02',
      createdBy: 'Nguyễn Văn A',
      active: true
    }
  ],
  'SC-003': [
    {
      id: 'PAKD-003-2',
      planCode: 'PAKD-003-0002',
      input: '200 tấn/tháng',
      output: '190 tấn/tháng',
      createdAt: '2025-12-10',
      createdBy: 'Trần Thị B',
      active: true
    },
    {
      id: 'PAKD-003-1',
      planCode: 'PAKD-003-0001',
      input: '180 tấn/tháng',
      output: '170 tấn/tháng',
      createdAt: '2025-11-20',
      createdBy: 'Lê Văn C',
      active: false
    }
  ],
  'SC-004': [
    {
      id: 'PAKD-004-1',
      planCode: 'PAKD-004-0001',
      input: '350 tấn/tháng',
      output: '330 tấn/tháng',
      createdAt: '2026-01-18',
      createdBy: 'Nguyễn Văn A',
      active: true
    }
  ]
};
