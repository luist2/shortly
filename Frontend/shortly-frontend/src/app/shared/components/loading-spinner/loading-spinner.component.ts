import { Component, Input } from '@angular/core';

/**
 * Componente reutilizable para mostrar un spinner de carga.
 * Puede usarse en diferentes contextos: inline, centrado o como overlay.
 */
@Component({
  selector: 'app-loading-spinner',
  templateUrl: './loading-spinner.component.html',
  styleUrls: ['./loading-spinner.component.scss'],
})
export class LoadingSpinnerComponent {
  /**
   * Mensaje que se muestra debajo del spinner.
   */
  @Input() message: string = 'Loading...';

  /**
   *  Diametro del spinner en píxeles.
   *  24, 32, 40, 50, 60
   */
  @Input() diameter: number = 50;

  /**
   * Tipo de layout:
   * - 'centered': Centrado verticalmente con padding (para secciones completas)
   * - 'inline': Sin padding adicional (para usar dentro de containers)
   * - 'overlay': Capa superpuesta sobre el contenido (posición absoluta)
   */
  @Input() layout: 'centered' | 'inline' | 'overlay' = 'centered';

  /**
   * Si se debe mostrar el mensaje de texto
   */
  @Input() showMessage: boolean = true;

  /**
   * Color del spinner (theme de Material)
   * Opciones: 'primary', 'accent', 'warn'
   */
  @Input() color: 'primary' | 'accent' | 'warn' = 'primary';
}
