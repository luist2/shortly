import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from 'src/environments/environment';

// Importar modelos
import { UserDTO, UserResponse } from 'src/app/models/user.model';
import { TokenResponse, RefreshTokenRequest } from 'src/app/models/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

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
      .pipe(tap((response) => this.saveTokens(response)));
  }

  /**
   * Refresca el token de acceso utilizando el token de refresco almacenado.
   * @param request - Objeto que contiene el userId y el refreshToken.
   * @returns Observable con la nueva respuesta de tokens del servidor.
   */
  refreshToken(): Observable<TokenResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    const userId = localStorage.getItem('userId');

    if (!refreshToken || !userId) {
      throw new Error('No refresh token or userId found in localStorage');
    }

    const body: RefreshTokenRequest = {
      userId: userId,
      refreshToken: refreshToken,
    };

    return this.http
      .post<TokenResponse>(`${this.apiUrl}/Auth/refresh-tokens`, body)
      .pipe(tap((response) => this.saveTokens(response)));
  }

  /**
   * Obtiene el token de acceso almacenado en el localStorage.
   * @returns El token de acceso o null si no existe.
   */
  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  /**
   * Retorna true si el usuario está autenticado (es decir, si existe un token de acceso válido).
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && token.trim() !== '';
  }

  /**
   * Obtiene el userId desde el JWT decodificado.
   * Busca el claim 'nameid' en el payload del token.
   */
  getUserId(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const payloadBase64 = token.split('.')[1];
      const payloadDecoded = JSON.parse(atob(payloadBase64));
      return payloadDecoded['nameid'] || null;
    } catch (error) {
      console.error('Error: ', error);
      return null;
    }
  }

  /**
   * Cierra la sesión del usuario eliminando los tokens almacenados.
   */
  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userId');
  }

  private saveTokens(tokenResponse: TokenResponse): void {
    localStorage.setItem('accessToken', tokenResponse.accessToken);
    localStorage.setItem('refreshToken', tokenResponse.refreshToken);

    // Extraer userId desde el JWT y guardarlo
    const userId = this.extractUserIdFromToken(tokenResponse.accessToken);
    if (userId) {
      localStorage.setItem('userId', userId);
    }
  }

  private extractUserIdFromToken(token: string): string | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['nameid'] || null;
    } catch (error) {
      console.error('Error: ', error);
      return null;
    }
  }
}
