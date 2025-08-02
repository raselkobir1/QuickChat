import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {
  selectedChat: any = null;
  messageInput: string = '';

  groups = [
    { id: 'g1', name: 'Project Team' },
    { id: 'g2', name: 'Family' }
  ];

  users = [
    { id: 'u1', name: 'Alice' },
    { id: 'u2', name: 'Bob' },
    { id: 'u3', name: 'Charlie' }
  ];

  // Dummy message storage
  messages: { [key: string]: { text: string; sender: 'me' | 'other'; timestamp: Date }[] } = {
    g1: [
      { text: 'Hey team, meeting at 3?', sender: 'other', timestamp: new Date() },
      { text: 'Got it!', sender: 'me', timestamp: new Date() }
    ],
    u1: [
      { text: 'Hi Alice!', sender: 'me', timestamp: new Date() },
      { text: 'Hey! Long time.', sender: 'other', timestamp: new Date() }
    ]
  };

  selectChat(chat: any) {
    this.selectedChat = chat;
  }

  get currentMessages() {
    return this.selectedChat ? this.messages[this.selectedChat.id] || [] : [];
  }

  sendMessage() {
    if (!this.messageInput.trim() || !this.selectedChat) return;

    const id = this.selectedChat.id;
    if (!this.messages[id]) this.messages[id] = [];

    this.messages[id].push({
      text: this.messageInput,
      sender: 'me',
      timestamp: new Date() // ✅ Add timestamp here
    });

    this.messageInput = '';
  }
}
