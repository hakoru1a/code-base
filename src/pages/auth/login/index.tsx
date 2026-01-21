import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { AuthContainer } from './components';
import { Form, FormField, useFormResolver } from '@forms';
import { schema, FormProps } from './schema';
import Button from '@mui/material/Button';
import { useAuth, useRouter, useToast } from '@hooks';
import { routes } from '@routes';

const DEFAULT_USERNAME = 'admin';
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
            <Typography variant="h3">Đăng nhập</Typography>
          </Stack>
        </Grid>
        <Grid size={12}>
          <Form onSubmit={handleSubmit} methods={form}>
            <Stack spacing={2}>
              <FormField.Text name="username" label="Tài khoản" required />

              <FormField.Text name="password" label="Mật khẩu" required type="password" />

              <Button variant="contained" color="primary" type="submit" disabled={isSubmitting} fullWidth sx={{ mt: 2 }}>
                {isSubmitting ? 'Đang đăng nhập...' : 'Đăng nhập'}
              </Button>
            </Stack>
          </Form>
        </Grid>
      </Grid>
    </AuthContainer>
  );
};

export default Login;
