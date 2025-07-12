import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ChatService } from '../chat.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  chatService = inject(ChatService);
  router = inject(Router);
  inputMessage = "";
  messages: any[] = [];
  loggedInUserName: string = '';
  roomName: string = '';

  ngOnInit(): void {
    this.loggedInUserName = sessionStorage.getItem("user") || '';
    this.roomName = sessionStorage.getItem("room") || '';

    this.chatService.messages$.subscribe(messages => {
      this.messages = messages;
      console.log("Messages updated:", this.messages);
    })

    this.chatService.connectedUsers$.subscribe(users => {
      console.log("Connected users:", users);
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { 
      console.error(err);
    }
  }

  sendMessage() {
      this.chatService.sendMessage(this.inputMessage)
        .catch(err => console.error(err));
      this.inputMessage = '';
  }

  leaveChat() {
    this.chatService.leaveChat()
      .then(() => {
        this.router.navigate(['welcome']);
        setTimeout(() => {
          location.reload();
        }, 0);
      })
      .catch(err => console.error(err));
  }

}
