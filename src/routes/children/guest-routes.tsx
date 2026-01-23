import { routes } from '../routing';
import { lazy } from 'react';

const LoginPage = lazy(() => import('@pages/auth/login'));
const AuthCallbackPage = lazy(() => import('@pages/auth/callback'));

const guestRoutes = [
  {
    path: routes.login,
    element: <LoginPage />
  },
  {
    path: routes.auth.callback,
    element: <AuthCallbackPage />
  }
];

export default guestRoutes;
