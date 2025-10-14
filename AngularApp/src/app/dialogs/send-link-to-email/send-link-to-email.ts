import { AfterViewInit, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { IdentityService } from '../../services/identity-service';
import { SingletonModes } from '../../services/singleton-modes';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButton } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Result, ResultDialogInputData } from '../result/result';

declare const turnstile:any;

@Component({
  selector: 'app-send-link-to-email',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule, MatError, MatProgressSpinnerModule],
  templateUrl: './send-link-to-email.html',
  styleUrl: './send-link-to-email.css'
})
export class SendLinkToEmail implements AfterViewInit {
  readonly sendLinkToEmailDialogRef = inject(MatDialogRef<SendLinkToEmail>);
  readonly data = inject<{purpose:string}>(MAT_DIALOG_DATA);
  dialog = inject(MatDialog);
  
  identityService = inject(IdentityService);
  readonly singletonModes = inject(SingletonModes);

  email = signal(new FormControl("",{
    nonNullable:true,
    validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60), Validators.email]
  }));
  cfTurnstile = signal(new FormControl("",{
    nonNullable:true,
    validators:[Validators.required]
  }));

  displaySubmitSpinner = signal(false);
  widgetId = signal("");

  dialogTitle = signal("Send Link to Email");

  constructor(){
    if(this.data.purpose === "resendEmailValidation"){
      this.dialogTitle.set("Resending Email Validation Link");
    }
    else if(this.data.purpose === "forgetPassword"){
      this.dialogTitle.set("Sending Email to Proceed Reseting Password");
    }
    else if(this.data.purpose === "changeEmail"){
      this.dialogTitle.set("Sending Email Validation Link to New Email");
    }
  }

  ngAfterViewInit(): void {
    this.widgetId.set(
      turnstile.render("#resendEmail-widget-container", {
        sitekey: this.singletonModes.turnstileSiteKey,
        size: "flexible",
        theme: this.singletonModes.darkMode() ? "dark" : "light",
        "response-field": false,
        action: "resend-email-validation",
        "refresh-expired": "manual",
        "refresh-timeout": "manual",
        callback: (token:string) => {
          this.cfTurnstile()?.setValue(token);
        },
        'error-callback': (errorCode: string) => {
          this.cfTurnstile().setErrors({turnstileError: "Turnstile error! error code: " + errorCode});
          console.error("error-callback: " + errorCode);
        },
        'expired-callback': () => {
          this.cfTurnstile().setErrors({turnstileError: "Turnstile expired!"});
          console.error("expired-callback");
        },
        'timeout-callback': () => {
          this.cfTurnstile().setErrors({turnstileError: "Turnstile timeouted!"});
          console.error("timeout-callback");
        },
      })
    );
  }

  requestSendingLinkToEmail(){
    if(this.email().valid && this.cfTurnstile().valid){
      this.displaySubmitSpinner.set(true);

      const callBacks = {
        next: (res:{success:boolean}) => {
          if(res.success){
            console.log("Email validation link sent successfully!");
            const resultInputData = new ResultDialogInputData();
            resultInputData.status = "success";
            resultInputData.title = "Success";
            resultInputData.description = [
              "A validation link sent to your email successfully.",
              "Please check your email and click on the validation link to proceed."
            ];
            const resultDialogRef = this.dialog.open(Result,{data: resultInputData});
            resultDialogRef.afterClosed().subscribe(() => {
              this.displaySubmitSpinner.set(false);
              this.sendLinkToEmailDialogRef.close();
            });
          }
        },
        error: (err:any) => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error.Email || err.error.errors?.Email){
              this.email().setErrors({submitError: err.error.Email || err.error.errors?.Email});
            }
            else if(err.error.TurnstileError || err.error.errors?.CfTurnstileResponse){
              this.cfTurnstile().setErrors({submitError: err.error.TurnstileError || err.error.errors?.CfTurnstileResponse});
            }
            console.error("err: "+JSON.stringify(err));
            console.error("err.error: "+JSON.stringify(err.error));
            console.error("err.error.errors: "+JSON.stringify(err.error.errors));
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
          turnstile.reset(this.widgetId());
        }
      }

      if(this.data.purpose === "resendEmailValidation"){
        this.identityService.resendEmailValidation(
          this.email().value, this.cfTurnstile().value).subscribe(callBacks);
      }
      else if(this.data.purpose === "forgetPassword"){
        this.identityService.forgetPassword(
          this.email().value, this.cfTurnstile().value).subscribe(callBacks);
      }
      else if(this.data.purpose === "changeEmail"){
        this.identityService.changeEmail(
          this.email().value, this.cfTurnstile().value).subscribe(callBacks);
      }
    }
  }
}
