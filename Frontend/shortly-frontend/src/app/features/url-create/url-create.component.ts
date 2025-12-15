import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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

  constructor(private fb: FormBuilder, private urlService: UrlService) {}

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
      error: (error) => {
        this.isLoading = false;
        console.error('Error creating short URL:', error);
        // TODO: Mostrar error con MatSnackBar
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

    return '';
  }

  resetForm(): void {
    this.createdUrl = null;
    this.urlForm.reset();
  }
}
