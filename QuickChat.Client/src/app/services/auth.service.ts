import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/Auth';
  private accessTokenKey = 'access_token';
  private refreshTokenKey = 'refresh_token';

  private popup: Window | null = null;
  //private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  constructor(private http: HttpClient, private router: Router, private ngZone: NgZone) {
    //window.addEventListener('message', this.receiveMessage.bind(this));
  }
  register(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, payload);
  }

  login(user: { email: string; password: string }): Observable<any> {
    return this.http.post<{ accessToken: string, refreshToken: string, id: string }>(`${this.apiUrl}/login`, user).pipe(
      tap(response => {
        this.storeToken(response.accessToken, response.refreshToken);
        this.storeCurrentUserId(response.id)
        //this.isLoggedInSubject.next(true);
        console.log('login-response :', response)
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem('userId');
    //this.isLoggedInSubject.next(false);
  }

 storeToken(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.accessTokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }
  private storeCurrentUserId(userId: string): void {
    localStorage.setItem('userId', userId);
  }
  getToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }
  // private hasToken(): boolean {
  //   return !!this.getToken();
  // }

  // private receiveMessage(event: MessageEvent) {
  //   // Only accept from same origin
  //   if (event.origin !== window.location.origin) return;
  //   const data = event.data;
  //   if (data && data.token) {
  //     // store token and navigate inside Angular zone
  //     this.ngZone.run(() => {
  //       localStorage.setItem(this.accessTokenKey, data.token);
  //       // set auth state, then route
  //       this.router.navigate(['/']);
  //     });
  //   }
  // }

  startExternalLogin(provider: string) {
    if (provider === 'Google') {
      window.location.href = `${this.apiUrl}/login-with-google?provider=${provider}&returnUrl=/chat`;
    }
    if (provider === 'Facebook') {
      window.location.href = `${this.apiUrl}/login-with-facebook?provider=${provider}&returnUrl=/chat`;
    }
  }
}
