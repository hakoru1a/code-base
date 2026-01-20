import { ApiVersion } from './api';

export const getEndpoints = (version: ApiVersion = ApiVersion.v1) => ({
  auth: {
    logIn: `/${version}/auth/login`
  },
  account: {
    getProfile: `/${version}/account/profile`
  }
});
