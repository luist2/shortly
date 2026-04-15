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
  title = 'Unable to open short link';
  message = 'This short link could not be processed.';

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
        this.title = 'Short link not found';
        this.message = 'This short link does not exist or is no longer available.';
        return;
      case 'expired':
        this.title = 'Short link expired';
        this.message = 'This short link has expired and cannot be used anymore.';
        return;
      case 'invalid':
        this.title = 'Invalid short link';
        this.message = 'The short link format is invalid.';
        return;
      case 'server-error':
        this.title = 'Temporary server issue';
        this.message = 'We could not process this short link right now. Please try again later.';
        return;
      default:
        this.title = 'Unable to open short link';
        this.message = 'This short link could not be processed.';
    }
  }
}
