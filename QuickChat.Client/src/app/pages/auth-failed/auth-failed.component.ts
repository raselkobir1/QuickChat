import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth-failed',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './auth-failed.component.html',
  styleUrl: './auth-failed.component.css'
})
export class AuthFailedComponent {
 constructor(private router: Router) {}

  retryLogin() {
    this.router.navigate(['/login']);
  }
}
