import { Component, inject, signal } from '@angular/core';
import { DocumentsList } from "../documents-list/documents-list";
import { MatCard, MatCardAvatar, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from "@angular/material/card";
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { SingletonModes } from '../../services/singleton-modes';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { EditInput } from '../../dialogs/edit-input/edit-input';
import { EditTextarea } from '../../dialogs/edit-textarea/edit-textarea';

@Component({
  selector: 'app-shelf-page',
  imports: [DocumentsList, MatCard, MatCardHeader, MatCardTitle, MatCardSubtitle, MatCardContent, MatIcon,
    MatCardAvatar, MatBadge, MatIconButton, MatTooltip, MatButton],
  templateUrl: './shelf-page.html',
  styleUrl: './shelf-page.css'
})
export class ShelfPage {
  shelfTitle = signal("shelf title");
  shelfDescription = signal(`Lorem ipsum dolor sit amet consectetur adipisicing elit. Iste laudantium quibusdam aspernatur labore,
    consectetur quidem eveniet quod et quae, veniam optio cupiditate harum necessitatibus asperiores?`);

  signletonModes = inject(SingletonModes);
  dialog = inject(MatDialog);

  openEditTitleDialog(){
    const dialogRef = this.dialog.open(EditInput,{data:{label: 'Edit Shelf Title', value: this.shelfTitle()}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.shelfTitle.set(result);
      }
    });
  }
  
  openEditDescriptionDialog(){
    const dialogRef = this.dialog.open(EditTextarea,{data:{label: 'Edit Shelf Description', value: this.shelfDescription()}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        this.shelfDescription.set(result);
      }
    });
  }
}
