import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, computed, inject, signal, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from "@angular/material/input";
import { IdentityService } from '../../services/identity-service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { SingletonModes } from '../../services/singleton-modes';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SendLinkToEmail } from '../../dialogs/send-link-to-email/send-link-to-email';

declare const turnstile:any;

@Component({
  selector: 'app-login',
  imports: [MatFormField, MatInput, MatLabel, MatError, MatIcon, MatButton, MatIconButton, MatSuffix,
    ReactiveFormsModule, JsonPipe, MatProgressSpinnerModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements AfterViewInit {
  hide = signal(true);
  loginForm = signal(new FormGroup({
    UsernameOrEmail: new FormControl("",{nonNullable: true, validators: [Validators.required, Validators.maxLength(60)]}),
    Password: new FormControl("",{nonNullable: true, validators: [Validators.required, Validators.maxLength(60)]}),
    CfTurnstileResponse: new FormControl("",{nonNullable: true, validators: [Validators.required]})
  }));
  usernameOrEmail = computed(()=>this.loginForm().controls["UsernameOrEmail"]);
  password = computed(()=>this.loginForm().controls["Password"]);
  cfTurnstile = computed(()=>this.loginForm().controls["CfTurnstileResponse"]);

  errorResponse = signal<object | null>(null);
  displaySubmitSpinner = signal(false);
  widgetId = signal("");

  identityService = inject(IdentityService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
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
        action: "login",
        "refresh-expired": "manual",
        "refresh-timeout": "manual",
        callback: (token:string) => {
          const errors = this.loginForm().errors;
          this.cfTurnstile()?.setValue(token);
          for(let error in errors){
            if(error !== "turnstileError"){
              this.loginForm().setErrors(errors[error]);
            }
          }
          //console.log("Challenge completed:", token);
        },
        'error-callback': (errorCode: string) => {
          this.loginForm().setErrors({turnstileError: "Turnstile error! error code: " + errorCode});
          console.error("error-callback: " + errorCode);
        },
        'expired-callback': () => {
          this.loginForm().setErrors({turnstileError: "Turnstile expired!"});
          console.error("expired-callback");
        },
        'timeout-callback': () => {
          this.loginForm().setErrors({turnstileError: "Turnstile timeouted!"});
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

  login(){
    if(this.loginForm().valid){
      this.displaySubmitSpinner.set(true);
      let formValue = this.loginForm().value;
      this.identityService.login(formValue).subscribe({
        next: res => {
          //console.log("token: ", res.token);
          //console.log("login successfully!");
          //console.log("userModel: "+ JSON.stringify(res.user));
          let returnUrl = this.activatedRoute.snapshot.queryParamMap.get("returnUrl") || "/";
          this.router.navigateByUrl(returnUrl);
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            //console.error("BadRequest err: "+err.error);
            if(err.error.Inactive){
              this.usernameOrEmail()?.setErrors({loginError: err.error.Inactive});
            }
            else if(err.error.Username || err.error.errors?.UsernameOrEmail){
              this.usernameOrEmail()?.setErrors({loginError: err.error.Username || err.error.errors?.Username});
            }
            else if(err.error.Password || err.error.errors?.Password){
              this.password()?.setErrors({loginError: err.error.Password || err.error.errors?.Password});
            }
            else if(err.error.EmailValidation){
              this.loginForm().setErrors({emailValidationError: err.error.EmailValidation});
            }
            else if(err.error.TurnstileError || err.error.errors?.CfTurnstileResponse){
              this.loginForm().setErrors({turnstileError: err.error.TurnstileError || err.error.errors?.CfTurnstileResponse});
            }
            else{
              this.loginForm().setErrors({loginError: err.error});
            }
            this.errorResponse.set(err.error);
          }
          else{
            throwError(()=>err);
          }
          this.displaySubmitSpinner.set(false);
          turnstile.reset(this.widgetId());
        },
      });
    }
  }

  resendEmailValidation(){
    this.dialog.open(SendLinkToEmail, {data:{purpose:"resendEmailValidation"}});
  }
  forgetPassword(){
    this.dialog.open(SendLinkToEmail, {data:{purpose:"forgetPassword"}});
  }

  

}
