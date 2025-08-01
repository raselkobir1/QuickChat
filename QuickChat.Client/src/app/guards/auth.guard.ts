import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isLoggedIn = !!authService.getToken();
  if (isLoggedIn) {
    return true;
  }
  else {
    router.navigate(['/login']);
    return false;
  }
};
