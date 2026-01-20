import React, { useEffect } from 'react';
import { useAuth, useRouter } from '@hooks';
import { routes } from '@routes';

type Props = {
  children: React.ReactNode;
};

const AuthGuard = ({ children }: Props) => {
  const { isLoggedIn, isInitialized } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (isInitialized && !isLoggedIn) {
      router.replace(routes.login, {
        from: location.pathname
      });
    }
  }, [isLoggedIn, isInitialized, router]);

  if (!isInitialized) {
    return null; // or a loading spinner
  }

  return children;
};

export default AuthGuard;
