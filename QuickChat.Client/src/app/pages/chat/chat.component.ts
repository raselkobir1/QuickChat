import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { SignalRService } from '../../services/signalr.service'
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonService } from '../../common.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  //#region ------------- Variable declaration--------------------------
  showDropdown: boolean = false;
  // Modal & selection state
  showCreateGroup = false;
  showAddMembers = false;
  showGroupMembers = false;
  groupState = 'Leave';

  newGroupName = '';
  selectedMembers: string[] = []; // used in Create Group modal (ids)
  membersToAdd: string[] = [];    // used in Add Members to group modal (ids).
  membersToDeleted: string[] = []; // used in Delete group members.

  selectedChatId: any = null;
  selectedChatName: string = '';
  messageInput: string = '';
  users: any[] = [];
  usersInGroup: any = []
  groups: any[] = [];
  userProfile: any;
  messages: any[] = [];
  loginUserId: string = '';
  isGroupChat: boolean = false;
  connectedGroupMembers: any;
  connectedCount: number = 0;
  //#endregion
  constructor(
    private chatService: ChatService,
    private signalRService: SignalRService,
    private common: CommonService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadUserProfile();
    document.addEventListener('click', this.handleClickOutside.bind(this));
    this.loadUsers();
    this.loadGroups();
    this.signalRService.connect();
    this.signalRService.messageReceived$.subscribe((msg) => {
      console.log('signalR msg', msg);
      if (!msg) return;
      const isGroupMatch = this.isGroupChat && msg.groupId === this.selectedChatId;
      const isPrivateMatch = !this.isGroupChat &&
        (msg.senderId === this.selectedChatId || msg.receiverId === this.selectedChatId);

      if (!(isGroupMatch || isPrivateMatch)) return;

      const newMessage = {
        text: msg.content,
        senderId: msg.senderId,
        userName: msg.userName || '',
        sentAt: new Date(msg.sentAt || new Date())
      };

      this.messages = [...this.messages, newMessage];
    });

    this.signalRService.connectedUsers$.subscribe((groupWithUser) => {
      this.connectedGroupMembers = groupWithUser;
      this.connectedCount = this.connectedGroupMembers?.users?.length;
      console.log('ConnectedUsers :', this.connectedGroupMembers)
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
    });
  }

  loadUserProfile() {
    this.chatService.getCurrentUserProfile().subscribe((res) => (this.userProfile = res));
    this.loginUserId = localStorage.getItem('userId') ?? '';
  }

  //#region ----------- Chat/Message section--------------
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
          userName: m?.userName,
          sentAt: new Date(m?.sentAt || new Date)
        }));
      });
    } else {
      this.selectedChatName = this.users.find(u => u.id === id)?.userName
      this.chatService.getPrivateMessages(id).subscribe((res) => {
        this.messages = res?.map(m => ({
          text: m?.content,
          senderId: m?.senderId,
          sentAt: new Date(m?.sentAt || new Date)
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
  //#endregion

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
  onLeaveGroup(groupState: string): void {
    if (groupState === 'Join') {
      this.signalRService.joinGroup(this.selectedChatId);
      this.groupState = 'Leave';
    } else {
      this.signalRService.leaveGroup(this.selectedChatId);
      this.groupState = 'Join';
      this.connectedCount = this.connectedCount - 1;
    }
  }

  //#region ----------- Dropdown handle--------------
  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  handleClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-profile')) {
      this.showDropdown = false;
    }
  }
  //#endregion

  //#region ----------- Auto scroll to bottom (chat)---- 
  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom() {
    try {
      this.myScrollContainer.nativeElement.scrollTop =
        this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }
  //#endregion

  //#region ----------- Create Group -----------------
  openCreateGroup(): void { this.showCreateGroup = true; this.newGroupName = ''; this.selectedMembers = []; }
  closeCreateGroup(): void { this.showCreateGroup = false; this.newGroupName = ''; this.selectedMembers = []; }

  // For Create Group with if associate users.
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
      alert('Group name field must not be empty.');
      return;
    }

    //this.creatingGroup = true;
    const payload = { name, memberIds: this.selectedMembers };
    this.chatService.createGroup(payload)
      .subscribe({
        next: (res) => {
          this.closeCreateGroup();
          this.loadGroups();
        },
        error: err => this.common.handleApiError(err)
      })
  }
  //#endregion

  //#region ----------- Assign user to group --------------------
  openAddMembers() { this.showAddMembers = true; this.membersToAdd = []; }
  closeAddMembers() { this.showAddMembers = false; this.membersToAdd = []; }

  // For Assign User to Group.
  toggleAddMember(userId: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.membersToAdd.includes(userId)) {
        this.membersToAdd.push(userId);
      }
    } else {
      this.membersToAdd = this.membersToAdd.filter(id => id !== userId);
    }
  }

  addMembersToGroup() {
    const groupId = (this.selectedChatId || '').trim();
    if (!groupId) {
      alert('GroupId field must not be empty.');
      return;
    }
    if (!this.membersToAdd?.length) {
      alert('At least one user must be selected.');
      return;
    }

    const payload = { groupId: groupId, memberIds: this.membersToAdd };
    this.chatService.addMembersToGroup(payload)
      .subscribe({
        next: (res) => {
          this.closeAddMembers();
          this.loadGroups();
        },
        error: err => this.common.handleApiError(err)
      })
  }
  //#endregion

  //#region ----------- Delete user section------------------------
  openShowGroupMembers() {
    this.showGroupMembers = true;
    const group = this.groups.find((g: any) => g.id === this.selectedChatId);
    //this.usersInGroup = group?.members || [];
    //console.log(this.usersInGroup)
    this.usersInGroup = group?.members?.map((m: { email: any; userId: any; userName: any; }) => ({
      email: m?.email,
      userId: m?.userId,
      userName: m?.userName,
      status: this.connectedGroupMembers.users?.some((cu: any) => cu === m.userId) ? 'online' : 'offline'
    }));
  }
  closeShowGroupMembers() { this.showGroupMembers = false; this.usersInGroup = []; }
  // For Delete User from Group.
  toggleShowGroupMember(userId: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.membersToDeleted.includes(userId)) {
        this.membersToDeleted.push(userId);
      }
    } else {
      this.membersToDeleted = this.membersToDeleted.filter(id => id !== userId);
    }
  }

  deleteMembersFromGroup() {
    const groupId = (this.selectedChatId || '').trim();
    if (!groupId) {
      alert('GroupId field must not be empty.');
      return;
    }
    if (!this.membersToDeleted?.length) {
      alert('At least one user must be selected.');
      return;
    }
    const payload = { groupId: groupId, memberIds: this.membersToDeleted };
    console.log('delete member', payload);
    this.chatService.deleteMembersFromGroup(payload)
      .subscribe({
        next: (res) => {
          this.closeShowGroupMembers();
          this.loadGroups();
        },
        error: err => this.common.handleApiError(err)
      })
  }
  //#endregion


}