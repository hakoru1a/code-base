import httpClient from '@libs/axios';
import { callApi, initializeRequest } from './utils';

import type { AxiosResponse } from '@libs/axios';
import type { ApiResult } from './api';

export default abstract class BaseService {
  protected async get<TResponse>(url: string, params?: DynamicObject, cancellation?: Dynamic) {
    const config = initializeRequest(params, cancellation);
    return await callApi<TResponse>(httpClient.get(url, config));
  }

  protected async getDownload(url: string, params?: DynamicObject, cancellation?: Dynamic) {
    const config = initializeRequest(params, cancellation);
    const response = await httpClient.get<Dynamic, AxiosResponse<Dynamic, Dynamic>>(url, {
      ...config,
      responseType: 'blob'
    });

    return response.data;
  }

  protected post<TRequest, TResponse>(
    url: string,
    payload: TRequest,
    params?: DynamicObject,
    cancellation?: Dynamic
  ): Promise<ApiResult<TResponse>>;

  protected post<TResponse>(
    url: string,
    payload?: DynamicObject,
    params?: DynamicObject,
    cancellation?: Dynamic
  ): Promise<ApiResult<TResponse>>;

  protected async post<TRequest, TResponse>(
    url: string,
    payload: TRequest | DynamicObject,
    params?: DynamicObject,
    cancellation?: Dynamic
  ) {
    const config = initializeRequest(params, cancellation);
    return await callApi<TResponse>(httpClient.post<TRequest, AxiosResponse<ApiResult<TResponse>, Dynamic>>(url, payload, config));
  }

  protected async postFormData<TRequest extends DynamicObject, TResponse>(
    url: string,
    payload: TRequest,
    params?: DynamicObject,
    cancellation?: Dynamic
  ) {
    const data = this.transferToFormData(payload);
    const config = initializeRequest(params, cancellation);
    return await callApi<TResponse>(httpClient.post<TRequest, AxiosResponse<ApiResult<TResponse>, Dynamic>>(url, data, config));
  }

  protected put<TRequest, TResponse>(url: string, payload: TRequest, params?: DynamicObject): Promise<ApiResult<TResponse>>;

  protected put<TResponse>(url: string, payload?: DynamicObject, params?: DynamicObject): Promise<ApiResult<TResponse>>;

  protected async put<TRequest, TResponse>(url: string, payload?: TRequest | DynamicObject, params?: DynamicObject) {
    const config = initializeRequest(params);
    return await callApi<TResponse>(httpClient.put<TRequest, AxiosResponse<ApiResult<TResponse>, Dynamic>>(url, payload, config));
  }

  protected async delete<TResponse>(url: string, params?: DynamicObject, payload?: DynamicObject) {
    const config = initializeRequest(params);

    return await callApi(
      httpClient.delete<DynamicObject, AxiosResponse<ApiResult<TResponse>, Dynamic>>(url, {
        ...config,
        data: payload
      })
    );
  }

  protected patch<TRequest, TResponse>(url: string, payload: TRequest, params?: DynamicObject): Promise<TResponse>;

  protected patch<TResponse>(url: string, payload?: DynamicObject, params?: DynamicObject): Promise<TResponse>;

  protected async patch<TRequest, TResponse>(url: string, payload: TRequest | DynamicObject, params?: DynamicObject) {
    const config = initializeRequest(params);

    const response = await httpClient.patch<TRequest, AxiosResponse<TResponse, Dynamic>>(url, payload, config);
    return response.data;
  }

  private transferToFormData(obj: DynamicObject) {
    if (!obj) {
      throw new Error('Object convert must not null');
    }

    const formData = new FormData();
    for (const [key, value] of Object.entries(obj)) {
      formData.append(key, value);
    }

    return formData;
  }
}
