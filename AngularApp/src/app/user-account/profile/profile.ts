import { Component, computed, effect, ElementRef, inject, input, signal, viewChild } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditImage } from '../../dialogs/edit-image/edit-image';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { IdentityService, UserModel } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { SendLinkToEmail } from '../../dialogs/send-link-to-email/send-link-to-email';
import { MatCheckbox, MatCheckboxModule } from '@angular/material/checkbox';
import { ConfirmChange } from '../../dialogs/confirm-change/confirm-change';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatCardActions, MatIcon, MatButton, MatIconButton, MatTooltip, NgOptimizedImage,
    MatCheckboxModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
  userGuid = input<string|null>(null);

  singletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  identityService = inject(IdentityService);

  userModel = signal<UserModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  username = computed(()=>this.userModel()?.username);
  description = computed(()=>this.userModel()?.description);
  email = computed(()=>this.userModel()?.email);
  displayEmailPublicly = computed(()=>this.userModel()?.displayEmailPublicly);

  errorResponse = signal("");

  constructor(){
    if(!this.userGuid()){
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
      if(this.userGuid()){
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
    const dialogRef = this.dialog.open(EditImage,
      {data:{value: this.identityService.userModel()?.imageAddress}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          //this.userImgSrc.set("");
        }
        else{
          //this.userImgSrc.set(result.file.name);
        }
      }
    });
  }

  openEditUsernameDialog(){
    /*this.identityService.getCsrf().subscribe({
      next: () => console.log("Csrf received successfully."),
      error: err => {
        console.error("Couldn't get Csrf!");
        //throwError(()=>err);//doesn't pass error to the app-error-handler
        throw(err);
      },
    });*/
    
    const dialogRef = this.dialog.open(EditInput,
      {data:{label: 'Username', value: this.identityService.userModel()?.username}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitUserName(result).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserModel(this.userModel());
              newUserModel.username = result;
              this.identityService.updateUserModel(newUserModel);
            }
          },
          error: err => {
            if(err instanceof HttpErrorResponse && err.status == HttpStatusCode.BadRequest){
              if(err.error.Username || err.error.errors?.Username){
                this.errorResponse.set("*Error: "+err.error.errors?.Username);
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
    /*this.identityService.getCsrf().subscribe({
      next: () => console.log("Csrf received successfully."),
      error: err => {
        console.error("Couldn't get Csrf!");
        throw(err);
      },
    });*/

    const dialogRef = this.dialog.open(EditTextarea,
      {data:{label: 'About Me', value: this.identityService.userModel()?.description}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.identityService.submitDescription(result).subscribe({
          next: res=>{
            if(res.success){
              let newUserModel = new UserModel(this.userModel());
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
              let newUserModel = new UserModel(this.userModel());
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

}
