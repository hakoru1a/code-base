import { GuestGuard } from '@routes';
import { Outlet } from 'react-router-dom';

const GuestLayout = () => {
  return (
    <GuestGuard>
      <Outlet />
    </GuestGuard>
  );
};

export default GuestLayout;
