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

@Component({
  selector: 'app-signup',
  imports: [MatFormField, MatInput, MatButton, MatIconButton, MatSuffix, MatLabel, MatError, MatIcon,
    ReactiveFormsModule, JsonPipe, MatProgressSpinner],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup implements AfterViewInit {
  hide = signal(true);
  signupForm = signal(new FormGroup({
    username: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60)],
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
  }));
  username = computed(()=>this.signupForm().get("username"));
  email = computed(()=>this.signupForm().get("email"));
  password = computed(()=>this.signupForm().get("password"));

  errorResponse = signal<object | null>(null);

  //usernameValid = signal(false);
  identityService = inject(IdentityService);
  
  formFields = viewChildren(MatFormField);
  
  ngAfterViewInit(): void {
    for(let formField of this.formFields()){
      formField.subscriptSizing = "dynamic";
    }
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
      let formValue = this.signupForm().value;
      this.identityService.signup(formValue.username!, formValue.email!, formValue.password!).subscribe({
        next: res => {
          if(res.success){
            console.log("user account created successfully!");
            //display a message that the user needs to validate their email
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
            else{
              this.signupForm().setErrors({signupError: err.error});
            }
            this.errorResponse.set(err.error);
          }
          else{
            throwError(()=>err);
          }
        },
      });
    }
  }

}
