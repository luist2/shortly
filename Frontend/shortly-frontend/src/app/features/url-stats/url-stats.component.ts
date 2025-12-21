import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UrlService } from 'src/app/core/services/url.service';
import { ShortUrlStatsResponse } from 'src/app/models/short-url.model';

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
    private urlService: UrlService
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

  private loadUrlStats(): void {
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

  /**
   * Navega de vuelta al dashboard
   */
  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
