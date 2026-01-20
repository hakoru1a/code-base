import { EntityWithName } from '../core';

export type UserProfile = EntityWithName<string> & {
  email?: string;
  avatar?: string;
  image?: string;
};
