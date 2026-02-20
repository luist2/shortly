import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map, throwError } from 'rxjs';
import { environment } from 'src/environments/environment';
import { jwtDecode } from 'jwt-decode';

// Importar modelos y constantes
import { UserDTO, UserResponse } from 'src/app/models/user.model';
import {
  TokenResponse,
  RefreshTokenRequest,
  JwtPayload,
} from 'src/app/models/auth.model';
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

  // 1. Estado Reactivo
  private currentUserSubject = new BehaviorSubject<string | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map((userId) => !!userId));

  private isInitializedSubject = new BehaviorSubject<boolean>(false);
  public isInitialized$ = this.isInitializedSubject.asObservable();

  constructor(private http: HttpClient) {
    // Inicializar estado al cargar la app
    this.initializeState();
  }

  private initializeState(): void {
    // Intentar refrescar el token silenciosamente al iniciar
    this.refreshToken().subscribe({
      next: () => {
        this.isInitializedSubject.next(true);
      },
      error: () => {
        // Si falla el refresh (ej: cookie expirada), no hacemos logout explícito aquí
        // para evitar loops si el error es de red, pero marcamos como inicializado
        // y el usuario estará "no logueado" (accessToken null)
        this.clearLocalState(); // Asegurar estado limpio
        this.isInitializedSubject.next(true);
      },
    });
  }

  /**
   * Registra un nuevo usuario en el sistema.
   * @param userDTO - Datos del usuario a registrar. (email, password, etc.)
   * @returns Observable con la respuesta del servidor que incluye los detalles del usuario registrado.
   */
  register(userDTO: UserDTO): Observable<UserResponse> {
    return this.http.post<UserResponse>(
      `${this.apiUrl}/Auth/register`,
      userDTO,
      { withCredentials: true },
    );
  }

  /**
   * Inicia sesión con las credenciales del usuario.
   * @param userDTO - Datos del usuario para iniciar sesión. (email, password)
   * @returns Observable con la respuesta del servidor que incluye el token de acceso.
   */
  login(userDTO: UserDTO): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/login`, userDTO, {
        withCredentials: true,
      })
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  /**
   * Refresca el token de acceso utilizando el token de refresco en la cookie HttpOnly.
   * @returns Observable con la nueva respuesta de tokens del servidor.
   */
  refreshToken(): Observable<TokenResponse> {
    const userId = this.getUserIdFromStorage();

    // Si no hay usuario en storage, no podemos intentar refrescar (o al menos no sabemos quién es)
    // Aunque el endpoint pide userId en el body, podríamos guardarlo en memoria o storage.
    // Usamos localStorage.USER_ID porque ese sí lo persistimos para saber "quién" se supone que somos.
    if (!userId) {
      return throwError(() => new Error('No userId found in storage'));
    }

    const body: RefreshTokenRequest = {
      userId: userId,
      // refreshToken ya no se envía, va en cookie
    };

    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/refresh-tokens`, body, {
        withCredentials: true,
      })
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  /**
   * Obtiene el token de acceso en memoria.
   * @returns El token de acceso o null si no existe.
   */
  getToken(): string | null {
    return this._accessToken;
  }

  /**
   * Retorna true si el usuario está autenticado (es decir, si existe un token de acceso válido).
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  /**
   * Obtiene el userId actual (Snapshot del estado).
   */
  getUserId(): string | null {
    return this.currentUserSubject.value;
  }

  /**
   * Obtiene el email del usuario desde el JWT decodificado utilizando jwt-decode.
   */
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

  /**
   * Cierra la sesión del usuario llamando al endpoint de logout y limpiando el estado.
   */
  logout(callApi: boolean = true): void {
    const tokenSnapshot = this._accessToken;

    // Limpiar primero para reflejar logout inmediato en guards/UI y evitar race conditions de redireccion.
    this.clearLocalState();

    if (!callApi) {
      return;
    }

    // Intentar invalidar refresh token en backend; si falla, no se revierte estado local.
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
          // No-op: el estado local ya fue limpiado.
        },
      });
  }

  private clearLocalState(): void {
    this._accessToken = null;
    localStorage.removeItem(STORAGE_KEYS.USER_ID);
    // Ya no usamos ACCESS_TOKEN ni REFRESH_TOKEN en localStorage
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    this.currentUserSubject.next(null);
  }

  // --- Private Helpers ---

  private handleAuthenticationSuccess(tokenResponse: TokenResponse): void {
    this._accessToken = tokenResponse.accessToken;
    // No guardamos refresh token, va en cookie

    const userId = this.extractUserIdFromToken(tokenResponse.accessToken);
    if (userId) {
      this.currentUserSubject.next(userId);
      localStorage.setItem(STORAGE_KEYS.USER_ID, userId);
    }
  }

  private getUserIdFromStorage(): string | null {
    return localStorage.getItem(STORAGE_KEYS.USER_ID);
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
