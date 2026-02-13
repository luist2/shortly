export interface TokenResponse {
  accessToken: string;
  refreshToken?: string;
}

export interface RefreshTokenRequest {
  userId: string;
  refreshToken?: string;
}

export interface JwtPayload {
  nameid?: string;
  email?: string;
  [key: string]: any;
}
