import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Clipboard } from '@angular/cdk/clipboard';
import { MatSort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

// Servicios
import { UrlService } from 'src/app/core/services/url.service';

// Modelos
import { ShortUrlResponse } from 'src/app/models/short-url.model';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';

type UrlStatusFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild(MatSort)
  set matSort(sort: MatSort) {
    this.dataSource.sort = sort;
  }

  // Columnas de la tabla
  displayedColumns: string[] = [
    'status',
    'shortUrl',
    'originalUrl',
    'clickCount',
    'createdAt',
    'actions',
  ];

  // Datos de la tabla unificada
  dataSource = new MatTableDataSource<ShortUrlResponse>([]);

  // Todas las URLs sin filtrar
  allUrls: ShortUrlResponse[] = [];

  // Estado de carga
  isLoading = false;

  // Control de errores
  errorMessage: string | null = null;

  // Control de búsqueda
  searchTerm: string = '';
  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription | null = null;

  // Filtro de estado (activo/inactivo)
  statusFilter: UrlStatusFilter = 'all';

  // Paginación
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  pageSizeOptions = [5, 10, 25, 100];

  constructor(
    private urlService: UrlService,
    private snackBar: MatSnackBar,
    private router: Router,
    private dialog: MatDialog,
    private clipboard: Clipboard
  ) {}

  ngOnInit(): void {
    this.setupSearch();
    this.loadUserUrls();
    this.setupFilters();
    this.setupSorting();
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  private setupSearch(): void {
    this.searchSubscription = this.searchSubject
      .pipe(debounceTime(600), distinctUntilChanged())
      .subscribe((term) => {
        this.pageIndex = 0; // Reiniciar paginación en nueva búsqueda
        this.loadUserUrls();
      });
  }

  /**
   * Configura el sorting para el datasource.
   */
  private setupSorting(): void {
    this.dataSource.sortingDataAccessor = (
      item: ShortUrlResponse,
      property: string
    ) => {
      switch (property) {
        case 'createdAt':
          return new Date(item.createdAt).getTime();
        case 'status':
          return item.isActive ? 0 : 1; // Activos primero
        default:
          return (item as any)[property];
      }
    };
  }

  /**
   * Configura el filtro personalizado para el datasource.
   * Nota: Ahora solo filtra por estado en el cliente (para la página actual)
   * ya que la búsqueda se hace en el servidor.
   */
  private setupFilters(): void {
    this.dataSource.filterPredicate = (
      data: ShortUrlResponse,
      filter: string
    ): boolean => {
      const filterObj = JSON.parse(filter);
      const status = filterObj.status;

      // Filtro por estado
      if (status === 'active') {
        return data.isActive;
      } else if (status === 'inactive') {
        return !data.isActive;
      }
      
      return true;
    };
  }

  /**
   * Carga las URLs del usuario desde el backend.
   */
  loadUserUrls(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // MatPaginator usa índice 0, el backend usa índice 1
    const page = this.pageIndex + 1;

    this.urlService.getUserUrls(page, this.pageSize, this.searchTerm).subscribe({
      next: (result) => {
        this.allUrls = result.items;
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        
        // Si la página actual está vacía y no es la primera, volver a la anterior
        if (this.allUrls.length === 0 && this.pageIndex > 0) {
          this.pageIndex--;
          this.loadUserUrls();
          return;
        }

        this.applyFilters();
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
   * Maneja el cambio de página.
   */
  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadUserUrls();
  }

  /**
   * Aplica todos los filtros combinados.
   */
  private applyFilters(): void {
    const filterValue = JSON.stringify({
      search: this.searchTerm.trim().toLowerCase(),
      status: this.statusFilter,
    });
    this.dataSource.filter = filterValue;
  }

  /**
   * Aplica el filtro de búsqueda.
   */
  applySearchFilter(): void {
    this.searchSubject.next(this.searchTerm.trim());
  }

  /**
   * Cambia el filtro de estado.
   */
  changeStatusFilter(status: UrlStatusFilter): void {
    this.statusFilter = status;
    this.applyFilters();
  }

  /**
   * Limpia el término de búsqueda y el filtro.
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.searchSubject.next('');
  }

  /**
   * Verifica si hay un filtro de búsqueda activo.
   */
  get hasActiveSearchFilter(): boolean {
    return this.searchTerm.trim().length > 0;
  }

  /**
   * Obtiene el conteo de resultados filtrados.
   */
  get filteredResultsCount(): number {
    return this.dataSource.filteredData.length;
  }

  /**
   * Obtiene el conteo de URLs activas.
   */
  get activeUrlsCount(): number {
    return this.allUrls.filter((url) => url.isActive).length;
  }

  /**
   * Obtiene el conteo de URLs inactivas.
   */
  get inactiveUrlsCount(): number {
    return this.allUrls.filter((url) => !url.isActive).length;
  }

  /**
   * Verifica si hay URLs en total.
   */
  get hasUrls(): boolean {
    return this.allUrls.length > 0;
  }

  /**
   * Copia la URL acortada al portapapeles.
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
   */
  viewStats(shortCode: string): void {
    this.router.navigate(['/urls', shortCode, 'stats']);
  }

  /**
   * Abre un diálogo de confirmación para eliminar la URL.
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
   */
  private performDelete(url: ShortUrlResponse): void {
    this.urlService.deleteUrl(url.shortCode).subscribe({
      next: () => {
        // Eliminar de ambas listas
        this.allUrls = this.allUrls.filter(
          (u) => u.shortCode !== url.shortCode
        );
        this.dataSource.data = this.allUrls;
        // Apply status filter if active
        if (this.statusFilter !== 'all') {
             this.applyFilters();
        }
        
        // Recargar para actualizar el total y la lista
        this.loadUserUrls();

        this.snackBar.open('✓ URL deleted successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
      },
      error: (error) => {
        console.error('Error deleting URL:', error);
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
   * Verifica si se debe mostrar el estado vacío.
   */
  get shouldShowEmptyState(): boolean {
    return !this.isLoading && !this.errorMessage && !this.hasUrls && !this.hasActiveSearchFilter;
  }

  /**
   * Verifica si no hay resultados de búsqueda.
   */
  get hasNoSearchResults(): boolean {
    const isFiltering = this.hasActiveSearchFilter || this.statusFilter !== 'all';
    
    if (!isFiltering) {
      return false;
    }

    // Caso 1: Búsqueda activa sin resultados del servidor (hasUrls es false)
    if (this.hasActiveSearchFilter && !this.hasUrls) {
      return true;
    }

    // Caso 2: Hay URLs cargadas pero el filtro de estado las oculta todas
    if (this.hasUrls && this.filteredResultsCount === 0) {
      return true;
    }

    return false;
  }
}
