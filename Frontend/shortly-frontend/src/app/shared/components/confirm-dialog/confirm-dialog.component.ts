import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

/**
 * Interfaz para los datos que recibe el diálogo de confirmación
 */
export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
}

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss'],
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {
    // Valores por defecto
    this.data.confirmText = data.confirmText || 'Confirm';
    this.data.cancelText = data.cancelText || 'Cancel';
    this.data.confirmColor = data.confirmColor || 'primary';
  }

  /**
   * Maneja el click en el botón de cancelar
   */
  onCancel(): void {
    this.dialogRef.close(false);
  }

  /**
   * Maneja el click en el botón de confirmar
   */
  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
