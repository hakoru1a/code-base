import { TokenExchangeResponse } from './response';
import { BaseService, getEndpoints } from '@services/core';
import { localStorageHelper } from '@utils/helpers';
import { LocalStorageKey } from '@utils/constants';
import type { ApiResult } from '@services/core';
import type { UserProfile } from '@services/account';
import axiosServices from '@libs/axios';

class Service extends BaseService {
  private get apiUrl(): string {
    return import.meta.env.VITE_APP_API_URL || '';
  }

  private get frontendUrl(): string {
    return import.meta.env.VITE_APP_FRONTEND_URL || window.location.origin;
  }

  // Helper: Get auth headers
  private getAuthHeaders(): Record<string, string> {
    const accessToken = localStorageHelper.get(LocalStorageKey.ServiceToken);
    const tokenType = localStorageHelper.get(LocalStorageKey.TokenType) || 'Bearer';
    return {
      'Content-Type': 'application/json',
      ...(accessToken && { Authorization: `${tokenType} ${accessToken}` })
    };
  }

  // Helper: Fetch with error handling using axios
  private async fetchApi<T = unknown>(
    endpoint: string,
    method: 'GET' | 'POST' = 'POST',
    data?: unknown,
    headers?: Record<string, string>
  ): Promise<T> {
    try {
      const response = await axiosServices.request<T>({
        url: endpoint,
        method,
        data,
        headers: {
          'Content-Type': 'application/json',
          ...headers
        }
      });
      return response.data;
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } }; message?: string };
      const errorMessage = axiosError.response?.data?.message || axiosError.message || 'Request failed';
      throw new Error(errorMessage);
    }
  }

  // Redirect to Keycloak login
  login = () => {
    const returnUrl = `${this.frontendUrl}/auth/callback`;
    window.location.href = `${this.apiUrl}/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  };

  // Exchange code for tokens
  exchangeCode = (code: string): Promise<ApiResult<TokenExchangeResponse> | TokenExchangeResponse> => {
    return this.fetchApi<ApiResult<TokenExchangeResponse> | TokenExchangeResponse>(getEndpoints().auth.exchange, 'POST', { code });
  };

  // Refresh token
  refreshToken = (refreshToken: string) => {
    return this.fetchApi(getEndpoints().auth.refresh, 'POST', { refreshToken });
  };

  // Get user profile from API
  getProfile = (): Promise<ApiResult<UserProfile>> => {
    return this.fetchApi<ApiResult<UserProfile>>(getEndpoints().auth.profile, 'GET', undefined, this.getAuthHeaders());
  };

  // Logout
  logout = () => {
    const refreshToken = localStorageHelper.get(LocalStorageKey.RefreshToken);
    const userId = localStorageHelper.get(LocalStorageKey.UserId);

    const body: Record<string, string> = {
      refreshToken: refreshToken || ''
    };
    if (userId) {
      body.userId = userId;
    }

    return this.fetchApi(getEndpoints().auth.logout, 'POST', body, this.getAuthHeaders())
      .catch((err) => {
        // Log but don't throw - always continue with local logout
        console.log('Logout API call failed (continuing with local logout):', err);
      })
      .finally(() => {
        // Clear tokens and redirect to base (will trigger Keycloak redirect)
        this.clearTokens();
        window.location.href = '/';
      });
  };

  // Helper: Set tokens
  setTokens = (accessToken: string, refreshToken: string, tokenType: string = 'Bearer') => {
    localStorageHelper.set(LocalStorageKey.ServiceToken, accessToken);
    if (refreshToken) {
      localStorageHelper.set(LocalStorageKey.RefreshToken, refreshToken);
    }
    localStorageHelper.set(LocalStorageKey.TokenType, tokenType);
  };

  // Helper: Clear tokens
  clearTokens = () => {
    localStorageHelper.remove(LocalStorageKey.ServiceToken);
    localStorageHelper.remove(LocalStorageKey.RefreshToken);
    localStorageHelper.remove(LocalStorageKey.TokenType);
    localStorageHelper.remove(LocalStorageKey.UserId);
  };

  // Helper: Check if authenticated
  isAuthenticated = () => {
    return !!localStorageHelper.get(LocalStorageKey.ServiceToken);
  };
}

export default new Service();
