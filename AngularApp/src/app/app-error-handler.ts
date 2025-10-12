import { HttpErrorResponse, HttpStatusCode } from "@angular/common/http";
import { ErrorHandler, inject, Injectable } from "@angular/core";
import { IdentityService } from "./services/identity-service";
import { MatSnackBar } from "@angular/material/snack-bar";

@Injectable()
export class AppErrorHandler implements ErrorHandler{
    identityService = inject(IdentityService);
    private snackBar = inject(MatSnackBar);

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
                    this.identityService.logout();
                    break;
                case HttpStatusCode.Forbidden:
                    errTypeMessage = "Access Denied";
                    break;
                case HttpStatusCode.BadRequest:
                    errTypeMessage = "Bad Request";
                    break;
                default:
                    errTypeMessage = "Unknown Error";
            }
        }
        else{
            errTypeMessage = "Application Error";
        }

        this.snackBar.open(errTypeMessage, "Ok"/*, { duration: 5000 }*/);
        console.error(errTypeMessage, err);
    }
    
}