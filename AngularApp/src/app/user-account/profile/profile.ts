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
import { Result, ResultDialogInputData } from '../../dialogs/result/result';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatCardActions, MatIcon, MatTooltip, NgOptimizedImage,
    MatCheckboxModule, MatButtonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
  userGuid = signal<string|null>(null);
  isMyProfile = signal(false);

  singletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  identityService = inject(IdentityService);
  activatedRoute = inject(ActivatedRoute);

  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  username = computed(()=>this.userModel()?.username);
  description = computed(()=>this.userModel()?.description);
  email = computed(()=>this.userModel()?.email);
  displayEmailPublicly = computed(()=>this.userModel()?.displayEmailPublicly);

  errorResponse = signal("");

  constructor(){
    let userGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("userGuid");
    if(userGuidRouteParam){
      this.userGuid.set(userGuidRouteParam);
    }

    if(!this.userGuid() || this.userGuid() === this.identityService.userModel()?.guid){
      this.isMyProfile.set(true);
    }

    if(this.isMyProfile()){
      this.identityService.getCsrf().subscribe({
        next: () => {
          console.log("Csrf received successfully.");
        },
        error: err => {
          console.error("Couldn't get Csrf!");
          //throwError(()=>err);//doesn't pass error to the app-error-handler
          throw(err);
        },
      });
    }
    
    effect(()=>{
      if(!this.isMyProfile()){
        this.identityService.requestUserModel(this.userGuid()!).subscribe({
          next: res=>this.userModel.set(res),
        });
      }
      else{
        this.userModel.set(this.identityService.userModel());
      }
    });
  }

  openEditImageDialog(){
    this.dialog.open(EditUserImage,
    {data:{currentImgSrc: this.identityService.userModel()?.imageAddress}});
  }

  openEditUsernameDialog(){
    const dialogRef = this.dialog.open(EditInput,
      {data:{label: 'Username', value: this.identityService.userModel()?.username}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitUserName(result).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserProfileModel(this.userModel());
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
              let newUserModel = new UserProfileModel(this.userModel());
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
              let newUserModel = new UserProfileModel(this.userModel());
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
