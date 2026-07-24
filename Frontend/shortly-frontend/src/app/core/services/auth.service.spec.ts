import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';

import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  const refreshUrl = `${environment.apiUrl}/Auth/refresh-tokens`;
  const accessToken = 'eyJhbGciOiJub25lIn0.eyJuYW1laWQiOiJ1c2VyLTEifQ.';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('restores the session when refresh succeeds', () => {
    const service = TestBed.inject(AuthService);

    const request = httpMock.expectOne(refreshUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBeTrue();
    request.flush({ accessToken });

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.getUserId()).toBe('user-1');
  });

  it('retries refresh once after a 429 using Retry-After', fakeAsync(() => {
    const service = TestBed.inject(AuthService);
    const retryAfterHeaders = { 'Retry-After': '1' };

    httpMock
      .expectOne(refreshUrl)
      .flush({ message: 'Too many requests' }, {
        status: 429,
        statusText: 'Too Many Requests',
        headers: retryAfterHeaders,
      });

    tick(999);
    httpMock.expectNone(refreshUrl);

    tick(1);
    httpMock.expectOne(refreshUrl).flush({ accessToken });

    expect(service.isAuthenticated()).toBeTrue();
  }));

  it('does not retry a second 429 and leaves the user unauthenticated', fakeAsync(() => {
    const service = TestBed.inject(AuthService);
    const retryAfterHeaders = { 'Retry-After': '1' };

    httpMock
      .expectOne(refreshUrl)
      .flush({}, { status: 429, statusText: 'Too Many Requests', headers: retryAfterHeaders });

    tick(1_000);
    httpMock
      .expectOne(refreshUrl)
      .flush({}, { status: 429, statusText: 'Too Many Requests', headers: retryAfterHeaders });

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.getUserId()).toBeNull();
  }));

  it('does not retry an unauthorized refresh', fakeAsync(() => {
    const service = TestBed.inject(AuthService);

    httpMock
      .expectOne(refreshUrl)
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    tick(60_000);
    httpMock.expectNone(refreshUrl);

    expect(service.isAuthenticated()).toBeFalse();
  }));
});
