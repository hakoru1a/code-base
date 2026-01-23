import { EntityWithName } from '../core';

export type UserProfile = EntityWithName<string> & {
  email?: string;
  avatarUrl?: string;
  fullName?: string;
  roles?: string[];
};
