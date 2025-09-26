import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, computed, inject, signal, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from "@angular/material/input";
import { AuthService } from '../../services/auth-service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [MatFormField,MatInput,MatLabel,MatError,MatIcon,MatButton,MatIconButton,MatSuffix,
    ReactiveFormsModule,JsonPipe
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements AfterViewInit {
  hide = signal(true);
  loginForm = signal(new FormGroup({
    user: new FormControl("",[Validators.required, Validators.maxLength(60)]),
    password: new FormControl("",[Validators.required, Validators.maxLength(60)])
  }));
  user = computed(()=>this.loginForm().get("user"));
  password = computed(()=>this.loginForm().get("password"));

  authService = inject(AuthService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  
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

  login(){
    if(this.loginForm().valid){
      let formValue = this.loginForm().value;
      this.authService.login(formValue.user!, formValue.password!).subscribe({
        next: res => {
          console.log("token: ", res.token);
          let returnUrl = this.activatedRoute.snapshot.queryParamMap.get("returnUrl") || "/";
          this.router.navigateByUrl(returnUrl);
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            if(err.error.Activity){
              this.user()?.setErrors({loginError: "err.error.Activity"});
            }
            else if(err.error.Username){
              this.user()?.setErrors({loginError: "err.error.Username"});
            }
            else if(err.error.Password){
              this.password()?.setErrors({loginError: "err.error.Password"});
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
