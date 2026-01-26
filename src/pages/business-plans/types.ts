import type { DatePickerFormat } from '@utils/helpers';
import type { ApproveDecision } from '@components';
import type { BusinessPlanStatus } from './constants';

export type BusinessPlan = {
  id: string;
  planCode: string;
  salesContractCode: string;
  outputTon: number;
  inputSummary: string;
  outputSummary: string;
  createdAt: DatePickerFormat;
  status: Exclude<BusinessPlanStatus, BusinessPlanStatus.All>;
  approvalDecision?: ApproveDecision | null;
  approvalNote?: string | null;
  startDate?: DatePickerFormat;
  endDate?: DatePickerFormat;
};

export type BusinessPlanDetail = {
  id: string;
  planCode: string;
  salesContractCode: string;
  qualityCommitment: string;
  outputTon: number;
  params: BusinessPlanParams;
  selectedLines: ProductionLineMaster[];
  computed?: BusinessPlanComputed | null;
  startDate?: DatePickerFormat;
  endDate?: DatePickerFormat;
};

export type BusinessPlanAdvancedSearch = {
  salesContractCode?: string;
  outputFrom?: string;
  outputTo?: string;
};

export type ProductionLineMaster = {
  id: string;
  code: string;
  name: string;
  capacityTonPerDay: number;
  efficiency: number; // 0..1
  motors: number;
  staff: number;
};

export type BusinessPlanParams = {
  conversionRate: number; // 0..1 (Output/Input)
  moisturePercent: number; // 0..100
  treeAge: number; // years
  seasonFactor: number; // e.g. 0.9..1.2
  fixedCosts: {
    logistic: number;
    customs: number;
    finance: number;
    management: number;
  };
  indirectCosts: {
    electricity: number;
    vehicle: number;
    hr: number;
  };
};

export type BusinessPlanComputed = {
  ageFactor: number;
  requiredInputTon: number;
  totalMotors: number;
  totalStaff: number;
  totalEffectiveCapacityTonPerDay: number;
  timelineDays: number;
  requiredTonPerDay: number;
  electricityCostVnd: number;
  totalFixedCostVnd: number;
  totalIndirectCostVnd: number;
  totalCostVnd: number;
};
