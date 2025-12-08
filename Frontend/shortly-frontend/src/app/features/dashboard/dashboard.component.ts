import { Component, OnInit } from '@angular/core';
import { UrlService } from 'src/app/core/services/url.service';
import { ShortUrlResponse } from 'src/app/models/short-url.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  // Columnas de la tabla
  displayedColumns: string[] = [
    'shortUrl',
    'originalUrl',
    'clickCount',
    'createdAt',
    'actions',
  ];

  // Datos de la tabla
  dataSource: ShortUrlResponse[] = [];

  // Estado de carga
  isLoading = false;

  // Control de errores
  errorMessage: string | null = null;

  constructor(private urlService: UrlService) {}

  ngOnInit(): void {
    this.loadUserUrls();
  }

  /**
   * Carga las URLs del usuario desde el backend.
   */
  loadUserUrls(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.urlService.getUserUrls().subscribe({
      next: (urls) => {
        this.dataSource = urls;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading user URLs:', error);
        this.errorMessage = 'Failed to load URLs. Please try again later.';
        this.isLoading = false;
        this.dataSource = [];
      },
    });
  }

  /**
   * Verifica si hay URLs para mostrar.
   */
  get hasUrls(): boolean {
    return this.dataSource.length > 0;
  }

  /**
   * Verifica si se debe mostrar el estado vacío.
   */
  get shouldShowEmptyState(): boolean {
    return !this.isLoading && !this.errorMessage && !this.hasUrls;
  }
}
