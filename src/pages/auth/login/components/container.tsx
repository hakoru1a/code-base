import { ReactElement } from 'react';

import Grid from '@mui/material/Grid';
import Box from '@mui/material/Box';
import { Logo } from '@components';
import AuthFooter from './footer';
import AuthCard from './card';
import AuthBackground from './background';

interface Props {
  children: ReactElement;
}

const AuthContainer = ({ children }: Props) => {
  return (
    <Box sx={{ minHeight: '100vh' }}>
      <AuthBackground />
      <Grid container direction="column" justifyContent="flex-end" sx={{ minHeight: '100vh' }}>
        <Grid sx={{ px: 3, mt: 3 }} size={12}>
          <Logo />
        </Grid>
        <Grid size={12}>
          <Grid
            container
            justifyContent="center"
            alignItems="center"
            sx={{ minHeight: { xs: 'calc(100vh - 210px)', sm: 'calc(100vh - 134px)', md: 'calc(100vh - 132px)' } }}
          >
            <AuthCard>{children}</AuthCard>
          </Grid>
        </Grid>
        <Grid sx={{ p: 3 }} size={12}>
          <AuthFooter />
        </Grid>
      </Grid>
    </Box>
  );
};

export default AuthContainer;
