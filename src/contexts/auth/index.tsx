import React, { createContext, useEffect, useReducer } from 'react';

import { Chance } from 'chance';
import { jwtDecode } from 'jwt-decode';
import type { AuthContextType, AuthProps } from './types';
import axios from '@libs/axios';
import { LOGIN, LOGOUT } from './actions';
import authReducer from './authReducer';
import { LinearLoader } from '@components';
import { authService } from '@services/auth';
import { accountService } from '@services/account';

const chance = new Chance();

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
  const decoded: KeyedObject = jwtDecode(serviceToken);
  /**
   * Property 'exp' does not exist on type '<T = unknown>(token: string, options?: JwtDecodeOptions | undefined) => T'.
   */
  return decoded.exp > Date.now() / 1000;
};

const setSession = (serviceToken?: string | null) => {
  if (serviceToken) {
    localStorage.setItem('serviceToken', serviceToken);
    axios.defaults.headers.common.Authorization = `Bearer ${serviceToken}`;
  } else {
    localStorage.removeItem('serviceToken');
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
        const serviceToken = window.localStorage.getItem('serviceToken');
        if (serviceToken && verifyToken(serviceToken)) {
          setSession(serviceToken);
          const response = await accountService.getProfile();
          const user = response.data;
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
      } catch (err) {
        console.error(err);
        dispatch({
          type: LOGOUT
        });
      }
    };

    init();
  }, []);

  const trustLogin = async () => {
    const fakeToken = chance.string({ length: 32, pool: 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789' });
    setSession(fakeToken);
    dispatch({
      type: LOGIN,
      payload: {
        isLoggedIn: true,
        user: {
          id: 'trusted-user',
          name: 'Trusted User',
          email: ''
        }
      }
    });
  };

  const login = async (email: string, password: string) => {
    const response = await authService.logIn({ username: email, password });
    setSession(response.data?.token?.accessToken || '');
    dispatch({
      type: LOGIN,
      payload: {
        isLoggedIn: true
      }
    });
  };

  const logout = async () => {
    setSession(null);
    dispatch({ type: LOGOUT });
  };

  const updateProfile = () => {};

  if (state.isInitialized !== undefined && !state.isInitialized) {
    return <LinearLoader />;
  }

  return <AuthContext value={{ ...state, login, logout, updateProfile, trustLogin }}>{children}</AuthContext>;
};
