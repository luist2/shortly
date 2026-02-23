import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UrlService } from 'src/app/core/services/url.service';
import { AuthService } from 'src/app/core/services/auth.service';
import { ShortUrlResponse } from 'src/app/models/short-url.model';
import { Clipboard } from '@angular/cdk/clipboard';

@Component({
  selector: 'app-url-create',
  templateUrl: './url-create.component.html',
  styleUrls: ['./url-create.component.scss'],
})
export class UrlCreateComponent implements OnInit {
  urlForm!: FormGroup;
  isLoading = false;
  createdUrl: ShortUrlResponse | null = null;

  constructor(
    private fb: FormBuilder,
    private urlService: UrlService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private clipboard: Clipboard
  ) { }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.urlForm = this.fb.group({
      originalUrl: [
        '',
        [
          Validators.required,
          Validators.maxLength(2048),
          Validators.pattern(/^https?:\/\//),
        ],
      ],
    });

    if (this.isAuthenticated) {
      this.urlForm.addControl(
        'expiresInHours',
        this.fb.control(168, [Validators.required]) // Default to 1 week (168 hours)
      );
    }
  }

  onSubmit(): void {
    if (this.urlForm.invalid) {
      this.urlForm.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    const originalUrl = this.urlForm.get('originalUrl')?.value;
    const expiresInHours = this.isAuthenticated ? this.urlForm.get('expiresInHours')?.value : undefined;

    this.urlService.createShortUrl(originalUrl, expiresInHours).subscribe({
      next: (response: ShortUrlResponse) => {
        this.isLoading = false;
        this.createdUrl = response;
        this.urlForm.reset();

        this.snackBar.open('Short URL created successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;

        let errorMessage = 'Unexpected error occurred. Please try again.';

        // Backend no disponible / servidor caído
        if (error.status === 0) {
          errorMessage = 'Server is not available. Please try again later.';
        }

        // Error de validación (400)
        else if (error.status === 400 && error.error?.errors?.OriginalUrl) {
          errorMessage = error.error.errors.OriginalUrl[0];

          // Marcar el campo como inválido desde backend
          this.urlForm.get('originalUrl')?.setErrors({ backend: true });
        }

        // Rate limit excedido (429)
        else if (error.status === 429 && error.error?.message) {
          errorMessage = error.error.message;
        }

        // Otros errores (500, 403, etc.)
        else if (error.error?.title) {
          errorMessage = error.error.title;
        }

        this.snackBar.open(errorMessage, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'bottom',
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  copyToClipboard(): void {
    if (!this.createdUrl) return;

    const success = this.clipboard.copy(this.createdUrl.shortUrl);

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

  getErrorMessage(field: string): string {
    const control = this.urlForm.get(field);

    if (control?.hasError('required')) {
      return 'This field is required';
    }

    if (control?.hasError('pattern')) {
      return 'Please enter a valid URL';
    }

    if (control?.hasError('backend')) {
      return 'The provided URL is not valid';
    }

    return '';
  }

  resetForm(): void {
    this.createdUrl = null;
    this.urlForm.reset();
  }
}
