import { Component, inject, input, model, output, signal } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { SectionModel } from '../../../../models/section-model';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { EditHeader } from '../../../../dialogs/edit-header/edit-header';
import { EditParagraph } from '../../../../dialogs/edit-paragraph/edit-paragraph';
import { DocumentService } from '../../../../services/document-service';
import { EditCode } from '../../../../dialogs/edit-code/edit-code';
import { EditLink } from '../../../../dialogs/edit-link/edit-link';
import { EditFile } from '../../../../dialogs/edit-file/edit-file';
import { EditImageTitle } from '../../../../dialogs/edit-image-title/edit-image-title';
import { DocumentElementModel } from '../document-element/document-element';
import { EditElementFormModel, LibraryService } from '../../../../services/library-service';
import { Result } from '../../../../dialogs/result/result';

@Component({
  selector: 'app-edit-box',
  imports: [MatIcon,MatButton],
  templateUrl: './edit-box.html',
  styleUrl: './edit-box.css'
})
export class EditBox {
  elementModel = model.required<DocumentElementModel>();
  deleteElement = output<string>();
  //elementEdited = output<DocumentElementModel>();

  libraryService = inject(LibraryService);
  dialog = inject(MatDialog);

  editResponse = signal<{status:"success"|"fail", message:string}|null>(null);

  openDialog(){
    switch(this.elementModel().type){
      case "h1":
        this.openEditHeaderDialog();
        break;
      case "h2":
        this.openEditHeaderDialog();
        break;
      case "p":
        this.openEditParagraphDialog();
        break;
        case "code":
        this.openEditCodeDialog();
        break;
        case "file":
        this.openEditFileDialog();
        break;
        case "img":
        this.openEditImageDialog();
        break;
        case "link":
        this.openEditLinkDialog();
        break;
        /*case "tags":
        this.openEditTagsDialog();
        break;*/
    }
  }

  private openEditHeaderDialog(){
    const dialogRef = this.dialog.open(EditHeader, {
      data:{value:this.elementModel().value, enableEdit:true}
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.value = result;
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Value: result,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({value:result});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  private openEditParagraphDialog(){
    const dialogRef = this.dialog.open(EditParagraph, {
      data:{value:this.elementModel().value, enableEdit:true}
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.value = result;
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Value: result,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({value:result});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  private openEditCodeDialog(){
    const dialogRef = this.dialog.open(EditCode,{
      data:{value:this.elementModel().value, enableEdit:true}
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.value = result;
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Value: result,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({value:result});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  private openEditFileDialog(){
    const dialogRef = this.dialog.open(EditFile,{
      data:{value:this.elementModel().value, title:this.elementModel().title, enableEdit:true }
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.title = result;
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Title: result,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({title:result});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  private openEditImageDialog(){
    const dialogRef = this.dialog.open(EditImageTitle,{
      data:{value:this.elementModel().value,title:this.elementModel().title??'', enableEdit:true}
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.title = result;
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Title: result,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({title:result});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  private openEditLinkDialog(){
    const dialogRef = this.dialog.open(EditLink,{
      data:{value:this.elementModel().value, title:this.elementModel().title, enableEdit:true }
    });
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "Delete"){
          this.libraryService.requestDeleteElement(this.elementModel().guid).subscribe({
            next: res => {
              if(res && res.success){
                this.deleteElement.emit(this.elementModel().guid);
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when deleting this element!"});
              console.error(JSON.stringify(err));
            },
          });
        }
        else{
          this.elementModel.update(em=>{
            em.value = result.value; 
            em.title = result.title; 
            return em;
          });
          this.libraryService.editedElements().set(this.elementModel().guid, this.elementModel());

          /*let editElementFormModel: EditElementFormModel = {
            Guid: this.elementModel().guid,
            Title: result.title,
            Value: result.value,
          };
          this.libraryService.requestEditElement(editElementFormModel).subscribe({
            next: res => {
              if(res && res.success){
                this.editElement.emit({title:result.title, value: result.value});
                this.editResponse.set({status:"success", message:"Edited successfully."});
              }
            },
            error: err => {
              this.editResponse.set({status:"fail", message:"Something went wrong when editing this element!"});
              console.error(JSON.stringify(err));
            }
          });*/
        }
      }
    });
  }
  
}
