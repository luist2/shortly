import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

// Importar modelos
import {
  CreateShortUrlRequest,
  ShortUrlResponse,
  ShortUrlStatsResponse,
} from 'src/app/models/short-url.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UrlService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Crea una nueva URL acortada.
   * @param originalUrl - URL original a acortar.
   * @returns Observable con la respuesta del servidor que incluye el shortCode y la URL acortada.
   */
  createShortUrl(originalUrl: string): Observable<ShortUrlResponse> {
    const body: CreateShortUrlRequest = { originalUrl };
    return this.http.post<ShortUrlResponse>(
      `${this.apiUrl}/UrlShortener/urls`,
      body
    );
  }

  /**
   * Obtiene todas las URLs acortadas del usuario autenticado.
   * @returns Observable con un array de URLs acortadas del usuario.
   */
  getUserUrls(): Observable<ShortUrlResponse[]> {
    return this.http.get<ShortUrlResponse[]>(
      `${this.apiUrl}/UrlShortener/urls`
    );
  }

  /**
   * Obtiene las estadísticas de una URL acortada específica.
   * @param shortCode - Código corto de la URL.
   * @returns Observable con las estadísticas de la URL acortada.
   */
  getUrlStats(shortCode: string): Observable<ShortUrlStatsResponse> {
    return this.http.get<ShortUrlStatsResponse>(
      `${this.apiUrl}/UrlShortener/urls/${shortCode}`
    );
  }

  /**
   * Elimina una URL acortada específica del usuario.
   * @param shortCode - Código corto de la URL a eliminar.
   * @returns Observable que indica la finalización de la operación.
   */
  deleteUrl(shortCode: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/UrlShortener/urls/${shortCode}`
    );
  }
}
