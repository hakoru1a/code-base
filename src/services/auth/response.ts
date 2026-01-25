export type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
};

export type TokenExchangeResponse = {
  accessToken?: string;
  refreshToken?: string;
  tokenType?: string;
};

export type RefreshTokenResponse = {
  accessToken?: string;
  refreshToken?: string;
  tokenType?: string;
};

export type ExchangeCodeRequest = {
  code: string;
};

export type RefreshTokenRequest = {
  refreshToken: string;
};
