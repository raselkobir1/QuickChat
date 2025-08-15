import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const AuthErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 403 || error.status === 0) {
        console.error('Auth/CORS error detected:', error);
        router.navigate(['/access-denied']);
      }
      if (error.status === 401) {
        console.error('Auth/CORS error detected:', error);
        router.navigate(['/auth-failed']);
      }
      console.error('Interceptor detected error :', error);
      return throwError(() => error);
    })
  );
};

