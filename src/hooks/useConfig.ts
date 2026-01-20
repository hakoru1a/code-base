import { ConfigContext } from '@contexts/config';
import { use } from 'react';

const useConfig = () => {
  return use(ConfigContext);
};

export default useConfig;
