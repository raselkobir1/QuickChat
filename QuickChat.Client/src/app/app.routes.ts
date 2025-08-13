import { Routes } from '@angular/router';
import { WelcomeComponent } from './welcome/welcome.component';
import { ChatComponent } from './pages/chat/chat.component';
import { LoginComponent } from './pages/login/login.component';
import { authGuard } from './guards/auth.guard';
import { RegisterComponent } from './pages/register/register.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AuthCallbackComponent } from './pages/auth-callback/auth-callback.component';
import { AuthFailedComponent } from './pages/auth-failed/auth-failed.component';
import { AccessDeniedComponent } from './pages/access-denied/access-denied.component';

export const routes: Routes = [
  { path: '', redirectTo: 'welcome', pathMatch: 'full' },
  { path: 'welcome', component: WelcomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'chat', component: ChatComponent, canActivate: [authGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: 'auth-failed', component: AuthFailedComponent },
  { path: 'access-denied', component: AccessDeniedComponent }
];