import { Component, computed, effect, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditUserImage } from '../../dialogs/edit-user-image/edit-user-image';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { SendLinkToEmail } from '../../dialogs/send-link-to-email/send-link-to-email';
import { MatCheckbox, MatCheckboxModule } from '@angular/material/checkbox';
import { ConfirmChange } from '../../dialogs/confirm-change/confirm-change';
import { ChangePassword } from '../../dialogs/change-password/change-password';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-user-account-manager',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatCardActions, MatIcon, MatTooltip, NgOptimizedImage,
    MatCheckboxModule, MatButtonModule],
  templateUrl: './user-account-manager.html',
  styleUrl: './user-account-manager.css'
})
export class UserAccountManager {
  singletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  identityService = inject(IdentityService);
  activatedRoute = inject(ActivatedRoute);

  userImgSrc = computed(()=>this.singletonModes.getUserImageAddress(this.identityService.userModel()));
  username = computed(()=>this.identityService.userModel()?.username);
  description = computed(()=>this.identityService.userModel()?.description);
  email = computed(()=>this.identityService.userModel()?.email);
  displayEmailPublicly = computed(()=>this.identityService.userModel()?.displayEmailPublicly);

  errorResponse = signal("");

  constructor(){

    effect(() => {
      this.identityService.getCsrf().subscribe({
        next: () => {
          console.log("Csrf received successfully.");
        },
        error: err => {
          console.error("Couldn't get Csrf!");
          throw(err);
        },
      });
    });
  }

  openEditImageDialog(){
    this.dialog.open(EditUserImage,
    {data:{currentImgSrc: this.userImgSrc()}});
  }

  openEditUsernameDialog(){
    const dialogRef = this.dialog.open(EditInput,
      {data:{label: 'Username', value: this.identityService.userModel()?.username}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitUserName(result).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserProfileModel(this.identityService.userModel());
              newUserModel.username = result;
              this.identityService.updateUserModel(newUserModel);
            }
          },
          error: err => {
            if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
              if(err.error.Username || err.error.errors?.Username){
                this.errorResponse.set("*Error: "+ (err.error.Username || err.error.errors?.Username));
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
          }
        });
      }
    });
  }
  
  openEditEmailDialog(){
    this.dialog.open(SendLinkToEmail, {data:{purpose: 'changeEmail'}});
  }
  
  openEditDescriptionDialog(){
    const dialogRef = this.dialog.open(EditTextarea,
      {data:{label: 'About Me', value: this.identityService.userModel()?.description}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitDescription(result).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserProfileModel(this.identityService.userModel());
              newUserModel.description = result;
              this.identityService.updateUserModel(newUserModel);
            }
          },
          error: err => {
            if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
              if(err.error.Description || err.error.errors?.Description){
                this.errorResponse.set("*Error: "+err.error.errors?.Description);
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
          }
        });
      }
    });
  }

  editDisplayEmailPublicly(){
    let displayPubliclyEditedTo = !this.displayEmailPublicly();
    let changeMessage = displayPubliclyEditedTo ? "Display your email publicly" : "NOT display your email publicly";
    const dialogRef = this.dialog.open(ConfirmChange,
      {data:{change: changeMessage}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitDisplayEmailPublicly(displayPubliclyEditedTo).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserProfileModel(this.identityService.userModel());
              newUserModel.displayEmailPublicly = displayPubliclyEditedTo;
              this.identityService.updateUserModel(newUserModel);
            }
          },
          error: err => {
            if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
              if(err.error.DisplayEmailPublicly || err.error.errors?.DisplayEmailPublicly){
                this.errorResponse.set("*Error: "+err.error.errors?.DisplayEmailPublicly);
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
          }
        });
      }
    });
  }

  openChangePasswordDialog(){
    this.dialog.open(ChangePassword);
  }

}
