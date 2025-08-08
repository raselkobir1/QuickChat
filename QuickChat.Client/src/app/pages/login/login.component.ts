import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { CommonService } from '../../common.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginData = {
    email: '',
    password: ''
  };
  error: string | null = null;
  constructor(private authService: AuthService, private common: CommonService, private router: Router) { }

onSubmit() {
  console.log('Login form submitted:', this.loginData);

  this.authService.login(this.loginData).subscribe({
    next: () => this.router.navigate(['/chat']),
    error: err => this.common.handleApiError(err)
  });
}

}