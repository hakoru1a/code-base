import { Button, Text } from '@components';
import { locales } from '@locales';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import { authService } from '@services/auth';
import { AuthContainer } from './components';

const Login = () => {
  const handleKeycloakLogin = () => {
    authService.login();
  };

  return (
    <AuthContainer>
      <Grid container spacing={3}>
        <Grid size={12}>
          <Stack direction="row" sx={{ alignItems: 'baseline', justifyContent: 'space-between', mb: { xs: -0.5, sm: 0.5 } }}>
            <Text.Typography variant="h3" label={locales.page.login.title} />
          </Stack>
        </Grid>
        <Grid size={12}>
          <Stack spacing={2}>
            <Button.Base
              variant="contained"
              color="primary"
              onClick={handleKeycloakLogin}
              fullWidth
              sx={{ mt: 2 }}
              label={locales.page.login.buttons.login}
            />
          </Stack>
        </Grid>
      </Grid>
    </AuthContainer>
  );
};

export default Login;
