import { AuthContext } from '@contexts/auth';
import { use } from 'react';

export default function useAuth() {
  const context = use(AuthContext);

  if (!context) throw new Error('context must be use inside provider');

  return context;
}
