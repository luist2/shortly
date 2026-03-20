import { Component, Input, SecurityContext } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

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

  /** URL del QR generado por la libreria (para descarga) */
  qrCodeUrl: SafeUrl | null = null;

  private pendingDownloadFilename: string | null = null;

  constructor(private sanitizer: DomSanitizer) {}

  onQrCodeUrlChange(url: SafeUrl): void {
    this.qrCodeUrl = url;
    if (!this.pendingDownloadFilename) {
      return;
    }

    this.downloadFromSafeUrl(url, this.pendingDownloadFilename);
    this.pendingDownloadFilename = null;
  }

  downloadPng(): void {
    this.requestDownload('img', 'qr-code.png');
  }

  downloadSvg(): void {
    this.requestDownload('svg', 'qr-code.svg');
  }

  private requestDownload(type: 'img' | 'svg', filename: string): void {
    this.pendingDownloadFilename = filename;

    if (this.elementType === type && this.qrCodeUrl) {
      this.downloadFromSafeUrl(this.qrCodeUrl, filename);
      this.pendingDownloadFilename = null;
      return;
    }

    this.elementType = type;
  }

  private downloadFromSafeUrl(url: SafeUrl, filename: string): void {
    const sanitized = this.sanitizer.sanitize(SecurityContext.URL, url);
    if (!sanitized) {
      // eslint-disable-next-line no-console
      console.error('[qr] Unable to sanitize QR code URL for download');
      return;
    }

    this.triggerDownload(sanitized, filename);
  }

  private triggerDownload(url: string, filename: string): void {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
  }
}
