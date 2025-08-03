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
    //       content: msg.message,
    //       sender: msg.senderId === this.loginUserId ? 'me' : 'other',
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
          //sender: m.senderId === this.loginUserId ? 'me' : 'other',
          senderId: m.senderId,
          sentAt: m.timestamp
        }));
      });
    } else {
      this.chatService.getPrivateMessages(id).subscribe((res) => {
        console.log('getPrivateMessages msg', res);
        this.messages = res.map(m => ({
          text: m.content,
          //sender: m.senderId === this.loginUserId ? 'me' : 'other',
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















// import { Component, OnInit } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { FormsModule } from '@angular/forms';
// import { ChatService } from '../../services/chat.service';

// @Component({
//   selector: 'app-chat',
//   standalone: true,
//   imports: [CommonModule, FormsModule],
//   templateUrl: './chat.component.html',
//   styleUrls: ['./chat.component.css']
// })
// export class ChatComponent implements OnInit {
//   selectedChat: any = null;
//   messageInput: string = '';
//   users: any[] = [];
//   groups: any[] = [];
//   messages: any[] = [];
//   loginUserId: string = '';

//   //#region old code
//   // groups = [
//   //   { id: 'g1', name: 'Project Team' },
//   //   { id: 'g2', name: 'Family' }
//   // ];

//   // users = [
//   //   { id: 'u1', name: 'Alice' },
//   //   { id: 'u2', name: 'Bob' },
//   //   { id: 'u3', name: 'Charlie' }
//   // ];

//   // // Dummy message storage
//   // messages: { [key: string]: { text: string; sender: 'me' | 'other'; timestamp: Date }[] } = {
//   //   g1: [
//   //     { text: 'Hey team, meeting at 3?', sender: 'other', timestamp: new Date() },
//   //     { text: 'Got it!', sender: 'me', timestamp: new Date() }
//   //   ],
//   //   u1: [
//   //     { text: 'Hi Alice!', sender: 'me', timestamp: new Date() },
//   //     { text: 'Hey! Long time.', sender: 'other', timestamp: new Date() }
//   //   ]
//   // };
//   //#endregion

//   constructor(private chatService: ChatService) {}
//   ngOnInit(): void {
//     this.loginUserId = localStorage.getItem('userId') ?? '';
//     console.log('login userId',this.loginUserId);
//     this.loadUsers();
//     this.loadGroups();
//   }

//   loadUsers() {
//     this.chatService.getUsers().subscribe((res) => (this.users = res));
//   }
//     loadGroups() {
//     this.chatService.getGroups().subscribe((res) => (this.groups = res));
//   }
//   selectChat(id: string, isGroup = false) {
//     this.selectedChat = id;
//     if( isGroup === true){
//       this.chatService.getGroupMessages(id).subscribe((res) => {
//         this.messages = res;
//         console.log('groupMessage -:', res);
//       })
//     }
//     else{
//       this.selectedChat = id;
//       this.chatService.getPrivateMessages(id).subscribe((res) =>{
//         this.messages = res;
//         console.log('privateMessage -:', res);
//       })
//     }
//   }

//   get currentMessages() {
//     return  this.messages[this.selectedChat.id];
//   }

//   sendMessage() {
//     // if (!this.messageInput.trim() || !this.selectedChat) return;

//     // const id = this.selectedChat.id;
//     // if (!this.messages[id]) this.messages[id] = [];

//     // this.messages[id].push({
//     //   text: this.messageInput,
//     //   sender: 'me',
//     //   timestamp: new Date() // ✅ Add timestamp here
//     // });

//     // this.messageInput = '';
//   }
// }
