import { Component, inject, input } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { EditHeader } from '../../../../dialogs/edit-header/edit-header';
import { EditParagraph } from '../../../../dialogs/edit-paragraph/edit-paragraph';
import { EditCode } from '../../../../dialogs/edit-code/edit-code';
import { EditLink } from '../../../../dialogs/edit-link/edit-link';
import { EditFile } from '../../../../dialogs/edit-file/edit-file';
import { EditImageTitle } from '../../../../dialogs/edit-image-title/edit-image-title';
import { DocumentElementModel } from '../document-element/document-element';
import { EditElementFormModel, LibraryService } from '../../../../services/library-service';
import { DocumentPageService } from '../../document-page-service';

@Component({
  selector: 'app-edit-box',
  imports: [MatIcon,MatButton],
  templateUrl: './edit-box.html',
  styleUrl: './edit-box.css'
})
export class EditBox {
  elementModel = input.required<DocumentElementModel>();

  libraryService = inject(LibraryService);
  dialog = inject(MatDialog);
  documentPageService = inject(DocumentPageService);

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
          this.deleteElement();
        }
        else{
          this.editValueTitle(result);
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
          this.deleteElement();
        }
        else{
          this.editValueTitle(result);
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
          this.deleteElement();
        }
        else{
          this.editValueTitle(result);
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
          this.deleteElement();
        }
        else{
          this.editValueTitle(undefined,result);
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
          this.deleteElement();
        }
        else{
          this.editValueTitle(undefined, result);
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
          this.deleteElement();
        }
        else{
          this.editValueTitle(result.value,result.title);
        }
      }
    });
  }

  deleteElement(){
    const editElementFormModel: EditElementFormModel = {
      Guid: this.elementModel().guid,
      Delete: true,
    }
    this.documentPageService.editedElementFormModels().set(editElementFormModel.Guid!, editElementFormModel);
    
    const elementOrder = this.elementModel().order;
    this.deleteElementFromDocumentPage();
    
    if(this.documentPageService.documentPageModel()){
      let biggerOrderElements = this.documentPageService.documentPageModel()!.elements.filter(el=>
        el.order > elementOrder
      );
      this.decreaseOrderOfElements(biggerOrderElements);
    }
  }
  deleteElementFromDocumentPage(){
    if(this.documentPageService.documentPageModel()){
      let elementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(el=>
        el.guid === this.elementModel().guid
      );
      if(elementIndex >= 0){
        this.documentPageService.documentPageModel.update(dpm=>{
          dpm!.elements.splice(elementIndex,1);
          return dpm;
        });
      }
    }
  }
  decreaseOrderOfElements(elements: DocumentElementModel[]){
    /*elements.forEach(element=>{
      element.order -= 1;
      //save to edited elements fom models
      this.documentPageService.editedElementFormModels().set(element.guid, {
        Guid: element.guid,
        Order: element.order.toString(),
        Title: element.title,
        Value: element.value,
      });

    });*/
    //substitute the edited element with it's older version 
    this.documentPageService.documentPageModel.update(dpm=>{
      elements.forEach(element=>{
        element.order -= 1;
        let elementIndex = dpm!.elements.findIndex(el=>{
          el.guid === element.guid
        });
        dpm!.elements.splice(elementIndex,1,element);
      });
      return dpm;
    });
  }

  editValueTitle(value?:string, title?:string){
    const editElementFormModel: EditElementFormModel = {
      Guid: this.elementModel().guid,
      Order: this.elementModel().order.toString(),
      Title: title,
      Value: value,
    }
    const editedElement = this.elementModel();
    
    if(value){
      editedElement.value = value;
    }
    if(title){
      editedElement.title = title;
    }

    this.documentPageService.editedElementFormModels().set(editElementFormModel.Guid!, editElementFormModel);
    this.editElementOnDocumentPage(editedElement);
  }
  editElementOnDocumentPage(editedElement:DocumentElementModel){
    if(this.documentPageService.documentPageModel()){
      let elementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(el=>el.guid === editedElement.guid);
      if(elementIndex >= 0){
        this.documentPageService.documentPageModel.update(dpm=>{
          dpm!.elements.splice(elementIndex,1,editedElement);
          return dpm;
        });
      }
    }
  }
  
  increaseOrderSubstitute(){
    if(this.documentPageService.documentPageModel()){
      let totalElementNumber = this.documentPageService.documentPageModel()!.elements.length;
      let editedElementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(el=>el.guid === this.elementModel().guid);
      if(editedElementIndex >= 0 && editedElementIndex < (totalElementNumber - 1)){
        let editedElement = this.documentPageService.documentPageModel()!.elements[editedElementIndex];
        let substitutedElementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(
          el=>el.order === (editedElement.order + 1)
        );
        editedElement.order++;
        if(substitutedElementIndex >= 0){
          let substitutedElement = this.documentPageService.documentPageModel()!.elements[substitutedElementIndex];
          substitutedElement.order--;

          //save to edited elements fom models
          this.documentPageService.editedElementFormModels().set(editedElement.guid, {
            Guid: editedElement.guid,
            Order: editedElement.order.toString(),
            Title: editedElement.title,
            Value: editedElement.value,
          });
          this.documentPageService.editedElementFormModels().set(substitutedElement.guid, {
            Guid: substitutedElement.guid,
            Order: substitutedElement.order.toString(),
            Title: substitutedElement.title,
            Value: substitutedElement.value,
          });
          
          //substitute the edited element with it's substituted element 
          this.documentPageService.documentPageModel.update(dpm=>{
            dpm!.elements.splice(editedElementIndex,1,substitutedElement);
            dpm!.elements.splice(substitutedElementIndex,1,editedElement);
            return dpm;
          });
        }
      }
    }
  }
  decreaseOrderSubstitute(){
    if(this.documentPageService.documentPageModel()){
      //let totalElementNumber = this.documentPageService.documentPageModel()!.elements.length;
      let editedElementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(el=>el.guid === this.elementModel().guid);
      if(editedElementIndex >= 1){
        let editedElement = this.documentPageService.documentPageModel()!.elements[editedElementIndex];
        let substitutedElementIndex = this.documentPageService.documentPageModel()!.elements.findIndex(
          el=>el.order === (editedElement.order - 1)
        );
        editedElement.order--;
        if(substitutedElementIndex >= 0){
          let substitutedElement = this.documentPageService.documentPageModel()!.elements[substitutedElementIndex];
          substitutedElement.order++;

          //save to edited elements fom models
          this.documentPageService.editedElementFormModels().set(editedElement.guid, {
            Guid: editedElement.guid,
            Order: editedElement.order.toString(),
            Title: editedElement.title,
            Value: editedElement.value,
          });
          this.documentPageService.editedElementFormModels().set(substitutedElement.guid, {
            Guid: substitutedElement.guid,
            Order: substitutedElement.order.toString(),
            Title: substitutedElement.title,
            Value: substitutedElement.value,
          });
          
          //substitute the edited element with it's substituted element 
          this.documentPageService.documentPageModel.update(dpm=>{
            dpm!.elements.splice(editedElementIndex,1,substitutedElement);
            dpm!.elements.splice(substitutedElementIndex,1,editedElement);
            return dpm;
          });
        }
      }
    }
  }
  
}
