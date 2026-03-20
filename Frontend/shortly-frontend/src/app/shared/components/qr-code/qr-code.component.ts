import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { QRCodeComponent } from 'angularx-qrcode';
import { SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-qr-code',
  templateUrl: './qr-code.component.html',
  styleUrls: ['./qr-code.component.scss'],
})
export class AppQrCodeComponent {
  @Input() url: string = '';
  @Input() size: number = 200;

  /** elementType activo para renderizar el QR. Se alterna para disparar descargas. */
  elementType: 'img' | 'svg' = 'img';

  /** URL del QR generado por la librería (para descarga) */
  qrCodeUrl: SafeUrl | null = null;

  onQrCodeUrlChange(url: SafeUrl): void {
    this.qrCodeUrl = url;
  }

  downloadPng(): void {
    this.elementType = 'img';
    // Necesitamos esperar un tick para que la librería regenere el QR con elementType='img'
    setTimeout(() => {
      if (this.qrCodeUrl) {
        this.triggerDownload(this.qrCodeUrl as string, 'qr-code.png');
      }
    }, 100);
  }

  downloadSvg(): void {
    this.elementType = 'svg';
    setTimeout(() => {
      if (this.qrCodeUrl) {
        this.triggerDownload(this.qrCodeUrl as string, 'qr-code.svg');
      }
    }, 100);
  }

  private triggerDownload(url: string, filename: string): void {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
  }
}
