import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { UserDTO } from 'src/app/models/user.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group(
      {
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(6),
            this.passwordStrengthValidator,
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      {
        validators: this.passwordMatchValidator,
      }
    );
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) {
      return null;
    }

    const hasNumber = /[0-9]/.test(value);
    const hasUpper = /[A-Z]/.test(value);
    const hasLower = /[a-z]/.test(value);

    const passwordValid = hasNumber && hasUpper && hasLower;

    return !passwordValid ? { passwordStrength: true } : null;
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) return null;

    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      confirmPassword.setErrors(null);
      return null;
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    const userDTO: UserDTO = {
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
    };

    this.authService.register(userDTO).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('Registration successful!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar'],
        });
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Registration error:', error);

        let errorMessage =
          'An error occurred during registration. Please try again.';

        if (error.status === 400) {
          errorMessage =
            'Invalid data. Please check your information and try again.';
        } else if (error.status === 409) {
          errorMessage =
            'Email is already in use. Please use a different email.';
        } else if (error.status === 0) {
          errorMessage =
            'Unable to connect to the server. Please check your internet connection.';
        } else if (error.error?.message) {
          errorMessage = error.error.message;
        }

        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (field?.hasError('required')) {
      return 'This field is required';
    }

    if (fieldName === 'email' && field?.hasError('email')) {
      return 'Please enter a valid email address';
    }

    if (fieldName === 'password') {
      if (field?.hasError('minlength')) {
        return 'Password must be at least 6 characters long';
      }
      if (field?.hasError('passwordStrength')) {
        return 'Password must contain uppercase, lowercase letters and a number';
      }
    }

    if (fieldName === 'confirmPassword') {
      if (this.registerForm.hasError('passwordMismatch') && field?.touched) {
        return 'Passwords do not match';
      }
    }

    return '';
  }
}
