import { createBrowserRouter, Navigate } from 'react-router-dom';
import { routes } from './routing.ts';
import { dashboardRoutes, commonRoutes, guestRoutes, userRoutes } from './children';
import { AuthLayout, GuestLayout } from '@layout';

export * from './routing';
export * from './components';

export const router = createBrowserRouter([
  {
    element: <GuestLayout />,
    children: [...guestRoutes]
  },
  ...commonRoutes,
  {
    path: routes.base,
    element: <Navigate to={routes.default} />
  },
  {
    element: <AuthLayout />,
    children: [...dashboardRoutes, ...userRoutes]
  },
  {
    path: '*',
    element: <Navigate to={routes.notFound} />
  }
]);
