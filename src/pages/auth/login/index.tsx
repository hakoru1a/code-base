import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import { AuthContainer } from './components';
import { Form, FormField, useFormResolver } from '@forms';
import { schema, FormProps } from './schema';
import { useAuth, useRouter, useToast } from '@hooks';
import { routes } from '@routes';
import { locales } from '@locales';
import { Button, Text } from '@components';

const DEFAULT_USERNAME = 'admin@biomass.com';
const DEFAULT_PASSWORD = 'lp0zjzint58yt9evieyz6zzlu80vx06z';

const Login = () => {
  const toast = useToast();
  const { trustLogin } = useAuth();
  const router = useRouter();

  const form = useFormResolver<FormProps>(schema, {
    defaultValues: {
      username: DEFAULT_USERNAME,
      password: DEFAULT_PASSWORD
    }
  });

  const {
    formState: { isSubmitting }
  } = form;

  const handleSubmit = async (data: FormProps) => {
    if (data.password === DEFAULT_PASSWORD && data.username === DEFAULT_USERNAME) {
      await trustLogin();
      router.push(routes.base);
      return;
    }

    toast.error('Tài khoản hoặc mật khẩu không đúng');
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
          <Form onSubmit={handleSubmit} methods={form}>
            <Stack spacing={2}>
              <FormField.Text name="username" label={locales.common.field.email} required />

              <FormField.Text name="password" label={locales.common.field.password} required type="password" />

              <Button.Base
                variant="contained"
                color="primary"
                type="submit"
                disabled={isSubmitting}
                fullWidth
                sx={{ mt: 2 }}
                label={locales.page.login.buttons.login}
              />
            </Stack>
          </Form>
        </Grid>
      </Grid>
    </AuthContainer>
  );
};

export default Login;
