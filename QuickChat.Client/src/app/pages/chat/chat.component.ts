import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { SignalRService } from '../../services/signalr.service'
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  selectedChat: any = null;
  selectedChatName: string = '';
  messageInput: string = '';
  users: any[] = [];
  groups: any[] = [];
  messages: any[] = [];
  loginUserId: string = '';
  isGroupChat: boolean = false;

  constructor(
    private chatService: ChatService,
    private signalRService: SignalRService,
    private router: Router
  ) { }

  ngOnInit(): void {
    document.addEventListener('click', this.handleClickOutside.bind(this));
    this.loginUserId = localStorage.getItem('userId') ?? '';
    this.loadUsers();
    this.loadGroups();

    this.signalRService.connect();
    //this.signalRService.messageReceived$.subscribe((msg=> this.messages = msg))
    //    this.signalRService.messageReceived$.subscribe((msg) => {
    //   this.messages = [...this.messages, msg];
    // });
    // this.signalRService.messageReceived$.subscribe((msg) => {
    //   this.messages = [
    //     ...this.messages,
    //     {
    //       text: msg[0].content,
    //       senderId: msg[0].senderId,
    //       sentAt: msg[0].sentAt
    //     }
    //   ];
    // });

    this.signalRService.messageReceived$.subscribe((msg) => {
      const isForCurrentChat =true;
        // (this.isGroupChat && msg?.groupId === this.selectedChat) ||
        // (!this.isGroupChat &&
        //   (msg?.senderId === this.selectedChat || msg?.receiverId === this.selectedChat));

      //if (isForCurrentChat) {
        this.messages = [...this.messages,
        {
          text: msg?.content,
          senderId: msg?.senderId,
          sentAt: msg?.sentAt
        }];
      //}
    });
  }

  ngOnDestroy(): void {
    document.removeEventListener('click', this.handleClickOutside.bind(this));
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
      this.selectedChatName = this.groups.find(g => g.id === id)?.name

      this.chatService.getGroupMessages(id).subscribe((res) => {
        console.log('getGroupMessages msg', res);
        this.messages = res?.map(m => ({
          text: m?.content,
          senderId: m?.senderId,
          sentAt: m?.timestamp
        }));
      });
    } else {
      this.selectedChatName = this.users.find(g => g.id === id)?.userName
      this.chatService.getPrivateMessages(id).subscribe((res) => {
        console.log('getPrivateMessages msg', res);
        this.messages = res?.map(m => ({
          text: m?.content,
          senderId: m?.senderId,
          sentAt: m?.timestamp
        }));
        console.log('loginUserId', this.loginUserId);
      });
    }
  }

  sendMessage() {
    if (!this.messageInput.trim() || !this.selectedChat) return;

    const message = this.messageInput;
    this.messageInput = '';
    if (this.isGroupChat) {
      this.signalRService.sendMessageToGroup(this.selectedChat, message); // groupId, content, 
    } else {
      this.signalRService.sendMessageToUser(this.selectedChat, message); // receiverId, content
    }
  }
   toUIMessage(msg: any) {
  return {
    text: msg.content,
    senderId: msg.senderId,
    sentAt: msg.sentAt
  };
}

showDropdown: boolean = false;

toggleDropdown(): void {
  this.showDropdown = !this.showDropdown;
}

goToSettings(): void {
  this.showDropdown = false;
  // Replace with your real routing/navigation
  console.log('Navigating to settings...');
  // this.router.navigate(['/settings']);
}
logout():void{
  localStorage.removeItem('access_token');
  this.router.navigate(['welcome']);
}


//-------------------
handleClickOutside(event: MouseEvent): void {
  const target = event.target as HTMLElement;
  if (!target.closest('.user-profile')) {
    this.showDropdown = false;
  }
}

}