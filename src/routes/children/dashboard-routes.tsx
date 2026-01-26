import { routes } from '../routing';
import { lazy } from 'react';

const DashboardPage = lazy(() => import('@pages/dashboard'));
const SalesContractListPage = lazy(() => import('@pages/sales-contracts/list'));
const SalesContractDetailPage = lazy(() => import('@pages/sales-contracts/detail'));
const BusinessPlanListPage = lazy(() => import('@pages/business-plans/list'));
const BusinessPlanDetailPage = lazy(() => import('@pages/business-plans/detail'));

const dashboardRoutes = [
  {
    path: routes.dashboard.base,
    element: <DashboardPage />
  },
  {
    path: routes.dashboard.overview,
    element: <DashboardPage />
  },
  {
    path: routes.dashboard.sales,
    element: <SalesContractListPage />
  },
  {
    path: routes.dashboard.salesContract.create,
    element: <SalesContractDetailPage />
  },
  {
    path: routes.dashboard.salesContract.detail,
    element: <SalesContractDetailPage />
  },
  {
    path: routes.dashboard.salesContract.edit,
    element: <SalesContractDetailPage />
  },
  {
    path: routes.dashboard.businessPlans,
    element: <BusinessPlanListPage />
  },
  {
    path: routes.dashboard.businessPlan.create,
    element: <BusinessPlanDetailPage />
  },
  {
    path: routes.dashboard.businessPlan.detail,
    element: <BusinessPlanDetailPage />
  },
  {
    path: routes.dashboard.businessPlan.edit,
    element: <BusinessPlanDetailPage />
  }
];

export default dashboardRoutes;
