import { Routes } from '@angular/router';
import { WelcomeComponent } from './welcome/welcome.component';

export const routes: Routes = [
  {path: '', redirectTo: 'join-room', pathMatch: 'full'},
  //{path: 'join-room', component: JoinRoomComponent},
  {path: 'welcome', component: WelcomeComponent},
  //{path: 'chat', component: ChatComponent}
];