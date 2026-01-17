import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { jwtDecode } from 'jwt-decode';

// Importar modelos y constantes
import { UserDTO, UserResponse } from 'src/app/models/user.model';
import { TokenResponse, RefreshTokenRequest } from 'src/app/models/auth.model';
import { STORAGE_KEYS } from '../constants/storage-keys.constants';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;

  // 1. Estado Reactivo
  private currentUserSubject = new BehaviorSubject<string | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.currentUser$.pipe(map(userId => !!userId));

  constructor(private http: HttpClient) {
    // Inicializar estado al cargar la app
    this.initializeState();
  }

  private initializeState(): void {
    const userId = this.getUserIdFromStorage();
    if (userId && this.hasValidToken()) {
      this.currentUserSubject.next(userId);
    }
  }

  /**
   * Registra un nuevo usuario en el sistema.
   * @param userDTO - Datos del usuario a registrar. (email, password, etc.)
   * @returns Observable con la respuesta del servidor que incluye los detalles del usuario registrado.
   */
  register(userDTO: UserDTO): Observable<UserResponse> {
    return this.http.post<UserResponse>(
      `${this.apiUrl}/Auth/register`,
      userDTO
    );
  }

  /**
   * Inicia sesión con las credenciales del usuario.
   * @param userDTO - Datos del usuario para iniciar sesión. (email, password)
   * @returns Observable con la respuesta del servidor que incluye los tokens de autenticación.
   */
  login(userDTO: UserDTO): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/login`, userDTO)
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  /**
   * Refresca el token de acceso utilizando el token de refresco almacenado.
   * @param request - Objeto que contiene el userId y el refreshToken.
   * @returns Observable con la nueva respuesta de tokens del servidor.
   */
  refreshToken(): Observable<TokenResponse> {
    const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    const userId = localStorage.getItem(STORAGE_KEYS.USER_ID);

    if (!refreshToken || !userId) {
      this.logout(); // Limpiar estado si faltan datos
      throw new Error('No refresh token or userId found in localStorage');
    }

    const body: RefreshTokenRequest = {
      userId: userId,
      refreshToken: refreshToken,
    };

    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/refresh-tokens`, body)
      .pipe(tap((response) => this.handleAuthenticationSuccess(response)));
  }

  /**
   * Obtiene el token de acceso almacenado en el localStorage.
   * @returns El token de acceso o null si no existe.
   */
  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  }

  /**
   * Retorna true si el usuario está autenticado (es decir, si existe un token de acceso válido).
   * Es preferible leer directamente del storage para los Guards por si el token fue borrado manualmente.
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
      const payload: any = jwtDecode(token);
      return (
        payload.email ||
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
        null
      );
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Cierra la sesión del usuario eliminando los tokens almacenados y limpiando el estado.
   */
  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER_ID);
    this.currentUserSubject.next(null);
  }

  // --- Private Helpers ---

  private handleAuthenticationSuccess(tokenResponse: TokenResponse): void {
    this.saveTokens(tokenResponse);
    
    // Actualizar estado reactivo
    const userId = this.extractUserIdFromToken(tokenResponse.accessToken);
    this.currentUserSubject.next(userId);
  }

  private saveTokens(tokenResponse: TokenResponse): void {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, tokenResponse.accessToken);
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, tokenResponse.refreshToken);

    const userId = this.extractUserIdFromToken(tokenResponse.accessToken);
    if (userId) {
      localStorage.setItem(STORAGE_KEYS.USER_ID, userId);
    }
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    return token !== null && token.trim() !== '';
  }

  private getUserIdFromStorage(): string | null {
    return localStorage.getItem(STORAGE_KEYS.USER_ID);
  }

  private extractUserIdFromToken(token: string): string | null {
    try {
      const payload: any = jwtDecode(token);
      return (
        payload.nameid ||
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        null
      );
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }
}
