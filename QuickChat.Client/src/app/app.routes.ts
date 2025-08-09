import { Routes } from '@angular/router';
import { WelcomeComponent } from './welcome/welcome.component';
import { ChatComponent } from './pages/chat/chat.component';
import { LoginComponent } from './pages/login/login.component';
import { authGuard } from './guards/auth.guard';
import { RegisterComponent } from './pages/register/register.component';

export const routes: Routes = [
  {path: '', redirectTo: 'welcome', pathMatch: 'full'},
  {path: 'welcome', component: WelcomeComponent},
  {path: 'chat', component: ChatComponent, canActivate:[authGuard]},
  {path: 'login', component: LoginComponent},
  {path: 'register', component: RegisterComponent},
];