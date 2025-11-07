import { Injectable, signal } from '@angular/core';
import { DocumentPageModel } from './document-page';
import { EditElementFormModel } from '../../services/library-service';

@Injectable()
export class DocumentPageService {
  editMode = signal(false);
  toggleEditMode(){
    this.editMode.update(mode=>!mode);
  }

  documentPageModel = signal<DocumentPageModel|null>(null);
  unchangedDocumentPageModel = signal<DocumentPageModel|null>(null);
  
  editedElementFormModels = signal<Map<string, EditElementFormModel>>(new Map());
  getEditElementFormModelArray(){
    const editElementFormModelArray: EditElementFormModel[] = [];
    for(let v of this.editedElementFormModels().values()){
      editElementFormModelArray.push(v);
    }
    return editElementFormModelArray;
  }
}
