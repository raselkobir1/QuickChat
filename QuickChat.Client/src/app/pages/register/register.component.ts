import { CommonService } from './../../common.service';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service'; 
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

  constructor(private fb: FormBuilder, private auth: AuthService, private common: CommonService, private router: Router) {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      userType: [3, [Validators.required]], // default to 'User'
    });
  }

  onSubmit() {
    if (this.registerForm.invalid) return;

    const payload: RegisterDto = {
      FullName: this.registerForm.value.fullName,
      Email: this.registerForm.value.email,
      Password: this.registerForm.value.password,
      UserType: this.registerForm.value.userType
    };
    console.log('payload: ', payload);
    this.auth.register(payload).subscribe({
      next: (res) => {
        alert(res.message)
        //this.router.navigate(['/login']);
      },
      error: err => this.common.handleApiError(err)
    });
  }
}

interface RegisterDto {
  FullName: string;
  Email: string;
  Password: string;
  UserType: number;
}