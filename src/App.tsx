import { RouterProvider } from 'react-router-dom';
import { ThemeCustomization } from '@themes';
import { RtlLayout, ScrollToTop, SnackBar } from '@components';
import { AuthProvider } from '@contexts/auth';
import { router } from '@routes';
import { ToastProvider } from '@contexts/toast';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DialogProvider } from '@contexts/dialog';

const App = () => (
  <ThemeCustomization>
    <RtlLayout>
      <SnackBar>
        <ToastProvider>
          <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="vi">
            <AuthProvider>
              <DialogProvider>
                <RouterProvider router={router} />
                <ScrollToTop />
              </DialogProvider>
            </AuthProvider>
          </LocalizationProvider>
        </ToastProvider>
      </SnackBar>
    </RtlLayout>
  </ThemeCustomization>
);

export default App;
