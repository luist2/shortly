import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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
   * Inicia sesión de un usuario.
   * @param userDTO - Datos del usuario para iniciar sesión. (email, password)
   * @returns Observable con la respuesta del servidor que incluye tokens de acceso y refresh
   */
  login(userDTO: UserDTO): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/Auth/login`, userDTO);
  }

  /**
   * Refresca los tokens del usuario autenticado.
   * @param request Objeto con userId y refreshToken.
   * @returns Observable con los nuevos tokens.
   */
  refreshToken(
    refreshTokenRequest: RefreshTokenRequest
  ): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(
      `${this.apiUrl}/Auth/refresh-tokens`,
      refreshTokenRequest
    );
  }
}
