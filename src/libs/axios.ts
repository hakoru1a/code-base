import type { AxiosRequestConfig, AxiosResponse } from 'axios';
import axios from 'axios';
import { HttpStatusCode, LocalStorageKey } from '@utils/constants';
import { localStorageHelper } from '@utils/helpers';
import { routes } from '@routes';

const axiosServices = axios.create({
  baseURL: import.meta.env.VITE_APP_API_URL || '' // DEFAULT_BACKEND_API_URL
});

axiosServices.interceptors.request.use(
  async (config) => {
    const accessToken = localStorageHelper.get(LocalStorageKey.ServiceToken);
    if (accessToken) {
      config.headers['Authorization'] = `Bearer ${accessToken}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

axiosServices.interceptors.response.use(
  (response) => response,
  (error) => {
    if (
      (error.response?.status === HttpStatusCode.Unauthorized || error.response?.status === HttpStatusCode.Forbidden) &&
      !window.location.href.includes(routes.login)
    ) {
      localStorageHelper.remove(LocalStorageKey.ServiceToken);

      // Clear axios default headers
      delete axiosServices.defaults.headers.common['Authorization'];

      // Redirect to login page
      window.location.href = routes.login;
    }
    return Promise.reject((error.response && error.response.data) || 'Something went wrong');
  }
);

const cancelToken = axios.CancelToken;

export { cancelToken };

export type { AxiosResponse, AxiosRequestConfig };

export default axiosServices;
