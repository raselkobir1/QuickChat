import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  private messageReceivedSubject = new BehaviorSubject<ChatMessage | null>(null);
  public messageReceived$ = this.messageReceivedSubject.asObservable();
  public receivedMessages: any;

  private connectedUsersSubject = new BehaviorSubject<any>(null);
  public connectedUsers$ = this.connectedUsersSubject.asObservable();
  public connectedUser: any;

  connect(): void {
    const token = localStorage.getItem('access_token');
    if (!token) {
      console.warn('No token found. SignalR connection not started.');
      return;
    }
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.HubUrl}`, {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR connection failed:', err));

    this.registerHandlers();
  }

  disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private registerHandlers(): void {
    this.hubConnection.on('ReceiveMessage', (meg: ChatMessage) => {
      this.receivedMessages = meg;
      this.messageReceivedSubject.next(this.receivedMessages);
    });

    this.hubConnection.on('ReceiveConnectedUsers', (groupUser: any) => {
      this.connectedUser = groupUser;
      this.connectedUsersSubject.next(this.connectedUser);
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

export class ChatMessage {
  constructor(
    public content: string,
    public senderId: string,
    public sentAt: string,
    public userName: string,
    public receiverId?: string,
    public groupId?: string,
  ) { }

  get isGroup(): boolean {
    return !!this.groupId;
  }
}