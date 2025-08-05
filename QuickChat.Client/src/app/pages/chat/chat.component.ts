import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
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
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  // Modal & selection state
  showCreateGroup = false;
  showAddMembers = false;

  newGroupName = '';
  selectedMembers: string[] = []; // used in Create Group modal (ids)
  membersToAdd: string[] = [];    // used in Add Members modal (ids)

  // simple loading flags
  creatingGroup = false;
  addingMembers = false;


  selectedChatId: any = null;
  selectedChatName: string = '';
  messageInput: string = '';
  users: any[] = [];
  groups: any[] = [];
  userProfile: any;
  messages: any[] = [];
  loginUserId: string = '';
  isGroupChat: boolean = false;

  constructor(
    private chatService: ChatService,
    private signalRService: SignalRService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadUserProfile();
    document.addEventListener('click', this.handleClickOutside.bind(this));
    this.loadUsers();
    this.loadGroups();

    this.signalRService.connect();
    this.signalRService.messageReceived$.subscribe((msg) => {
      const isForCurrentChat = true;
      // (this.isGroupChat && msg?.groupId === this.selectedChatId) ||
      // (!this.isGroupChat &&
      //   (msg?.senderId === this.selectedChatId || msg?.receiverId === this.selectedChatId));

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
    this.chatService.getGroups().subscribe((res) => {
      this.groups = res;
      console.log('user groups: ',res);
    });
  }

  loadUserProfile() {
    this.chatService.getCurrentUserProfile().subscribe((res) => (this.userProfile = res));
    this.loginUserId = localStorage.getItem('userId') ?? '';
  }

  selectChat(id: string, isGroup = false) {
    this.selectedChatId = id;
    this.isGroupChat = isGroup;
    this.messages = [];

    if (isGroup) {
      this.signalRService.joinGroup(id); // ✅ Join group on SignalR
      this.selectedChatName = this.groups.find(g => g.id === id)?.name

      this.chatService.getGroupMessages(id).subscribe((res) => {
        this.messages = res?.map(m => ({
          text: m?.content,
          senderId: m?.senderId,
          sentAt: m?.timestamp
        }));
      });
    } else {
      this.selectedChatName = this.users.find(u => u.id === id)?.userName
      this.chatService.getPrivateMessages(id).subscribe((res) => {
        console.log('getPrivateMessages msg', res);
        this.messages = res?.map(m => ({
          text: m?.content,
          senderId: m?.senderId,
          sentAt: m?.timestamp
        }));
      });
    }
  }

  sendMessage() {
    if (!this.messageInput.trim() || !this.selectedChatId) return;

    const message = this.messageInput;
    this.messageInput = '';
    if (this.isGroupChat) {
      this.signalRService.sendMessageToGroup(this.selectedChatId, message); // groupId, content, 
    } else {
      this.signalRService.sendMessageToUser(this.selectedChatId, message); // receiverId, content
    }
  }

  showDropdown: boolean = false;

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  goToSettings(): void {
    this.showDropdown = false;
    console.log('Navigating to settings...');
  }
  logout(): void {
    localStorage.removeItem('access_token');
    this.router.navigate(['welcome']);
  }
  goToProfile(): void {

  }
  changePassword(): void {

  }
  toggleTheme(): void {

  }
  //---------------------------------------------------
  handleClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-profile')) {
      this.showDropdown = false;
    }
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom() {
    try {
      this.myScrollContainer.nativeElement.scrollTop =
        this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }

  //----------------------------------------------------------------------------------

  // methods
  // ----------------- Create Group -----------------
  openCreateGroup(): void {
    this.showCreateGroup = true;
    this.newGroupName = '';
    this.selectedMembers = [];
  }

  closeCreateGroup(): void {
    this.showCreateGroup = false;
    this.newGroupName = '';
    this.selectedMembers = [];
  }

  toggleSelectedMember(userId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.selectedMembers.includes(userId)) {
        this.selectedMembers.push(userId);
      }
    } else {
      this.selectedMembers = this.selectedMembers.filter(id => id !== userId);
    }
  }

  createGroup(): void {
    const name = (this.newGroupName || '').trim();
    if (!name) {
      return;
    }

    this.creatingGroup = true;
    const payload = {
      name,
      memberIds: this.selectedMembers // could be empty -> group with only creator
    };
    console.log('group create: ',payload);
    this.chatService.createGroup(payload)
      .subscribe({
        next: (res) =>{
          this.closeCreateGroup();
          this.reloadRoute();
          console.log('Group created successfully:', res)
        },
        error: (err) => console.error('Error creating group:', err)
      })
  }

  reloadRoute() {
  this.router.navigate([this.router.url])
    .then(() => {
      console.log('Route reloaded');
    });
}




  openAddMembers() { this.showAddMembers = true; }
  closeAddMembers() { this.showAddMembers = false; this.membersToAdd = []; }
  toggleAddMember(id: string, event: Event) { /* add/remove from membersToAdd */ }
  addMembers() { /* call API to add members into selectedChatId; close; optionally notify via SignalR */ }

}