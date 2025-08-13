import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  totalUsers = 0;
  totalMessages = 0;
  totalGroups = 0;
  profile: any;

  constructor(private chatdService: ChatService) {}

  ngOnInit() {
    this.loadStats();
    this.loadUserProfile();
  }

  loadStats() {
    this.chatdService.getStats().subscribe((data) => {
      this.totalUsers = data.totalUsers;
      this.totalMessages = data.totalMessages;
      this.totalGroups = data.totalGroups;
    });
  }

  loadUserProfile() {
    this.chatdService.getCurrentUserProfile().subscribe((profile) => {
      this.profile = profile;
    })
  }
}
