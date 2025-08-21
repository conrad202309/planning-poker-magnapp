import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';

export const userSetupGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const router = inject(Router);

  if (userService.isUserSetupComplete()) {
    return true;
  } else {
    router.navigate(['/setup']);
    return false;
  }
};
