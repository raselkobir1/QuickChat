import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service'; // adjust path as needed
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  userTypes = [
    { id: 1, name: 'Admin' },
    { id: 2, name: 'SuperAdmin' },
    { id: 3, name: 'User' }
  ];

  error: string | null = null;
  success: string | null = null;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.registerForm = this.fb.group({
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      userType: [3, [Validators.required]], // default to 'User'
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) return;

    const payload: RegisterDto = {
      UserName: this.registerForm.value.userName,
      Email: this.registerForm.value.email,
      Password: this.registerForm.value.password,
      UserType: this.registerForm.value.userType
    };
    console.log('payload: ', payload);
    this.auth.register(payload).subscribe({
      next: (res) => {
        //this.router.navigate(['/login']);
        console.log('result:', res);
        this.error = res.message;
      },
      error: err => {
        this.error = 'Registration failed. Please try again.';
      }
    });
  }
}

interface RegisterDto {
  UserName: string;
  Email: string;
  Password: string;
  UserType: number;
}