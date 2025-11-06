import { Injectable, signal } from '@angular/core';
import { DocumentPageModel } from './document-page';
import { EditElementFormModel } from '../../services/library-service';

@Injectable()
export class DocumentPageService {
  documentPageModel = signal<DocumentPageModel|null>(null);
  
  editedElementFormModels = signal<Map<string, EditElementFormModel>>(new Map());
  
  getEditElementFormModelArray(){
    const editElementFormModelArray: EditElementFormModel[] = [];
    for(let v of this.editedElementFormModels().values()){
      editElementFormModelArray.push(v);
    }
    return editElementFormModelArray;
  }
}
