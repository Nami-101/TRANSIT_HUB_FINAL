import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

let isRefreshing = false;
let refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Add JWT token to request if available
  const token = authService.getToken();
  
  if (token && !isAuthRequest(req.url)) {
    req = addTokenToRequest(req, token);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && !isAuthRequest(req.url)) {
        return handle401Error(req, next, authService, router);
      }

      // Handle other errors
      return throwError(() => error);
    })
  );
};

/**
 * Add JWT token to request headers
 */
function addTokenToRequest(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

/**
 * Check if this is an authentication-related request
 */
function isAuthRequest(url: string): boolean {
  return url.includes('/auth/login') || 
         url.includes('/auth/register') || 
         url.includes('/auth/refresh');
}

/**
 * Handle 401 Unauthorized errors with token refresh
 */
function handle401Error(
  request: HttpRequest<any>, 
  next: HttpHandlerFn, 
  authService: AuthService, 
  router: Router
): Observable<HttpEvent<any>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = authService.getRefreshToken();
    
    if (refreshToken) {
      return authService.refreshToken().pipe(
        switchMap((response) => {
          isRefreshing = false;
          
          if (response.success && response.data) {
            refreshTokenSubject.next(response.data.token);
            return next(addTokenToRequest(request, response.data.token));
          } else {
            // Only logout if we're not on auth pages
            if (!router.url.includes('/auth/')) {
              authService.logout().subscribe();
              router.navigate(['/auth/login']);
            }
            return throwError(() => new Error('Token refresh failed'));
          }
        }),
        catchError((err) => {
          isRefreshing = false;
          // Only logout if we're not on auth pages
          if (!router.url.includes('/auth/')) {
            authService.logout().subscribe();
            router.navigate(['/auth/login']);
          }
          return throwError(() => err);
        })
      );
    } else {
      // No refresh token, only logout if not on auth pages
      if (!router.url.includes('/auth/')) {
        authService.logout().subscribe();
        router.navigate(['/auth/login']);
      }
      return throwError(() => new Error('No refresh token available'));
    }
  } else {
    // Wait for the token refresh to complete
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next(addTokenToRequest(request, token)))
    );
  }
}