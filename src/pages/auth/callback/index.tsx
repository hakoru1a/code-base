import { useEffect } from 'react';
import { useRouter } from '@hooks';
import { routes } from '@routes';
import { authService } from '@services/auth';
import { localStorageHelper } from '@utils/helpers';
import { LocalStorageKey } from '@utils/constants';
import { LinearLoader } from '@components';
import axios from '@libs/axios';

const AuthCallback = () => {
  const router = useRouter();

  useEffect(() => {
    const handleCallback = async () => {
      try {
        const urlParams = new URLSearchParams(window.location.search);
        const code = urlParams.get('code');

        if (!code) {
          console.error('No authorization code found in callback URL');
          router.replace(routes.login);
          return;
        }

        // Exchange code for tokens
        const exchangeResponse = await authService.exchangeCode(code);
        const rawResult = ('data' in exchangeResponse && exchangeResponse.data) || exchangeResponse;
        const result = rawResult as DynamicObject;

        if (!result) {
          throw new Error('Invalid exchange response');
        }

        // Use camelCase only
        const accessToken = result.accessToken as string | undefined;
        const refreshToken = result.refreshToken as string | undefined;
        const tokenType = (result.tokenType as string | undefined) || 'Bearer';

        if (!accessToken) {
          throw new Error('No access token in response');
        }

        // Save tokens
        authService.setTokens(accessToken, refreshToken || '', tokenType);

        // Set axios default header
        axios.defaults.headers.common.Authorization = `${tokenType} ${accessToken}`;

        // Get user profile
        const { data: profileData } = await authService.getProfile();
        if (profileData) {
          // Save userId if available
          if (profileData.id) {
            localStorageHelper.set(LocalStorageKey.UserId, String(profileData.id));
          }
        }

        // Redirect to dashboard - auth context will pick up the token and set user
        window.location.href = routes.default;
      } catch (error) {
        console.error('Auth callback failed:', error);
        authService.clearTokens();
        router.replace(routes.login);
      }
    };

    handleCallback();
  }, [router]);

  return <LinearLoader />;
};

export default AuthCallback;
