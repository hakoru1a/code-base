import zod from 'zod';

export const schema = zod.object({
  username: zod.string().min(1, 'Username is required'),
  password: zod.string().min(6, 'Password must be at least 6 characters long')
});

export type FormProps = zod.infer<typeof schema>;
