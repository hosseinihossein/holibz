import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, computed, signal, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from "@angular/material/input";

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
}
