import { routes } from '../routing';
import { lazy } from 'react';

const DashboardPage = lazy(() => import('@pages/dashboard'));

const dashboardRoutes = [
  {
    path: routes.dashboard.base,
    element: <DashboardPage />
  }
];

export default dashboardRoutes;
