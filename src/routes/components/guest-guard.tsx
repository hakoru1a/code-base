import React, { useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { routes } from '@routes';

const isAuthenticated = false;

const GuestGuard = ({ children }: React.PropsWithChildren) => {
  const navigate = useNavigate();

  const check = useCallback(() => {
    if (isAuthenticated) {
      navigate(routes.default, { replace: true });
    }
  }, [navigate]);

  useEffect(() => {
    check();
  }, [check]);

  return <>{children}</>;
};

export default GuestGuard;
