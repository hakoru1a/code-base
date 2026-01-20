import { UserProfile } from '@services/account';

export type LoginResponse = {
  token?: {
    accessToken: string;
  };
  user?: UserProfile;
};
