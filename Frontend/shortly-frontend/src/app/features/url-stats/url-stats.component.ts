import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-url-stats',
  templateUrl: './url-stats.component.html',
  styleUrls: ['./url-stats.component.scss'],
})
export class UrlStatsComponent implements OnInit {
  shortCode: string = '';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Obtener el parametro shortCode de la URL
    this.shortCode = this.route.snapshot.paramMap.get('shortCode') || '';

    if (this.shortCode) {
      console.log(`Short code obtenido: ${this.shortCode}`);
    }
  }
}
