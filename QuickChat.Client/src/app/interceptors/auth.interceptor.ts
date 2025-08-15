import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Observable, switchMap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';


@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;

  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token = this.authService.getAccessToken();
    if (token && !this.authService.isAccessTokenExpired(token)) {
      req = this.addToken(req, token);
      return next.handle(req);
    }

    if (token && this.authService.isAccessTokenExpired(token)) {
      if (!this.isRefreshing) {
        this.isRefreshing = true;
        return this.authService.refreshAccessToken()!.pipe(
          switchMap((res) => {
            this.isRefreshing = false;
            this.authService.storeToken(res.accessToken, res.refreshToken);
            req = this.addToken(req, res.accessToken);
            return next.handle(req);
          }),
          catchError((err) => {
            this.isRefreshing = false;
            this.authService.logout();
            return throwError(() => err);
          })
        );
      }
    }

    return next.handle(req);
  }

  private addToken(req: HttpRequest<any>, token: string) {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
}
