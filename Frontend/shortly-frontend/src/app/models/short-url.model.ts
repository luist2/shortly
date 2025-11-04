export interface CreateShortUrlRequest {
  originalUrl: string;
}

export interface ShortUrlResponse {
  shortCode: string;
  shortUrl: string;
  originalUrl: string;
  createdAt: string; // ISO date string
  clickCount: number;
}

export interface ShortUrlStatsResponse {
  shortCode: string;
  originalUrl: string;
  clickCount: number;
  createdAt: string;
  lastAccessedAt?: string | null;
  expiresAt?: string | null;
  isActive: boolean;
}
