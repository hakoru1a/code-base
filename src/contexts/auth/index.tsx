import React, { createContext, useEffect, useReducer } from 'react';

import axios from '@libs/axios';
import { authService } from '@services/auth';
import { LocalStorageKey } from '@utils/constants';
import { localStorageHelper } from '@utils/helpers';
import { jwtDecode } from 'jwt-decode';
import { LOGIN, LOGOUT } from './actions';
import authReducer from './authReducer';
import type { AuthContextType, AuthProps } from './types';

// constant
const initialState: AuthProps = {
  isLoggedIn: false,
  isInitialized: false,
  user: null
};

const verifyToken: (st: string) => boolean = (serviceToken) => {
  if (!serviceToken) {
    return false;
  }
  try {
    const decoded: KeyedObject = jwtDecode(serviceToken);
    /**
     * Property 'exp' does not exist on type '<T = unknown>(token: string, options?: JwtDecodeOptions | undefined) => T'.
     */
    return decoded.exp > Date.now() / 1000;
  } catch (error) {
    return false;
  }
};

const setSession = (serviceToken?: string | null) => {
  if (serviceToken) {
    const tokenType = localStorageHelper.get(LocalStorageKey.TokenType) || 'Bearer';
    localStorageHelper.set(LocalStorageKey.ServiceToken, serviceToken);
    axios.defaults.headers.common.Authorization = `${tokenType} ${serviceToken}`;
  } else {
    localStorageHelper.remove(LocalStorageKey.ServiceToken);
    localStorageHelper.remove(LocalStorageKey.RefreshToken);
    localStorageHelper.remove(LocalStorageKey.TokenType);
    delete axios.defaults.headers.common.Authorization;
  }
};

// ==============================|| JWT CONTEXT & PROVIDER ||============================== //

export const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: React.ReactElement }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  useEffect(() => {
    const init = async () => {
      try {
        const serviceToken = localStorageHelper.get(LocalStorageKey.ServiceToken);
        if (serviceToken && verifyToken(serviceToken)) {
          setSession(serviceToken);
          // Get user profile from API
          const { data: user } = await authService.getProfile();

          if (user) {
            // Save userId if available
            if (user.id) {
              localStorageHelper.set(LocalStorageKey.UserId, String(user.id));
            }

            dispatch({
              type: LOGIN,
              payload: {
                isLoggedIn: true,
                user
              }
            });
          } else {
            dispatch({
              type: LOGOUT
            });
          }
        } else {
          dispatch({
            type: LOGOUT
          });
        }
      } catch (err) {
        console.error(err);
        authService.clearTokens();
        dispatch({
          type: LOGOUT
        });
      }
    };

    init();
  }, []);

  const login = () => {
    authService.login();
  };

  const logout = async () => {
    try {
      // Service logout will handle API call, clear tokens, and redirect
      await authService.logout();
    } catch (error) {
      console.error('Logout error:', error);
      // Even if API fails, clear local state and redirect
      authService.clearTokens();
      setSession(null);
      dispatch({ type: LOGOUT });
      window.location.href = '/';
    }
  };

  return <AuthContext value={{ ...state, login, logout }}>{children}</AuthContext>;
};
