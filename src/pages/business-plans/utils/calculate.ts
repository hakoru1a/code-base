import type { BusinessPlanComputed, BusinessPlanParams, ProductionLineMaster } from '../types';

const DEFAULT_KWH_PER_MOTOR_PER_HOUR = 15;
const DEFAULT_ELECTRICITY_PRICE_VND = 2500;
const DEFAULT_WORKING_HOURS_PER_DAY = 8;

const clampMin = (v: number, min: number) => (v < min ? min : v);

export const calculateBusinessPlan = ({
  outputTon,
  params,
  selectedLines
}: {
  outputTon: number;
  params: BusinessPlanParams;
  selectedLines: ProductionLineMaster[];
}): BusinessPlanComputed => {
  const safeOutput = clampMin(Number(outputTon) || 0, 0);

  const conversionRate = clampMin(Number(params.conversionRate) || 0, 0.0001);
  const moisture = clampMin(Number(params.moisturePercent) || 0, 0) / 100;
  const seasonFactor = clampMin(Number(params.seasonFactor) || 0, 0.0001);
  const treeAge = clampMin(Number(params.treeAge) || 0, 0);

  const ageFactor = 1 + Math.max(0, treeAge - 5) * 0.01;

  const requiredInputTon = (safeOutput / conversionRate) * (1 + moisture) * seasonFactor * ageFactor;

  const totalMotors = selectedLines.reduce((sum, l) => sum + (Number(l.motors) || 0), 0);
  const totalStaff = selectedLines.reduce((sum, l) => sum + (Number(l.staff) || 0), 0);
  const totalEffectiveCapacityTonPerDay = selectedLines.reduce(
    (sum, l) => sum + (Number(l.capacityTonPerDay) || 0) * (Number(l.efficiency) || 0),
    0
  );

  const safeCapacity = clampMin(totalEffectiveCapacityTonPerDay, 1);
  const timelineDays = Math.max(1, Math.ceil(safeOutput / safeCapacity));
  const requiredTonPerDay = safeOutput / timelineDays;

  const electricityCostVnd =
    totalMotors * DEFAULT_WORKING_HOURS_PER_DAY * timelineDays * DEFAULT_KWH_PER_MOTOR_PER_HOUR * DEFAULT_ELECTRICITY_PRICE_VND;

  const totalFixedCostVnd =
    (Number(params.fixedCosts.logistic) || 0) +
    (Number(params.fixedCosts.customs) || 0) +
    (Number(params.fixedCosts.finance) || 0) +
    (Number(params.fixedCosts.management) || 0);

  const totalIndirectCostVnd =
    (Number(params.indirectCosts.electricity) || 0) + (Number(params.indirectCosts.vehicle) || 0) + (Number(params.indirectCosts.hr) || 0);

  const totalCostVnd = totalFixedCostVnd + totalIndirectCostVnd;

  return {
    ageFactor,
    requiredInputTon,
    totalMotors,
    totalStaff,
    totalEffectiveCapacityTonPerDay,
    timelineDays,
    requiredTonPerDay,
    electricityCostVnd,
    totalFixedCostVnd,
    totalIndirectCostVnd,
    totalCostVnd
  };
};
