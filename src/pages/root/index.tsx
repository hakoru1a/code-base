import { useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@hooks';
import { routes } from '@routes';
import { LinearLoader } from '@components';

/**
 * Root page component that handles authentication-based routing
 * - Redirects to Keycloak login if not authenticated
 * - Redirects to dashboard if already authenticated
 */
const RootPage = () => {
  const { isLoggedIn, isInitialized, login } = useAuth();

  useEffect(() => {
    // Auto-redirect to Keycloak login if not authenticated
    if (isInitialized && !isLoggedIn) {
      login();
    }
  }, [isInitialized, isLoggedIn, login]);

  // Show loading while auth is initializing
  if (!isInitialized) {
    return <LinearLoader />;
  }

  // If authenticated, redirect to dashboard
  if (isLoggedIn) {
    return <Navigate to={routes.default} replace />;
  }

  // Show loading while redirecting to Keycloak
  return <LinearLoader />;
};

export default RootPage;
