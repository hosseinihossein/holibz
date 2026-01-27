import { Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { compareTwoInputs } from '../../validators/compare-two-inputs';
import { MatDialog, MatDialogActions, MatDialogClose, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButton } from '@angular/material/button';
import { IdentityService } from '../../services/identity-service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Result, ResultDialogInputData } from '../result/result';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { WaitSpinner } from '../../shared/wait-spinner/wait-spinner';

@Component({
  selector: 'app-change-password',
  imports: [MatDialogContent, MatDialogActions, MatFormField, MatInput, MatLabel,
    MatButton, MatDialogClose, ReactiveFormsModule, MatError, WaitSpinner],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePassword {
  readonly changePasswordDialogRef = inject(MatDialogRef<ChangePassword>);
  //readonly data = inject<{label:string, value:string}>(MAT_DIALOG_DATA);
  readonly identityService = inject(IdentityService);
  dialog = inject(MatDialog);

  changePasswordForm = signal(new FormGroup({
    CurrentPassword: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.maxLength(60)],
    }),
    NewPassword: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required, Validators.minLength(8), Validators.maxLength(60)],
    }),
    RepeatNewPassword: new FormControl("",{
      nonNullable:true,
      validators:[Validators.required],
    }),
  },{validators: [compareTwoInputs("NewPassword","RepeatNewPassword"),]}));

  currentPassword = computed(() => this.changePasswordForm().controls["CurrentPassword"]);
  newPassword = computed(() => this.changePasswordForm().controls["NewPassword"]);
  repeatNewPassword = computed(() => this.changePasswordForm().controls["RepeatNewPassword"]);

  displayWaitSpinner = signal(false);

  submitForm(){
    if(this.changePasswordForm().valid){
      this.displayWaitSpinner.set(true);

      this.identityService.submitChangePassword(this.changePasswordForm().value).subscribe({
        next: res => {
          if(res.success){
            const resultInputData = new ResultDialogInputData();
            resultInputData.status = "success";
            resultInputData.title = "Success";
            resultInputData.description = ["Password changed successfully."];
            const resultDialogRef = this.dialog.open(Result,{data: resultInputData});
            resultDialogRef.afterClosed().subscribe(() => {
              this.displayWaitSpinner.set(false);
              this.changePasswordDialogRef.close();
            });
          }
        },
        error: err => {
          if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
              if(err.error.CurrentPassword || err.error.errors?.CurrentPassword){
                this.currentPassword().setErrors({sbmitError: err.error.CurrentPassword || err.error.errors?.CurrentPassword});
              }
              else if(err.error.NewPassword || err.error.errors?.NewPassword){
                this.newPassword().setErrors({sbmitError: err.error.NewPassword || err.error.errors?.NewPassword});
              }
              else if(err.error.RepeatNewPassword || err.error.errors?.RepeatNewPassword){
                this.repeatNewPassword().setErrors({sbmitError: err.error.RepeatNewPassword || err.error.errors?.RepeatNewPassword});
              }
              else if(err.error.ChangePassword){
                this.repeatNewPassword().setErrors({sbmitError: err.error.ChangePassword});
              }
              else if(err.error.errors){
                console.error("err.error?.errors: "+JSON.stringify(err.error.errors));
                throw(err);
              }
              else{
                console.error("err.error: "+JSON.stringify(err.error));
                throw(err);
              }
            }
        },
      });
    }
  }

}
