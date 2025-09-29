import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityService } from '../services/identity-service';

export const authGuard: CanActivateFn = (route, state) => {
  const identityService = inject(IdentityService);
  const router = inject(Router);
  if(identityService.isAuthenticated()){
    return true;
  }
  return router.createUrlTree(["/login"],{ queryParams: {returnUrl: state.url}});
};
