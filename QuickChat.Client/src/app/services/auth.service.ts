import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/Auth';
  private accessTokenKey = 'access_token';
  private refreshTokenKey = 'refresh_token';

  constructor(private http: HttpClient, private router: Router) {
  }
  register(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, payload);
  }

  login(user: { email: string; password: string }): Observable<any> {
    return this.http.post<{ accessToken: string, refreshToken: string, id: string }>(`${this.apiUrl}/login`, user).pipe(
      tap(response => {
        this.storeToken(response.accessToken, response.refreshToken);
        this.storeCurrentUserId(response.id)
        console.log('login-response :', response)
      })
    );
  }

  startExternalLogin(provider: string) {
    if (provider === 'Google') {
      window.location.href = `${this.apiUrl}/login-with-google?provider=${provider}&returnUrl=/chat`;
    }
    if (provider === 'Facebook') {
      window.location.href = `${this.apiUrl}/login-with-facebook?provider=${provider}&returnUrl=/chat`;
    }
  }

  logout(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem('userId');
    this.router.navigate(['/welcome']);
  }

  storeToken(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.accessTokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }
  private storeCurrentUserId(userId: string): void {
    localStorage.setItem('userId', userId);
  }
  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  isAccessTokenExpired(accessToken: string): boolean {
    try {
      const decoded = jwtDecode<JwtPayload>(accessToken);
      if (!decoded.exp) return true;
      const now = Date.now() / 1000;
      return decoded.exp < now;
    } catch {
      return true;
    }
  }

  refreshAccessToken() {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.logout();
      return;
    }
    return this.http.post<{ accessToken: string; refreshToken: string }>(
      `${this.apiUrl}/refresh-token`, { refreshToken }
    );
  }
}
interface JwtPayload {
  exp: number;
}
