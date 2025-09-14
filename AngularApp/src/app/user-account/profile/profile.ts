import { Component, inject, signal } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditImage } from '../../dialogs/edit-image/edit-image';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';

@Component({
  selector: 'app-profile',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent,
    MatCardActions, MatIcon, MatButton, MatIconButton, MatTooltip],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
  userImgSrc = signal("defaultProfile.jpg");
  username = signal("username");
  email = signal("useremail@example.com");
  description = signal(`Lorem ipsum dolor sit amet consectetur adipisicing elit. Iste laudantium quibusdam aspernatur labore,
    consectetur quidem eveniet quod et quae, veniam optio cupiditate harum necessitatibus asperiores?`);

  singletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);

  openEditImageDialog(){
    const dialogRef = this.dialog.open(EditImage,{data:{value: "defaultProfile.jpg"}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.userImgSrc.set("defaultProfile.jpg");
        }
        else{
          this.userImgSrc.set(result.file.name);
        }
      }
    });
  }

  openEditUsernameDialog(){
    const dialogRef = this.dialog.open(EditInput,{data:{label: 'Edit Username', value: this.username()}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.username.set(result);
      }
    });
  }
  
  openEditEmailDialog(){
    const dialogRef = this.dialog.open(EditInput,{data:{label: 'Edit Email', value: this.email()}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.email.set(result);
      }
    });
  }
  
  openEditDescriptionDialog(){
    const dialogRef = this.dialog.open(EditTextarea,{data:{label: 'Edit User Description', value: this.description()}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.description.set(result);
      }
    });
  }

}
