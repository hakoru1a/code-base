import type { DatePickerFormat } from '@utils/helpers';
import type { ApproveDecision } from '@components';

import { SalesContractStatus } from './constants';

export type SalesContract = {
  id: string;
  contractCode: string;
  customerName: string;
  productName: string;
  salePrice: number;
  signedAt: DatePickerFormat;
  status: Exclude<SalesContractStatus, SalesContractStatus.All>;
  approvalDecision?: ApproveDecision | null;
  approvalNote?: string | null;
};

export type BusinessPlan = {
  id: string;
  planCode: string;
  input: string;
  output: string;
  createdAt: DatePickerFormat;
  createdBy: string;
  active: boolean;
};

export type SalesContractAdvancedSearch = {
  customerName?: string;
  productName?: string;
  salePrice?: string;
  signedFrom?: DatePickerFormat;
  signedTo?: DatePickerFormat;
};
