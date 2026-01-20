import { routes } from '../routing';
import { lazy } from 'react';

const LoginPage = lazy(() => import('@pages/auth/login'));

const guestRoutes = [
  {
    path: routes.login,
    element: <LoginPage />
  }
];

export default guestRoutes;
