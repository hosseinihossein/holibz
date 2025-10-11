import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, Observable } from "rxjs";
import { IdentityService } from "../services/identity-service";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const identityService = inject(IdentityService);

    if(identityService.isAuthenticated()){
        let token = identityService.token();
        let cloned = req.clone({setHeaders: {Authorization: "Bearer " + token}});
        return next(cloned);
    }
    else{
        return next(req);
    }
}