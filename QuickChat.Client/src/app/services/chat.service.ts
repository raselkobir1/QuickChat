import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = `${environment.apiUrl}`;
  constructor(private http: HttpClient) { }

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Auth/users`);
  }

  getGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Chat/my-groups`);
  }

  getPrivateMessages(receiverId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/chat/private-history/${receiverId}`);
  }

  getGroupMessages(groupId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/chat/group-history/${groupId}`);
  }

  getCurrentUserProfile(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Auth/profile`);
  }

  createGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/create-group`, payload)
  }

  addMembersToGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/add-member`, payload)
  }

    deleteMembersFromGroup(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Chat/delete-member`, payload)
  }

    updateUserProfile(payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/Auth/update-profile`, payload)
  }


async uploadFile(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);

    try {
      // Convert Observable to Promise for async/await usage
      const response: any = await lastValueFrom(this.http.post(`${this.apiUrl}/Auth/file-upload`, formData));
      return response.path;
    } catch (error) {
      console.error('File upload failed', error);
      throw error;
    }
  }

  getStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/Dashboard/stats`);
  }


}
