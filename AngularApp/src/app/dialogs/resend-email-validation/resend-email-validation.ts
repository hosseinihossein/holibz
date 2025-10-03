import { Component, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { IdentityService } from '../../services/identity-service';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-resend-email-validation',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel, 
    MatButton, MatDialogClose
  ],
  templateUrl: './resend-email-validation.html',
  styleUrl: './resend-email-validation.css'
})
export class ResendEmailValidation {
  readonly dialogRef = inject(MatDialogRef<ResendEmailValidation>);
  readonly data = inject<{email:string}>(MAT_DIALOG_DATA);
  
  identityService = inject(IdentityService);

  emailFormControl = signal(new FormControl("",{
    nonNullable:true,
    validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60), Validators.email]
  }));

  resendEmailValidation(email:string){
    this.identityService.resendEmailValidation(email);
  }

}
