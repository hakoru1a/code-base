export const routes = {
  base: '/',
  default: '/dashboard',
  login: '/login',
  auth: {
    callback: '/auth/callback'
  },
  demo: {
    base: '/demo'
  },
  notFound: '/not-found',
  error: {
    base: '/error',
    403: '/error/403',
    404: '/error/404',
    500: '/error/500'
  },
  masterData: {
    user: {
      base: '/master-data/user'
    }
  },
  dashboard: {
    base: '/dashboard',
    overview: '/dashboard/overview',
    sales: '/dashboard/sales'
  }
};
