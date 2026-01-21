import { routes } from '../routing';
import { lazy } from 'react';

const ListUserPage = lazy(() => import('@pages/users/list'));

const userRoutes = [
  {
    path: routes.masterData.user.base,
    element: <ListUserPage />
  }
];

export default userRoutes;
