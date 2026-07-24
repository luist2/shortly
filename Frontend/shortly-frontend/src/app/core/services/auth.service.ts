import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map, retry, throwError, timer } from 'rxjs';
import { environment } from 'src/environments/environment';
import { jwtDecode } from 'jwt-decode';

import { UserDTO, UserResponse } from 'src/app/models/user.model';
import { TokenResponse, JwtPayload } from 'src/app/models/auth.model';
import { STORAGE_KEYS } from '../constants/storage-keys.constants';

const JWT_CLAIMS = {
  EMAIL: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  NAME_ID:
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
};

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private _accessToken: string | null = null;

  private currentUserSubject = new BehaviorSubject<string | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map((userId) => !!userId));

  private isInitializedSubject = new BehaviorSubject<boolean>(false);
  public isInitialized$ = this.isInitializedSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeState();
  }

  private initializeState(): void {
    this.refreshToken().subscribe({
      next: () => {
        this.isInitializedSubject.next(true);
      },
      error: () => {
        this.clearLocalState();
        this.isInitializedSubject.next(true);
      },
    });
  }

  register(userDTO: UserDTO): Observable<UserResponse> {
    return this.http.post<UserResponse>(
      `${this.apiUrl}/Auth/register`,
      userDTO,
      { withCredentials: true },
    );
  }

  login(userDTO: UserDTO): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/login`, userDTO, {
        withCredentials: true,
      })
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  refreshToken(): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/refresh-tokens`, {}, {
        withCredentials: true,
      })
      .pipe(
        retry({
          count: 1,
          delay: (error: HttpErrorResponse) => {
            if (error.status !== 429) {
              return throwError(() => error);
            }

            return timer(this.getRetryAfterMilliseconds(error));
          },
        }),
      )
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  private getRetryAfterMilliseconds(error: HttpErrorResponse): number {
    const retryAfter = Number(error.headers?.get('Retry-After'));

    if (!Number.isFinite(retryAfter) || retryAfter <= 0) {
      return 60_000;
    }

    return Math.ceil(retryAfter * 1_000);
  }

  getToken(): string | null {
    return this._accessToken;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUserId(): string | null {
    return this.currentUserSubject.value;
  }

  getUserEmail(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = jwtDecode<JwtPayload>(token);
      return payload.email || payload[JWT_CLAIMS.EMAIL] || null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  logout(callApi: boolean = true): void {
    const tokenSnapshot = this._accessToken;
    this.clearLocalState();

    if (!callApi) {
      return;
    }

    const headers = tokenSnapshot
      ? new HttpHeaders({ Authorization: `Bearer ${tokenSnapshot}` })
      : undefined;

    this.http
      .post(
        `${this.apiUrl}/Auth/logout`,
        {},
        { withCredentials: true, headers },
      )
      .subscribe({
        error: () => {
          // Local state is already cleared.
        },
      });
  }

  private clearLocalState(): void {
    this._accessToken = null;
    localStorage.removeItem(STORAGE_KEYS.USER_ID);
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    this.currentUserSubject.next(null);
  }

  private handleAuthenticationSuccess(tokenResponse: TokenResponse): void {
    this._accessToken = tokenResponse.accessToken;

    const userId = this.extractUserIdFromToken(tokenResponse.accessToken);
    if (userId) {
      this.currentUserSubject.next(userId);
      localStorage.setItem(STORAGE_KEYS.USER_ID, userId);
    }
  }

  private extractUserIdFromToken(token: string): string | null {
    try {
      const payload = jwtDecode<JwtPayload>(token);
      return payload.nameid || payload[JWT_CLAIMS.NAME_ID] || null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }
}
