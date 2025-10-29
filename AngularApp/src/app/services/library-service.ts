import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IdentityService, UserProfileModel } from './identity-service';
import { LibraryCardModel } from '../library/library-card/library-card';
import { ShelfCardModel } from '../library/shelf-card/shelf-card';
import { DocumentCardModel } from '../library/document-card/document-card';
import { DocumentPageModel } from '../library/document-page/document-page';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  private identityService = inject(IdentityService);
  currentLibraryModel = signal<LibraryCardModel|null>(null);
  currentShelfModel = signal<ShelfCardModel|null>(null);
  currentOwnerUserModel = signal<UserProfileModel|null>(null);

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
class NewDocumentFormModel{
  title?:string;
  description?:string|null;
  shelfGuids?:string[]|null;
  image?:File|null;
}