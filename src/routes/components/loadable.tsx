import { LinearLoader } from '@components';
import { ElementType, Suspense } from 'react';

const Loadable = (Component: ElementType) => (props: Dynamic) => {
  return (
    <Suspense fallback={<LinearLoader />}>
      <Component {...props} />
    </Suspense>
  );
};

export default Loadable;
