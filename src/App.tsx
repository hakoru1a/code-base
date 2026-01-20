import { RouterProvider } from 'react-router-dom';
import { ThemeCustomization } from '@themes';
import { RtlLayout, ScrollTop, SnackBar } from '@components';
import { AuthProvider } from '@contexts/auth';
import { router } from '@routes';
import { ToastProvider } from '@contexts/toast';
import { LocalProvider } from '@locales';

const App = () => (
  <ThemeCustomization>
    <RtlLayout>
      <SnackBar>
        <LocalProvider>
          <ToastProvider>
            <AuthProvider>
              <ScrollTop>
                <RouterProvider router={router} />
              </ScrollTop>
            </AuthProvider>
          </ToastProvider>
        </LocalProvider>
      </SnackBar>
    </RtlLayout>
  </ThemeCustomization>
);

export default App;
