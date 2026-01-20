import type { AxiosRequestConfig, AxiosResponse } from 'axios';

import { cancelToken } from '@libs/axios';
import type { ApiResult } from './api';

export const BAD_REQUEST_ERROR_CODE = 400;

const createCancelToken = (executor: Dynamic) => (executor ? new cancelToken(executor) : cancelToken.source().token);

export const initializeRequest = (params?: DynamicObject, cancellation?: Dynamic): AxiosRequestConfig => {
  const config: AxiosRequestConfig = {
    params: { ...params }
  };

  if (cancellation) {
    config.cancelToken = createCancelToken(cancellation);
  }

  return config;
};

export const callApi = async <TResponse>(callback: Promise<AxiosResponse<ApiResult<TResponse>>>): Promise<ApiResult<TResponse>> => {
  try {
    const response = await callback;
    if (response.status >= BAD_REQUEST_ERROR_CODE) {
      return {
        status: response.status,
        error: response.data.error,
        success: false
      };
    }
    return {
      status: response.status,
      success: true,
      data: response?.data?.data
    };
  } catch (error: Dynamic) {
    return {
      ...error,
      data: undefined
    };
  }
};
