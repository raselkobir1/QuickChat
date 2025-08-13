import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  template: `<div class="p-4">Signing in…</div>`
})
export class AuthCallbackComponent implements OnInit {
  constructor(private router: Router) { }

  ngOnInit() {
    //-----------way 1
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');
    if (token && window.opener) {
      // send to opener and close
      window.opener.postMessage({ token }, window.location.origin);
      window.close();
    } else if (token) {
      // if opened directly, just store and navigate
      localStorage.setItem('access_token', token);
    } else {
      this.router.navigate(['/login']);
    }
    this.router.navigate(['/chat']);

    //-----------way 2
    // window.addEventListener('message', (event) => {
    //   //if (event.origin !== window.location.origin) return; // security check
    //   const allowedOrigins = ['http://localhost:4200', 'https://myapp.com'];
    //   if (!allowedOrigins.includes(event.origin)) return;
    //   const { token } = event.data;
    //   if (token) {
    //     localStorage.setItem('jwtToken', token);
    //     this.router.navigate(['/dashboard']);
    //   }
    // });

    //-----------way 3
    // this.router.queryParams.subscribe(params => {
    //     const token = params['token'];
    //     if (token) {
    //       localStorage.setItem('authToken', token);
    //       this.router.navigate(['/dashboard']);
    //     } else {
    //       this.router.navigate(['/login']);
    //     }
    //   });

  }
}

