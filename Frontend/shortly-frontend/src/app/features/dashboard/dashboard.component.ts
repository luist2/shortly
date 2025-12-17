import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Clipboard } from '@angular/cdk/clipboard';

// Servicios
import { UrlService } from 'src/app/core/services/url.service';

// Modelos
import { ShortUrlResponse } from 'src/app/models/short-url.model';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import { MatSort } from '@angular/material/sort';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  @ViewChild(MatSort)
  set matSort(sort: MatSort) {
    this.dataSource.sort = sort;
  }

  // Columnas de la tabla
  displayedColumns: string[] = [
    'shortUrl',
    'originalUrl',
    'clickCount',
    'createdAt',
    'actions',
  ];

  // Datos de la tabla
  dataSource = new MatTableDataSource<ShortUrlResponse>([]);

  // Estado de carga
  isLoading = false;

  // Control de errores
  errorMessage: string | null = null;

  // Control de busqueda
  searchTerm: string = '';

  constructor(
    private urlService: UrlService,
    private snackBar: MatSnackBar,
    private router: Router,
    private dialog: MatDialog,
    private clipboard: Clipboard
  ) {}

  ngOnInit(): void {
    this.loadUserUrls();
    this.setupFilter();

    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'createdAt':
          return new Date(item.createdAt).getTime();
        default:
          return (item as any)[property];
      }
    };
  }

  /**
   * Configura el filtro personalizado para el datasource.
   */
  private setupFilter(): void {
    this.dataSource.filterPredicate = (
      data: ShortUrlResponse,
      filter: string
    ): boolean => {
      const searchStr = filter.toLowerCase();

      // Buscar en shortUrl, originalUrl y shortCode
      const matchesShortUrl = data.shortUrl.toLowerCase().includes(searchStr);
      const matchesOriginalUrl = data.originalUrl
        .toLowerCase()
        .includes(searchStr);
      const matchesShortCode = data.shortCode.toLowerCase().includes(searchStr);
      const matchesClicks = data.clickCount.toString().includes(searchStr);

      return (
        matchesShortUrl ||
        matchesOriginalUrl ||
        matchesShortCode ||
        matchesClicks
      );
    };
  }

  /**
   * Carga las URLs del usuario desde el backend.
   */
  loadUserUrls(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.urlService.getUserUrls().subscribe({
      next: (urls) => {
        this.dataSource.data = urls;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading user URLs:', error);
        this.errorMessage = 'Failed to load URLs. Please try again later.';
        this.isLoading = false;
      },
    });
  }

  /**
   * Aplica el filtro de búsqueda al datasource.
   */
  applyFilter(): void {
    this.dataSource.filter = this.searchTerm.trim().toLowerCase();
  }

  /**
   * Limpia el término de búsqueda y el filtro del datasource.
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.dataSource.filter = '';
  }

  /**
   * Verifica si hay un filtro activo.
   */
  get hasActiveFilter(): boolean {
    return this.searchTerm.trim().length > 0;
  }

  /**
   * Obtiene el conteo de resultados filtrados.
   */
  get filteredResultsCount(): number {
    return this.dataSource.filteredData.length;
  }

  /**
   * Copia la URL acortada al portapapeles.
   * @param shortUrl - URL acortada a copiar.
   */
  copyToClipboard(shortUrl: string): void {
    const success = this.clipboard.copy(shortUrl);

    if (success) {
      this.snackBar.open('✓ URL copied to clipboard!', 'Close', {
        duration: 3000,
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: ['success-snackbar'],
      });
    } else {
      this.snackBar.open('Failed to copy URL', 'Close', {
        duration: 3000,
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
        panelClass: ['error-snackbar'],
      });
    }
  }

  /**
   * Navega a la página de estadísticas de la URL acortada.
   * @param shortCode - Código corto de la URL.
   */
  viewStats(shortCode: string): void {
    this.router.navigate(['/urls', shortCode, 'stats']);
  }

  /**
   * Abre un diálogo de confirmación para eliminar la URL.
   * @param url - URL a eliminar.
   */
  deleteUrl(url: ShortUrlResponse): void {
    const dialogData: ConfirmDialogData = {
      title: 'Delete URL',
      message: `Are you sure you want to delete this shortened URL? This action cannot be undone.\n\nShort URL: ${url.shortUrl}`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      confirmColor: 'warn',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData,
      disableClose: false,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result === true) {
        this.performDelete(url);
      }
    });
  }

  /**
   * Realiza la eliminación de la URL y maneja la respuesta.
   * @param url - URL a eliminar.
   */
  private performDelete(url: ShortUrlResponse): void {
    this.urlService.deleteUrl(url.shortCode).subscribe({
      next: () => {
        // Eliminar de la tabla
        const currentData = this.dataSource.data;
        this.dataSource.data = currentData.filter(
          (u) => u.shortCode !== url.shortCode
        );

        // Mostrar mensaje de éxito
        this.snackBar.open('✓ URL deleted successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
      },
      error: (error) => {
        console.error('Error deleting URL:', error);

        // Mostrar mensaje de error
        this.snackBar.open('Failed to delete URL. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  /**
   * Verifica si hay URLs para mostrar.
   */
  get hasUrls(): boolean {
    return this.dataSource.data.length > 0;
  }

  /**
   * Verifica si se debe mostrar el estado vacío.
   */
  get shouldShowEmptyState(): boolean {
    return !this.isLoading && !this.errorMessage && !this.hasUrls;
  }

  get hasNoSearchResults(): boolean {
    return (
      this.hasActiveFilter && this.hasUrls && this.filteredResultsCount === 0
    );
  }
}
