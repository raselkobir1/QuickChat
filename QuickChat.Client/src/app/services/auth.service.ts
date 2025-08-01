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
  register(user: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }
  login(user: { email: string; password: string }): Observable<any> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/login`, user).pipe(
      tap(response => {
        this.storeToken(response.token);
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
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }
  private hasToken(): boolean {
    return !!this.getToken();
  }
}
