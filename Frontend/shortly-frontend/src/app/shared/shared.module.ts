import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

// Material Modules
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';

// CDK
import { ClipboardModule } from '@angular/cdk/clipboard';

// Third-party
import { QRCodeModule } from 'angularx-qrcode';

// Components
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { AppQrCodeComponent } from './components/qr-code/qr-code.component';

@NgModule({
  declarations: [ConfirmDialogComponent, LoadingSpinnerComponent, AppQrCodeComponent],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    QRCodeModule,
  ],
  exports: [
    // Angular Common
    CommonModule,

    // CDK
    ClipboardModule,

    // Material Modules comunes
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    MatDialogModule,

    // Componentes compartidos
    ConfirmDialogComponent,
    LoadingSpinnerComponent,
    AppQrCodeComponent,
    QRCodeModule,
  ],
})
export class SharedModule { }
