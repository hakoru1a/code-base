import { createBrowserRouter, Navigate } from 'react-router-dom';
import { lazy } from 'react';
import { routes } from './routing.ts';
import { authRoutes, guestRoutes } from './children';
import { AuthLayout, GuestLayout } from '@layout';

const NotFoundPage = lazy(() => import('@pages/maintenance/404'));

export * from './routing';
export * from './components';

export const router = createBrowserRouter([
  {
    element: <GuestLayout />,
    children: [...guestRoutes]
  },
  {
    path: routes.notFound,
    element: <NotFoundPage />
  },
  {
    path: routes.base,
    element: <Navigate to={routes.default} />
  },
  {
    element: <AuthLayout />,
    children: [...authRoutes]
  },
  {
    path: '*',
    element: <Navigate to={routes.notFound} />
  }
]);
