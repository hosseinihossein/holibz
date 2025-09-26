import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, Observable } from "rxjs";
import { AuthService } from "../services/auth-service";

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const authService = inject(AuthService);

    if(authService.isLogin()){
        let token = authService.getToken();
        let cloned = req.clone({headers: req.headers.set("Authorization", "Bearer " + token)});
        return next(cloned);
    }
    else{
        return next(req);
    }
}