import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/chat")
    .configureLogging(signalR.LogLevel.Information)
    .build();

  public messages$ = new BehaviorSubject<any>([]);
  public connectedUsers$ = new BehaviorSubject<string[]>([]);
  public messages: any[] = [];
  public users: string[] = [];

  constructor() { 
    this.connection.start()
      .then(() => console.log("Connection started"))
      .catch(err => console.error("Error while starting connection: ", err));

    this.connection.on("ReceiveMessage", (user: string, message: string, messageTime: string)=>{
      this.messages = [...this.messages, {user, message, messageTime} ];
      this.messages$.next(this.messages);
    });

    this.connection.on("ReceiveConnectedUsers", (users: any) => {
      this.connectedUsers$.next(users);
      console.log("Connected users updated:", this.users);
    });
  }


  // Join Room
  public async joinRoom(user: string, room: string){
    return this.connection.invoke("JoinRoom", {user, room})
  }
  // Send Messages
  public async sendMessage(message: string){
    return this.connection.invoke("SendMessage", message)
  }
  // leave chat/Room
  public async leaveChat(){
    return this.connection.stop()
  }
}
