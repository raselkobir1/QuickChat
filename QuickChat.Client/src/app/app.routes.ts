import { Routes } from '@angular/router';
import { WelcomeComponent } from './welcome/welcome.component';
import { JoinRoomComponent } from './join-room/join-room.component';
import { ChatComponent } from './chat/chat.component';
import { LoginComponent } from './pages/login/login.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {path: '', redirectTo: 'welcome', pathMatch: 'full'},
  {path: 'join-room', component: JoinRoomComponent},
  {path: 'welcome', component: WelcomeComponent},
  {path: 'chat', component: ChatComponent, canActivate:[authGuard]},
  {path: 'login', component: LoginComponent}
];