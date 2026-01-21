import { routes } from '../routing';
import { lazy } from 'react';

const NotFoundPage = lazy(() => import('@pages/maintenance/404'));

const commonRoutes = [
  {
    path: routes.notFound,
    element: <NotFoundPage />
  }
];

export default commonRoutes;
