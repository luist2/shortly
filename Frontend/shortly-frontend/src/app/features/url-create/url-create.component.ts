import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UrlService } from 'src/app/core/services/url.service';
import { ShortUrlResponse } from 'src/app/models/short-url.model';

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
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.urlForm = this.fb.group({
      originalUrl: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/
          ),
        ],
      ],
    });
  }

  onSubmit(): void {
    if (this.urlForm.invalid) {
      this.urlForm.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    const originalUrl = this.urlForm.get('originalUrl')?.value;

    this.urlService.createShortUrl(originalUrl).subscribe({
      next: (response: ShortUrlResponse) => {
        this.isLoading = false;
        this.createdUrl = response;
        this.urlForm.reset();
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

        // Otros errores (500, 403, etc.)
        else if (error.error?.title) {
          errorMessage = error.error.title;
        }

        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['snackbar-error'],
        });
      },
    });
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
