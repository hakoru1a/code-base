import { LoginResponse } from './response';
import { LogInRequest } from './request';
import { BaseService, getEndpoints } from '@services/core';

class Service extends BaseService {
  logIn = (payload: LogInRequest) => this.post<LogInRequest, LoginResponse>(getEndpoints().auth.logIn, payload);
}

export default new Service();
