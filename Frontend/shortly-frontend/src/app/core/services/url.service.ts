import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

// Importar modelos
import {
  CreateShortUrlRequest,
  ShortUrlResponse,
  ShortUrlStatsResponse,
} from 'src/app/models/short-url.model';
import { PagedResult } from 'src/app/models/paged-result.model';

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
   * Obtiene todas las URLs acortadas del usuario autenticado con paginación.
   * @param page - Número de página (comienza en 1).
   * @param pageSize - Cantidad de elementos por página.
   * @returns Observable con el resultado paginado de URLs.
   */
  getUserUrls(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    sortBy?: string,
    sortDirection?: string,
    status?: string
  ): Observable<PagedResult<ShortUrlResponse>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }
    
    if (sortBy) {
        params = params.set('sortBy', sortBy);
    }

    if (sortDirection) {
        params = params.set('sortDirection', sortDirection);
    }

    if (status && status !== 'all') {
        params = params.set('status', status);
    }

    return this.http.get<PagedResult<ShortUrlResponse>>(
      `${this.apiUrl}/UrlShortener/urls`,
      { params }
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
