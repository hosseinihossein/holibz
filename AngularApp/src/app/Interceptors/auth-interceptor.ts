import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, Observable } from "rxjs";
import { AuthService } from "../services/auth-service";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const authService = inject(AuthService);

    if(authService.isAuthenticated()){
        let token = authService.getToken();
        let cloned = req.clone({setHeaders: {Authorization: "Bearer " + token}});
        return next(cloned);
    }
    else{
        return next(req);
    }
}