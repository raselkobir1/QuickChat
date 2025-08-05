import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'http://localhost:5000/api';
  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('access_token');
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Auth/users`, {
      headers: this.getAuthHeaders()
    });
  }

  getGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Chat/my-groups`, {
      headers: this.getAuthHeaders()
    });
  }

  getPrivateMessages(receiverId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/chat/private-history/${receiverId}`, {
      headers: this.getAuthHeaders()
    });
  }

    getGroupMessages(groupId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/chat/group-history/${groupId}`, {
      headers: this.getAuthHeaders()
    });
  }

    getCurrentUserProfile(): Observable<any[]> {
    return this.http.get<any>(`${this.apiUrl}/Auth/profile`, {
      headers: this.getAuthHeaders()
    });
  }

  createGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/create-group`, payload,{
      headers: this.getAuthHeaders()
    })
  }
}
