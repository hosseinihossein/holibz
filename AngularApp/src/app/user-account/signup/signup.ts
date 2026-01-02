import { JsonPipe } from '@angular/common';
import { AfterViewChecked, AfterViewInit, Component, computed, effect, inject, signal, viewChild, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatTooltip } from '@angular/material/tooltip';
import { validateUsername } from '../../validators/username-validator';
import { IdentityService } from '../../services/identity-service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { Result } from '../../dialogs/result/result';
import { SingletonModes } from '../../services/singleton-modes';
import { RouterLink } from '@angular/router';

declare const turnstile : any;

@Component({
  selector: 'app-signup',
  imports: [MatFormField, MatInput, MatButton, MatIconButton, MatSuffix, MatLabel, MatError, MatIcon,
    ReactiveFormsModule, MatProgressSpinner, RouterLink],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup implements AfterViewInit {
  hide = signal(true);
  signupForm = signal(new FormGroup({
    username: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.minLength(3), Validators.maxLength(60)],
      asyncValidators: [validateUsername()],
      updateOn: "change"
    }),
    email: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60), Validators.email],
      //asyncValidators: [validateEmail()],
      updateOn: "change"
    }),
    password: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60)],
      updateOn: "change"
    }),
    CfTurnstileResponse: new FormControl("",{nonNullable: true, validators: [Validators.required]})
  }));
  username = computed(()=>this.signupForm().controls["username"]);
  email = computed(()=>this.signupForm().controls["email"]);
  password = computed(()=>this.signupForm().controls["password"]);
  cfTurnstile = computed(()=>this.signupForm().controls["CfTurnstileResponse"]);

  //errorResponse = signal<object | null>(null);
  displaySubmitSpinner = signal(false);
  widgetId = signal("");

  identityService = inject(IdentityService);
  readonly dialog = inject(MatDialog);
  readonly singletonModes = inject(SingletonModes);
  
  formFields = viewChildren(MatFormField);
  
  ngAfterViewInit(): void {
    for(let formField of this.formFields()){
      formField.subscriptSizing = "dynamic";
    }

    this.widgetId.set(
      turnstile.render("#widget-container", {
        sitekey: this.singletonModes.turnstileSiteKey,
        size: "flexible",
        theme: this.singletonModes.darkMode() ? "dark" : "light",
        "response-field": false,
        action: "signup",
        "refresh-expired": "manual",
        "refresh-timeout": "manual",
        callback: (token:string) => {
          const errors = this.signupForm().errors;
          this.cfTurnstile()?.setValue(token);
          this.signupForm().setErrors(errors);
        },
        'error-callback': (errorCode: string) => {
          this.signupForm().setErrors({turnstileError: "Turnstile error! error code: " + errorCode});
          console.error("error-callback: " + errorCode);
        },
        'expired-callback': () => {
          this.signupForm().setErrors({turnstileError: "Turnstile expired!"});
          console.error("expired-callback");
        },
        'timeout-callback': () => {
          this.signupForm().setErrors({turnstileError: "Turnstile timeouted!"});
          console.error("timeout-callback");
        },
      })
    );
  }

  changeVisibility(){
    if(this.hide()){
      this.hide.set(false);
    }
    else{
      this.hide.set(true);
    }
  }

  //checkUsernameValidation(){}

  signup(){
    if(this.signupForm().valid){
      this.displaySubmitSpinner.set(true);
      let formValue = this.signupForm().value;
      this.identityService.signup(formValue).subscribe({
        next: res => {
          if(res.success){
            this.displaySubmitSpinner.set(false);
            //display a message to users that they need to validate their email
            this.dialog.open(Result,{
              //panelClass: "success-ResultStatus", 
              data:{
                status: "success",
                title: "New User Account",
                description: ["Your account created successfully.",
                  `An email with a validation link has just sent to your registered email address '${this.email()?.value}'.`,
                  "You need to click the validation link to confirm your email before you can login.",
                  "Your email validation link expires in 10 hours."],
                link: {name: "Login", address: "/login"}
              }
            });

            console.log("user account created successfully!");
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error?.Email || err.error?.errors?.Email){
              this.email()?.setErrors({signupError: err.error?.Email || err.error?.errors?.Email});
            }
            else if(err.error?.Username || err.error?.errors?.Username){
              this.username()?.setErrors({signupError: err.error?.Username || err.error?.errors?.Username});
            }
            else if(err.error?.Password || err.error?.errors?.Password){
              this.password()?.setErrors({signupError: err.error?.Password || err.error?.errors?.Password});
            }
            else if(err.error?.Signup){
              this.signupForm().setErrors({signupError: err.error?.Signup});
            }
            else if(err.error?.TurnstileError || err.error.errors?.CfTurnstileResponse){
              this.signupForm().setErrors({turnstileError: err.error?.TurnstileError || err.error.errors?.CfTurnstileResponse});
            }
            else{
              this.signupForm().setErrors({signupError: err.error});
            }
            //this.errorResponse.set(err.error);
          }
          else{
            throw(err);
          }
          this.displaySubmitSpinner.set(false);
          turnstile.reset(this.widgetId());
        },
      });
    }
  }

}
