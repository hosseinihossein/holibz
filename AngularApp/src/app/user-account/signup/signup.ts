import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, computed, inject, signal, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatTooltip } from '@angular/material/tooltip';
import { validateUsername } from '../../validators/username-validator';
import { AuthService } from '../../services/auth-service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-signup',
  imports: [MatFormField,MatInput,MatButton,MatIconButton,MatSuffix,MatLabel,MatError,MatIcon,
    ReactiveFormsModule,JsonPipe
  ],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup implements AfterViewInit {
  hide = signal(true);
  signupForm = signal(new FormGroup({
    username: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.maxLength(60)],
      asyncValidators: [validateUsername()],
      updateOn: "change"
    }),
    email: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.maxLength(60)],
      //asyncValidators: [validateEmail()],
      updateOn: "change"
    }),
    password: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.maxLength(60)],
      updateOn: "change"
    }),
  }));
  username = computed(()=>this.signupForm().get("username"));
  email = computed(()=>this.signupForm().get("email"));
  password = computed(()=>this.signupForm().get("password"));

  //usernameValid = signal(false);
  authService = inject(AuthService);

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
      this.authService.signup(formValue.username!, formValue.email!, formValue.password!).subscribe({
        next: res => {
          console.log("user account created successfully!");
          //display a message that the user needs to validate their email
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error.Email){
              this.email()?.setErrors({loginError: err.error.Email});
            }
            else if(err.error.Username){
              this.username()?.setErrors({loginError: err.error.Username});
            }
            else if(err.error.Password){
              this.password()?.setErrors({loginError: err.error.Password});
            }
          }
          else{
            throwError(()=>err);
          }
        },
      });
    }
  }

}
