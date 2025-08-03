import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  private messageReceivedSubject = new Subject<{ senderId: string, message: string }>();
  private groupMessageSubject = new Subject<{ senderId: string, groupId: string, message: string }>();
  private systemMessageSubject = new Subject<{ sender: string, message: string, time: string }>();
  private connectedUsersSubject = new Subject<string[]>();

  messageReceived$ = this.messageReceivedSubject.asObservable();
  groupMessageReceived$ = this.groupMessageSubject.asObservable();
  systemMessage$ = this.systemMessageSubject.asObservable();
  connectedUsers$ = this.connectedUsersSubject.asObservable();

  connect(): void {
    const token = localStorage.getItem('access_token');
    if (!token) {
      console.warn('No token found. SignalR connection not started.');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/chatHub', {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerHandlers();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR connection failed:', err));
  }

  disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private registerHandlers(): void {
    this.hubConnection.on('ReceiveMessage', (senderId: string, message: string) => {
      this.messageReceivedSubject.next({ senderId, message });
    });

    this.hubConnection.on('ReceiveGroupMessage', (senderId: string, groupId: string, message: string) => {
      this.groupMessageSubject.next({ senderId, groupId, message });
    });

    this.hubConnection.on('ReceiveMessage', (sender: string, message: string, time: string) => {
      this.systemMessageSubject.next({ sender, message, time });
    });

    this.hubConnection.on('ReceiveConnectedUsers', (users: string[]) => {
      this.connectedUsersSubject.next(users);
    });
  }

  sendMessageToUser(receiverId: string, message: string): void {
    this.hubConnection.invoke('SendMessageToUser', receiverId, message)
      .catch(err => console.error('SendMessageToUser error:', err));
  }

  sendMessageToGroup(groupId: string, message: string): void {
    this.hubConnection.invoke('SendMessageToGroup', groupId, message)
      .catch(err => console.error('SendMessageToGroup error:', err));
  }

  joinGroup(groupId: string): void {
    this.hubConnection.invoke('JoinGroup', groupId)
      .catch(err => console.error('JoinGroup error:', err));
  }

  leaveGroup(groupId: string): void {
    this.hubConnection.invoke('LeaveGroup', groupId)
      .catch(err => console.error('LeaveGroup error:', err));
  }
}
