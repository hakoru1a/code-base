import { routes } from '../routing';
import { lazy } from 'react';

const DashboardPage = lazy(() => import('@pages/dashboard'));
const SalesContractListPage = lazy(() => import('@pages/sales-contracts/list'));
const SalesContractDetailPage = lazy(() => import('@pages/sales-contracts/detail'));
const BusinessPlanListPage = lazy(() => import('@pages/business-plans/list'));
const BusinessPlanDetailPage = lazy(() => import('@pages/business-plans/detail'));
const WeighTicketListPage = lazy(() => import('@pages/weigh-tickets/list'));
const WeighTicketDetailPage = lazy(() => import('@pages/weigh-tickets/detail'));
const AssignVehiclesPage = lazy(() => import('@pages/production-assignments/assign-vehicles'));
const AssignStaffPage = lazy(() => import('@pages/production-assignments/assign-staff'));
const ProductionAssignmentListPage = lazy(() => import('@pages/production-assignments/list'));

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
  },
  {
    path: routes.dashboard.weighTickets,
    element: <WeighTicketListPage />
  },
  {
    path: routes.dashboard.weighTicket.detail,
    element: <WeighTicketDetailPage />
  },
  {
    path: routes.dashboard.weighTicket.qc,
    element: <WeighTicketDetailPage />
  },
  {
    path: routes.dashboard.productionAssignments.list,
    element: <ProductionAssignmentListPage />
  },
  {
    path: routes.dashboard.productionAssignments.vehicles,
    element: <AssignVehiclesPage />
  },
  {
    path: routes.dashboard.productionAssignments.staff,
    element: <AssignStaffPage />
  }
];

export default dashboardRoutes;
