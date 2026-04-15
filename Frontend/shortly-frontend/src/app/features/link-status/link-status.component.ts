import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

type LinkStatusReason = 'not-found' | 'expired' | 'invalid' | 'server-error' | 'unknown';

@Component({
  selector: 'app-link-status',
  templateUrl: './link-status.component.html',
  styleUrls: ['./link-status.component.scss'],
})
export class LinkStatusComponent implements OnInit {
  reason: LinkStatusReason = 'unknown';
  shortCode: string | null = null;
  icon = 'error_outline';
  statusClass = 'status-unknown';
  title = 'Link unavailable';
  message = 'We could not open this link.';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    const reasonParam = this.route.snapshot.queryParamMap.get('reason') ?? '';
    const shortCodeParam = this.route.snapshot.queryParamMap.get('code');

    this.reason = this.parseReason(reasonParam);
    this.shortCode = shortCodeParam;
    this.setMessagesByReason();
  }

  private parseReason(rawReason: string): LinkStatusReason {
    switch (rawReason) {
      case 'not-found':
      case 'expired':
      case 'invalid':
      case 'server-error':
        return rawReason;
      default:
        return 'unknown';
    }
  }

  private setMessagesByReason(): void {
    switch (this.reason) {
      case 'not-found':
        this.title = 'Link not found';
        this.message = 'This link does not exist.';
        this.icon = 'link_off';
        this.statusClass = 'status-not-found';
        return;
      case 'expired':
        this.title = 'Link expired';
        this.message = 'This link has expired.';
        this.icon = 'schedule';
        this.statusClass = 'status-expired';
        return;
      case 'invalid':
        this.title = 'Invalid link';
        this.message = 'This link format is invalid.';
        this.icon = 'error_outline';
        this.statusClass = 'status-invalid';
        return;
      case 'server-error':
        this.title = 'Server issue';
        this.message = 'Please try again in a moment.';
        this.icon = 'cloud_off';
        this.statusClass = 'status-server-error';
        return;
      default:
        this.title = 'Link unavailable';
        this.message = 'We could not open this link.';
        this.icon = 'help_outline';
        this.statusClass = 'status-unknown';
    }
  }
}
