import { JsonPipe } from '@angular/common';
import { AfterViewInit, Component, computed, signal, viewChildren } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-signup',
  imports: [MatFormField,MatInput,MatButton,MatIconButton,MatSuffix,MatLabel,MatError,MatIcon,
    ReactiveFormsModule,MatTooltip,JsonPipe
  ],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup implements AfterViewInit {
  hide = signal(true);
  signupForm = signal(new FormGroup({
    username: new FormControl("",[Validators.required, Validators.maxLength(60)],[]),
    email:  new FormControl("",[Validators.required, Validators.maxLength(60)],[]),
    password:  new FormControl("",[Validators.required, Validators.maxLength(60)]),
  }));
  username = computed(()=>this.signupForm().get("username"));
  email = computed(()=>this.signupForm().get("email"));
  password = computed(()=>this.signupForm().get("password"));

  usernameValid = signal(false);

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

  checkUsernameValidation(){}
}
