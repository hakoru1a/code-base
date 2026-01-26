import type { BusinessPlan, ProductionLineMaster, BusinessPlanParams } from './types';
import { BusinessPlanStatus } from './constants';

export const mockProductionLines: ProductionLineMaster[] = [
  { id: 'LINE-01', code: 'CH-01', name: 'Chuyền 01 - Sấy & nghiền', capacityTonPerDay: 45, efficiency: 0.9, motors: 6, staff: 10 },
  { id: 'LINE-02', code: 'CH-02', name: 'Chuyền 02 - Viên nén', capacityTonPerDay: 35, efficiency: 0.85, motors: 5, staff: 8 },
  { id: 'LINE-03', code: 'CH-03', name: 'Chuyền 03 - Đóng gói', capacityTonPerDay: 55, efficiency: 0.8, motors: 4, staff: 6 },
  { id: 'LINE-04', code: 'CH-04', name: 'Chuyền 04 - Dăm gỗ', capacityTonPerDay: 60, efficiency: 0.88, motors: 7, staff: 12 }
];

export const mockBusinessPlans: BusinessPlan[] = [
  {
    id: 'BP-001',
    planCode: 'PAKD-2026-0001',
    salesContractCode: 'HDB-2026-0001',
    outputTon: 450,
    inputSummary: 'Dăm gỗ 520 tấn',
    outputSummary: 'Viên nén 450 tấn',
    createdAt: '2026-01-15',
    status: BusinessPlanStatus.Active,
    approvalDecision: null,
    approvalNote: null,
    startDate: '2026-01-18',
    endDate: '2026-02-05'
  },
  {
    id: 'BP-002',
    planCode: 'PAKD-2026-0002',
    salesContractCode: 'HDB-2026-0002',
    outputTon: 295,
    inputSummary: 'Mùn cưa 330 tấn',
    outputSummary: 'Viên nén 295 tấn',
    createdAt: '2026-01-05',
    status: BusinessPlanStatus.Completed,
    approvalDecision: null,
    approvalNote: null,
    startDate: '2026-01-06',
    endDate: '2026-01-16'
  },
  {
    id: 'BP-003',
    planCode: 'PAKD-2025-0128',
    salesContractCode: 'HDB-2025-0128',
    outputTon: 190,
    inputSummary: 'Dăm gỗ 220 tấn',
    outputSummary: 'Dăm gỗ 190 tấn',
    createdAt: '2025-12-20',
    status: BusinessPlanStatus.Stopped,
    approvalDecision: null,
    approvalNote: null
  },
  {
    id: 'BP-004',
    planCode: 'PAKD-2026-0003',
    salesContractCode: 'HDB-2026-0003',
    outputTon: 330,
    inputSummary: 'Dăm gỗ 380 tấn',
    outputSummary: 'Viên nén 330 tấn',
    createdAt: '2026-01-20',
    status: BusinessPlanStatus.Active,
    approvalDecision: null,
    approvalNote: null,
    startDate: '2026-01-22',
    endDate: '2026-02-02'
  }
];

export const mockDefaultParams: BusinessPlanParams = {
  conversionRate: 0.9,
  moisturePercent: 12,
  treeAge: 6,
  seasonFactor: 1.05,
  fixedCosts: {
    logistic: 18000000,
    customs: 6000000,
    finance: 9000000,
    management: 7000000
  },
  indirectCosts: {
    electricity: 12000000,
    vehicle: 5000000,
    hr: 8000000
  }
};
