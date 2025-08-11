import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';

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

  getCurrentUserProfile(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Auth/profile`, {
      headers: this.getAuthHeaders()
    });
  }

  createGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/create-group`, payload, {
      headers: this.getAuthHeaders()
    })
  }

  addMembersToGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/add-member`, payload, {
      headers: this.getAuthHeaders()
    })
  }

    deleteMembersFromGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/delete-member`, payload, {
      headers: this.getAuthHeaders()
    })
  }

    updateUserProfile(payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/Auth/update-profile`, payload, {
      headers: this.getAuthHeaders()
    })
  }


async uploadFile(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);

    try {
      // Convert Observable to Promise for async/await usage
      const response: any = await lastValueFrom(this.http.post(`${this.apiUrl}/Auth/file-upload`, formData,{
        headers: this.getAuthHeaders()
      }));
      return response.path;
    } catch (error) {
      console.error('File upload failed', error);
      throw error;
    }
  }

  getStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/Dashboard/stats`, {
      headers: this.getAuthHeaders()
    });
  }


}
