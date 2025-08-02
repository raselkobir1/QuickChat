import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/Auth';
  private tokenKey = 'access_token';

  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  constructor(private http: HttpClient) { }
  register(payload:any): Observable<any> {
    console.log('service:', payload)
    return this.http.post(`${this.apiUrl}/register`, payload);
  }
  login(user: { email: string; password: string }): Observable<any> {
    return this.http.post<{ token: string , id:string}>(`${this.apiUrl}/login`, user).pipe(
      tap(response => {
        this.storeToken(response.token);
        this.storeCurrentUserId(response.id)
        this.isLoggedInSubject.next(true);
        console.log(response)
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.isLoggedInSubject.next(false);
  }

  private storeToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }
    private storeCurrentUserId(userId: string): void {
    localStorage.setItem('userId', userId);
  }
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }
  private hasToken(): boolean {
    return !!this.getToken();
  }
}
