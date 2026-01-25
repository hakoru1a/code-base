import { ApiVersion } from './api';

export const getEndpoints = (version: ApiVersion = ApiVersion.v1) => ({
  auth: {
    logIn: `/${version}/auth/login`,
    exchange: `/auth/exchange`,
    refresh: `/auth/refresh`,
    profile: `/auth/profile`,
    logout: `/auth/logout`
  },
  account: {
    getProfile: `/${version}/account/profile`
  }
});
