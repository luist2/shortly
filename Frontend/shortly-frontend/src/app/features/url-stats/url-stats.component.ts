import { Clipboard } from '@angular/cdk/clipboard';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { UrlService } from 'src/app/core/services/url.service';
import { ShortUrlStatsResponse } from 'src/app/models/short-url.model';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-url-stats',
  templateUrl: './url-stats.component.html',
  styleUrls: ['./url-stats.component.scss'],
})
export class UrlStatsComponent implements OnInit {
  shortCode: string = '';
  urlStats: ShortUrlStatsResponse | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private urlService: UrlService,
    private clipboard: Clipboard,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    // Obtener el parametro shortCode de la URL
    this.shortCode = this.route.snapshot.paramMap.get('shortCode') || '';

    if (this.shortCode) {
      this.loadUrlStats();
    } else {
      this.errorMessage = 'Invalid URL code.';
    }
  }

  loadUrlStats(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.urlService.getUrlStats(this.shortCode).subscribe({
      next: (stats: ShortUrlStatsResponse) => {
        this.urlStats = stats;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;

        // Manejo de errores
        if (error.status === 404) {
          this.errorMessage = 'URL not found.';
        } else if (error.status === 403) {
          this.errorMessage =
            'You do not have permission to view these statistics.';
        } else {
          this.errorMessage = 'An error occurred while loading URL statistics.';
        }
      },
    });
  }

  copyToClipboard(): void {
    if (!this.urlStats) return;

    const success = this.clipboard.copy(this.urlStats.shortUrl);
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
   * Abre un diálogo de confirmación para eliminar la URL.
   */
  deleteUrl(): void {
    if (!this.urlStats) return;

    const dialogData: ConfirmDialogData = {
      title: 'Delete URL',
      message: `Are you sure you want to delete this shortened URL? This action cannot be undone.\n\nShort URL: ${this.urlStats.shortUrl}`,
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
        this.performDelete();
      }
    });
  }

  /**
   * Realiza la eliminación de la URL y maneja la respuesta.
   */
  private performDelete(): void {
    this.urlService.deleteUrl(this.shortCode).subscribe({
      next: () => {
        // Mostrar mensaje de éxito
        this.snackBar.open('✓ URL deleted successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });

        // Redirigir al dashboard después de eliminar
        this.goToDashboard();
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
   * Navega de vuelta al dashboard
   */
  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
