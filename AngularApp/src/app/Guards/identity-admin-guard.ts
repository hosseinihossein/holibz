import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { IdentityService } from "../services/identity-service";

export const identityAdminGuard: CanActivateFn = (route, state) => {
  const identityService = inject(IdentityService);
  const router = inject(Router);
  if(identityService.isAuthenticated() && identityService.userModel()?.roles?.includes("Identity_Admins")){
    return true;
  }
  else if (identityService.isAuthenticated()){
    return router.createUrlTree(["/AccessDenied"]);
  }
  return router.createUrlTree(["/login"],{ queryParams: {returnUrl: state.url}});
}