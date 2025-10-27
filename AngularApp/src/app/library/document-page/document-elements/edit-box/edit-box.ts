import { Component, inject, input } from '@angular/core';
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

@Component({
  selector: 'app-edit-box',
  imports: [MatIcon,MatButton],
  templateUrl: './edit-box.html',
  styleUrl: './edit-box.css'
})
export class EditBox {
  elementModel = input.required<DocumentElementModel>();

  documentService = inject(DocumentService);
  dialog = inject(MatDialog);

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
    const dialogRef = this.dialog.open(EditHeader,{data:{value:this.elementModel().value}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            value: result,
          };
          this.documentService.mockEditSection(sectionInfo);
          //this.documentService.editSectionValue(this.sectionModel().guid, result);
        }
      }
    });
  }
  private openEditParagraphDialog(){
    const dialogRef = this.dialog.open(EditParagraph,{data:{value:this.elementModel().value}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            value: result,
          };
          this.documentService.mockEditSection(sectionInfo);
          //this.documentService.editSectionValue(this.sectionModel().guid, result);
        }
      }
    });
  }
  private openEditCodeDialog(){
    const dialogRef = this.dialog.open(EditCode,{data:{value:this.elementModel().value}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            value: result,
          };
          this.documentService.mockEditSection(sectionInfo);
          //this.documentService.editSectionValue(this.sectionModel().guid, result);
        }
      }
    });
  }
  private openEditFileDialog(){
    const dialogRef = this.dialog.open(EditFile,{data:{value:this.elementModel().value, title:this.elementModel().title }});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            file: result.file, 
            title: result.title, 
          };
          this.documentService.mockEditSection(sectionInfo);
        }
      }
    });
  }
  private openEditImageDialog(){
    const dialogRef = this.dialog.open(EditImageTitle,{data:{value:this.elementModel().value,title:this.elementModel().title??''}});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            file: result.file, 
            title: result.title, 
          };
          this.documentService.mockEditSection(sectionInfo);
        }
      }
    });
  }
  private openEditLinkDialog(){
    const dialogRef = this.dialog.open(EditLink,{data:{value:this.elementModel().value, title:this.elementModel().title }});
    dialogRef.afterClosed().subscribe(result=>{
      if(result){
        if(result === "delete"){
          this.documentService.deleteSection(this.elementModel().guid);
        }
        else{
          let sectionInfo = {
            guid:this.elementModel().guid, 
            value: result.value, 
            title: result.title, 
          };
          this.documentService.mockEditSection(sectionInfo);
          //this.documentService.editSectionValueTitle(this.sectionModel().guid, result.value, result.title);
        }
      }
    });
  }
  
}
