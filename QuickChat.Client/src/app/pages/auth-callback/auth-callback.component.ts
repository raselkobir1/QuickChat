import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  template: `<div class="p-4">Signing in…</div>`
})
export class AuthCallbackComponent implements OnInit {
  constructor(private router: Router, private authService: AuthService) { }
  ngOnInit() {
    //-----------way 1
    const params = new URLSearchParams(window.location.search);
    const accessToken = params.get('accessToken') || '';
    const refreshToken = params.get('refreshToken') || '';
    if (accessToken && refreshToken) { //&& window.opener
      // send to opener and close
      // window.opener.postMessage({ token }, window.location.origin);
      // window.close();
      this.authService.storeToken(accessToken, refreshToken);
      this.router.navigate(['/chat']);
    } else {
      this.router.navigate(['/auth-faild']);
    }
  }
}

