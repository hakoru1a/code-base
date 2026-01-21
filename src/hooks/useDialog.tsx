import { DialogContext } from '@contexts/dialog';
import { use } from 'react';

const useDialog = () => {
  const context = use(DialogContext);

  if (!context) {
    throw new Error('useDialog must be used within DialogProvider');
  }

  return context;
};

export default useDialog;
