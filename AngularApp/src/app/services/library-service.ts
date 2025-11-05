import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IdentityService, UserProfileModel } from './identity-service';
import { LibraryCardModel } from '../library/library-card/library-card';
import { ShelfCardModel } from '../library/shelf-card/shelf-card';
import { DocumentCardModel } from '../library/document-card/document-card';
import { DocumentPageModel } from '../library/document-page/document-page';
import { DocumentElementModel } from '../library/document-page/document-elements/document-element/document-element';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  private identityService = inject(IdentityService);

  currentLibraryModel = signal<LibraryCardModel|null>(null);
  currentShelfModel = signal<ShelfCardModel|null>(null);
  currentOwnerUserModel = signal<UserProfileModel|null>(null);

  editedElements = signal<Map<string, DocumentElementModel>>(new Map());


  requestLibraryList(userGuid: string|null = null){
    let quryParams:HttpParams;
    if(userGuid){
      quryParams = new HttpParams().set("userGuid", userGuid);
    }
    else if(this.identityService.isAuthenticated()){
      quryParams = new HttpParams().set("userGuid", this.identityService.userModel()!.guid!);
    }
    else{
      return null;
    }
    return this.httpClient.get<LibraryCardModel[]>("/api/Library/List", { params: quryParams});
  }
  requestShelfList(libraryGuid: string){
    let quryParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<ShelfCardModel[]>("/api/Library/ShelfList", { params: quryParams});
  }
  requestDocumentCardList(shelfGuid: string){
    let quryParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<DocumentCardModel[]>("/api/Library/DocumentCardList", { params: quryParams});
  }

  /*requestTotalNumberOfShelves(userGuid: string|null = null){
    let quryParams:HttpParams;
    if(userGuid){
      quryParams = new HttpParams().set("userGuid", userGuid);
    }
    else if(this.identityService.isAuthenticated()){
      quryParams = new HttpParams().set("userGuid", this.identityService.userModel()!.guid!);
    }
    else{
      return null;
    }
    return this.httpClient.get<{totalNumberOfUserShelves: number}>(
      "/api/Library/TotalNumberOfShelves", { params: quryParams}
    );
  }*/
  requestTotalNumberOfDocuments(userGuid: string|null = null){
    let quryParams:HttpParams;
    if(userGuid){
      quryParams = new HttpParams().set("userGuid", userGuid);
    }
    else if(this.identityService.isAuthenticated()){
      quryParams = new HttpParams().set("userGuid", this.identityService.userModel()!.guid!);
    }
    else{
      return null;
    }
    return this.httpClient.get<{totalNumberOfUserDocuments: number}>(
      "/api/Library/TotalNumberOfDocuments", { params: quryParams}
    );
  }

  requestLibraryModel(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<LibraryCardModel>(
      "/api/Library/LibraryModel", {params: httpParams}
    );
  }
  requestShelfModel(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<ShelfCardModel>(
      "/api/Library/ShelfModel", {params: httpParams}
    );
  }
  requestDocumentCardModel(docGuid: string){
    let httpParams = new HttpParams().set("documentGuid", docGuid);
    return this.httpClient.get<DocumentCardModel>(
      "/api/Library/DocumentCardModel", {params: httpParams}
    );
  }
  requestDocumentPageModel(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.get<DocumentPageModel>(
      "/api/Library/DocumentPageModel", {params:httpParams}
    );
  }

  createNewLibrary(newLibraryFormModel: NewLibraryFormModel){
    const formData = new FormData();
    if(newLibraryFormModel.title){
      formData.append("Title", newLibraryFormModel.title);
    }
    if(newLibraryFormModel.image){
      formData.append('Image', newLibraryFormModel.image);
    }
    if(newLibraryFormModel.description){
      formData.append("Description", newLibraryFormModel.description);
    }
    return this.httpClient.post<{success:boolean, libraryGuid: string}>(
      "/api/Library/CreateNewLibrary", formData
    );
  }
  createNewShelf(newShelfFormModel: NewShelfFormModel){
    const formData = new FormData();
    if(newShelfFormModel.title){
      formData.append("Title", newShelfFormModel.title);
    }
    if(newShelfFormModel.libraryGuid){
      formData.append("LibraryGuid", newShelfFormModel.libraryGuid);
    }
    if(newShelfFormModel.image){
      formData.append('Image', newShelfFormModel.image);
    }
    if(newShelfFormModel.description){
      formData.append("Description", newShelfFormModel.description);
    }
    return this.httpClient.post<{success:boolean, shelfGuid: string}>(
      "/api/Library/CreateNewShelf", formData
    );
  }
  createNewDocument(newDocumentFormModel: NewDocumentFormModel){
    const formData = new FormData();
    if(newDocumentFormModel.title){
      formData.append("Title", newDocumentFormModel.title);
    }
    if(newDocumentFormModel.description){
      formData.append("Description", newDocumentFormModel.description);
    }
    if(newDocumentFormModel.shelfGuids){
      newDocumentFormModel.shelfGuids.forEach((value,index)=>{
        formData.append(`ShelfGuids[${index}]`, value);
      });
    }
    if(newDocumentFormModel.image){
      formData.append('Image', newDocumentFormModel.image);
    }
    return this.httpClient.post<{success:boolean, documentGuid: string}>(
      "/api/Library/CreateNewDocument", formData
    );
  }
  createNewElement(newElementFormModel: NewElementFormModel){
    const formData = new FormData();
    if(newElementFormModel.DocumentGuid){
      formData.append("DocumentGuid", newElementFormModel.DocumentGuid);
    }
    if(newElementFormModel.Order){
      formData.append("Order", newElementFormModel.Order);
    }
    if(newElementFormModel.Title){
      formData.append("Title", newElementFormModel.Title);
    }
    if(newElementFormModel.Type){
      formData.append("Type", newElementFormModel.Type);
    }
    if(newElementFormModel.Value){
      formData.append("Value", newElementFormModel.Value);
    }
    if(newElementFormModel.File){
      formData.append("File", newElementFormModel.File);
    }
    return this.httpClient.post<DocumentElementModel>(
      "/api/Library/CreateNewElement", formData
    );
  }

  requestDeleteDocument(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteDocument", {params: httpParams}
    );
  }
  requestDeleteElement(elementGuid:string){
    let httpParams = new HttpParams().set("elementGuid", elementGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteElement", {params: httpParams}
    );
  }

  requestEditElement(editElementFormModel: EditElementFormModel){
    const formData = new FormData();
    if(editElementFormModel.Guid){
      formData.append("Guid", editElementFormModel.Guid);
    }
    else{
      return throwError(()=>new Error("Element Guid is needed to request for editing the element!"));
    }
    if(editElementFormModel.Order){
      formData.append("Order", editElementFormModel.Order);
    }
    if(editElementFormModel.Title){
      formData.append("Title", editElementFormModel.Title);
    }
    if(editElementFormModel.Value){
      formData.append("Value", editElementFormModel.Value);
    }

    return this.httpClient.post<{success:boolean, element?:DocumentElementModel}>(
      "/api/Library/EditElement", formData
    );
  }

  /*updateEditedElement(editedElement: DocumentElementModel){
    this.editedElements().set(editedElement.guid, editedElement);
  }*/

}

class NewLibraryFormModel{
  title?:string;
  description?:string|null; 
  image?:File|null
}
class NewShelfFormModel{
  title?:string; 
  description?:string|null; 
  libraryGuid?:string; 
  image?:File|null
}
export class NewDocumentFormModel{
  title?:string;
  description?:string|null;
  shelfGuids?:string[]|null;
  image?:File|null;
}
export class NewElementFormModel{
  Type?:string;
  Value?:string;
  Title?:string;
  Order?:string;
  DocumentGuid?:string;
  File?:File;
}
export class EditElementFormModel{
  Guid?:string;
  Value?:string;
  Title?:string;
  Order?:string;
}