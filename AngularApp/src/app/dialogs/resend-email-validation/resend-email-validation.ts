import { Component, computed, inject, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { IdentityService } from '../../services/identity-service';
import { FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-resend-email-validation',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule, MatError],
  templateUrl: './resend-email-validation.html',
  styleUrl: './resend-email-validation.css'
})
export class ResendEmailValidation {
  readonly dialogRef = inject(MatDialogRef<ResendEmailValidation>);
  //readonly data = inject<{email:string}>(MAT_DIALOG_DATA);
  
  identityService = inject(IdentityService);

  email = signal(new FormControl("",{
    nonNullable:true,
    validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60), Validators.email]
  }));

  response = signal<{success:boolean, error:string} | null>(null);

  resendEmailValidation(){
    if(this.email().valid){
      this.identityService.resendEmailValidation(this.email().value).subscribe({
        next: res => {
          if(res.success){
            this.response.set({success: true, error: ""});
            console.log("email validation link sent successfully!");
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
            this.response.set({success: false, error: err.error.Email || err.error.errors?.Email});
            console.error(err);
          }
          else{
            throwError(()=>err);
          }
        }
      });
    }
  }

}
