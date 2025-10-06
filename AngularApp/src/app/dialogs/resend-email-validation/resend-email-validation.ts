import { AfterViewInit, Component, computed, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { IdentityService } from '../../services/identity-service';
import { FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';
import { SingletonModes } from '../../services/singleton-modes';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

declare const turnstile:any;

@Component({
  selector: 'app-resend-email-validation',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule, MatError, MatProgressSpinnerModule],
  templateUrl: './resend-email-validation.html',
  styleUrl: './resend-email-validation.css'
})
export class ResendEmailValidation implements AfterViewInit {
  readonly dialogRef = inject(MatDialogRef<ResendEmailValidation>);
  
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

  response = signal<{success:boolean, error:string} | null>(null);
  displaySubmitSpinner = signal(false);
  widgetId = signal("");

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
          this.response.set({success: false, error: "Turnstile error! error code: " + errorCode});
          console.error("error-callback: " + errorCode);
        },
        'expired-callback': () => {
          this.response.set({success: false, error: "Turnstile expired!"});
          console.error("expired-callback");
        },
        'timeout-callback': () => {
          this.response.set({success: false, error: "Turnstile timeouted!"});
          console.error("timeout-callback");
        },
      })
    );
  }

  resendEmailValidation(){
    if(this.email().valid && this.cfTurnstile().valid){
      this.displaySubmitSpinner.set(true);
      this.identityService.resendEmailValidation(this.email().value, this.cfTurnstile().value).subscribe({
        next: res => {
          this.displaySubmitSpinner.set(false);
          if(res.success){
            this.response.set({success: true, error: ""});
            console.log("email validation link sent successfully!");
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error.Email || err.error.errors?.Email){
              this.response.set({success: false, error: "email error: " + (err.error.Email || err.error.errors?.Email)});
            }
            else if(err.error.TurnstileError || err.error.errors?.CfTurnstileResponse){
              this.response.set({success: false, error: "turnstile error: " + (err.error.TurnstileError || err.error.errors?.CfTurnstileResponse)});
            }
            console.error(err);
          }
          else{
            throwError(()=>err);
          }
          this.displaySubmitSpinner.set(false);
          turnstile.reset(this.widgetId());
        }
      });
    }
  }

}
