import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { SignalRService} from '../../services/signalr.service'
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  selectedChat: any = null;
  messageInput: string = '';
  users: any[] = [];
  groups: any[] = [];
  messages: any[] = [];
  loginUserId: string = '';
  isGroupChat: boolean = false;

  constructor(
    private chatService: ChatService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.loginUserId = localStorage.getItem('userId') ?? '';
    this.loadUsers();
    this.loadGroups();

    this.signalRService.connect();

    // this.signalRService.messageReceived$.subscribe(msg => {
    //   // Message to me (private)
    //   console.log('private msg', msg);
    //   if (this.selectedChat === msg.senderId && !this.isGroupChat) {
    //     this.messages.push({
    //       text: msg.message,
    //       senderId: msg.senderId,
    //       sentAt: new Date()
    //     });
    //   }
    // });

    // this.signalRService.groupMessageReceived$.subscribe(msg => {
    //   console.log('group msg', msg);
    //   if (this.selectedChat === msg.groupId && this.isGroupChat) {
    //     this.messages.push({
    //       content: msg.message,
    //       sender: msg.senderId === this.loginUserId ? 'me' : 'other',
    //       senderId: msg.senderId,
    //       timestamp: new Date()
    //     });
    //   }
    // });
  }

  ngOnDestroy(): void {
    this.signalRService.disconnect();
  }

  loadUsers() {
    this.chatService.getUsers().subscribe((res) => (this.users = res));
  }

  loadGroups() {
    this.chatService.getGroups().subscribe((res) => (this.groups = res));
  }

  selectChat(id: string, isGroup = false) {
    this.selectedChat = id;
    this.isGroupChat = isGroup;
    this.messages = [];

    if (isGroup) {
      //this.signalRService.joinGroup(id); // ✅ Join group on SignalR
      this.chatService.getGroupMessages(id).subscribe((res) => {
        console.log('getGroupMessages msg', res);
        this.messages = res.map(m => ({
          text: m.content,
          senderId: m.senderId,
          sentAt: m.timestamp
        }));
      });
    } else {
      this.chatService.getPrivateMessages(id).subscribe((res) => {
        console.log('getPrivateMessages msg', res);
        this.messages = res.map(m => ({
          text: m.content,
          senderId: m.senderId,
          sentAt: m.timestamp
        }));
        console.log('PrivateMessages msg', this.messages[0].senderId);
        console.log('loginUserId', this.loginUserId);
      });
    }
  }

  sendMessage() {
    if (!this.messageInput.trim() || !this.selectedChat) return;

    const message = this.messageInput;
    this.messageInput = '';

    // Add to UI immediately
    // this.messages.push({
    //   text: message,
    //   sender: 'me',
    //   timestamp: new Date()
    // });

    if (this.isGroupChat) {
      this.signalRService.sendMessageToGroup(this.selectedChat, message); // groupId, content, 
    } else {
      this.signalRService.sendMessageToUser(this.selectedChat, message); // receiverId, content
    }
  }
}