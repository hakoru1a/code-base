import axios from 'axios';
import type { AxiosRequestConfig, AxiosResponse } from 'axios';

const axiosServices = axios.create({
  baseURL: import.meta.env.VITE_APP_API_URL || '' // DEFAULT_BACKEND_API_URL
});

axiosServices.interceptors.request.use(
  async (config) => {
    const accessToken = localStorage.getItem('serviceToken');
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
    if ((error.response?.status === 401 || error.response?.status === 403) && !window.location.href.includes('/login')) {
      localStorage.removeItem('serviceToken');

      // Clear axios default headers
      delete axiosServices.defaults.headers.common['Authorization'];

      // Redirect to login page
      window.location.href = '/login';
    }
    return Promise.reject((error.response && error.response.data) || 'Something went wrong');
  }
);

const cancelToken = axios.CancelToken;

export { cancelToken };

export type { AxiosResponse, AxiosRequestConfig };

export default axiosServices;
