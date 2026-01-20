import { routes } from '../routing';
import { lazy } from 'react';

const DashboardPage = lazy(() => import('@pages/dashboard'));

const authRoutes = [
  {
    path: routes.dashboard,
    element: <DashboardPage />
  }
];

export default authRoutes;
