import { HttpErrorResponse, HttpStatusCode } from "@angular/common/http";
import { ErrorHandler, inject, Injectable } from "@angular/core";
import { AuthService } from "./services/auth-service";

@Injectable()
export class AppErrorHandler implements ErrorHandler{
    authService = inject(AuthService);

    handleError(error: any): void {
        const err = error.rejection || error;
        let errTypeMessage = "";

        if(err instanceof HttpErrorResponse){
            switch(err.status){
                case 0:
                    errTypeMessage = "Client Error";
                    break;
                case HttpStatusCode.Unauthorized:
                    errTypeMessage = "Unauthorized";
                    this.authService.logout();
                    break;
                case HttpStatusCode.Forbidden:
                    errTypeMessage = "Access Denied";
                    break;
                default:
                    errTypeMessage = "Unknown Error";
            }
        }
        else{
            errTypeMessage = "Application Error";
        }

        console.error(errTypeMessage, err);
    }
    
}