import { BaseService, getEndpoints } from '@services/core';
import { UserProfile } from './response';

class Service extends BaseService {
  getProfile = () => this.get<UserProfile>(getEndpoints().account.getProfile);
}

export default new Service();
