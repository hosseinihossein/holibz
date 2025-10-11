import { Component, computed, inject, input, signal } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditImage } from '../../dialogs/edit-image/edit-image';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';
import { IdentityService } from '../../services/identity-service';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatCardActions, MatIcon, MatButton, MatIconButton, MatTooltip, NgOptimizedImage],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
  userGuid = input<string|null>(null);

  singletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);
  identityService = inject(IdentityService);

  userImgSrc = computed(()=>
    this.userGuid() ? 
    this.identityService.getUserImageAddress(this.userGuid()!) :
    this.identityService.userModel()?.imageAddress
  );
  username = computed(()=>
    this.userGuid() ? 
    this.identityService.getUserName(this.userGuid()!) :
    this.identityService.userModel()?.username
  );
  email = computed(()=>
    this.userGuid() ?
    this.identityService.getUserEmail(this.userGuid()!) : 
    this.identityService.userModel()?.email
  );
  description = computed(()=>
    this.userGuid() ? 
    this.identityService.getUserImageAddress(this.userGuid()!) :
    this.identityService.userModel()?.description
  );

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
    const dialogRef = this.dialog.open(EditInput,
      {data:{label: 'Edit Username', value: this.identityService.userModel()?.username}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.username.set(result);
      }
    });
  }
  
  openEditEmailDialog(){
    const dialogRef = this.dialog.open(EditInput,
      {data:{label: 'Edit Email', value: this.identityService.userModel()?.email}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.email.set(result);
      }
    });
  }
  
  openEditDescriptionDialog(){
    const dialogRef = this.dialog.open(EditTextarea,
      {data:{label: 'Edit User Description', value: this.identityService.userModel()?.description}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        //this.description.set(result);
      }
    });
  }

}
