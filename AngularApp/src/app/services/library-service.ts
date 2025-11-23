import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IdentityService, UserProfileModel } from './identity-service';
import { LibraryCardModel } from '../library/library-card/library-card';
import { ShelfCardModel } from '../library/shelf-card/shelf-card';
import { DocumentCardModel } from '../library/document-card/document-card';
import { DocumentPageModel } from '../library/document-page/document-page';
import { DocumentElementModel } from '../library/document-page/document-elements/document-element/document-element';
import { Observable, of, tap, throwError } from 'rxjs';
import { ParentShelfModel } from '../library/new-document-form/new-document-form';
import { SingletonModes } from './singleton-modes';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  //private identityService = inject(IdentityService);
  //private singleton = inject(SingletonModes);

  libraryCard_Storage = signal<RuCache<LibraryCardModel>>(new RuCache<LibraryCardModel>());
  shelfCard_Storage = signal<RuCache<ShelfCardModel>>(new RuCache<ShelfCardModel>());
  documentCard_Storage = signal<RuCache<DocumentCardModel>>(new RuCache<DocumentCardModel>());
  documentPage_Storage = signal<RuCache<DocumentPageModel>>(new RuCache<DocumentPageModel>());
  owner_Storage = signal<RuCache<OwnerModel>>(new RuCache<OwnerModel>());


  requestLibraryList(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<LibraryCardModel[]>(
      "/api/Library/List", { params: quryParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add library card list to cache: "+JSON.stringify(res.map(lib=>lib.title)));
          this.libraryCard_Storage.update(ruCache=>{
            ruCache.add(...res);
            return ruCache;
          });
        }
      }),
    );
  }
  requestShelfList(libraryGuid: string){
    let quryParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<ShelfCardModel[]>(
      "/api/Library/ShelfList", { params: quryParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add shelf card list to cache: "+JSON.stringify(res.map(shelf=>shelf.title)));
          this.shelfCard_Storage.update(ruCache=>{
            ruCache.add(...res);
            return ruCache;
          });
        }
      }),
    );
  }
  requestUserShelfList(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<ParentShelfModel[]>(
      "/api/Library/UserShelfList", { params: quryParams}
    );
  }
  requestDocumentCardList(shelfGuid: string){
    let quryParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<DocumentCardModel[]>(
      "/api/Library/DocumentCardList", { params: quryParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add document card list to cache: "+JSON.stringify(res.map(doc=>doc.title)));
          this.documentCard_Storage.update(ruCache=>{
            ruCache.add(...res);
            return ruCache;
          });
        }
      }),
    );
  }

  requestTotalNumberOfDocuments(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<{totalNumberOfUserDocuments: number}>(
      "/api/Library/TotalNumberOfDocuments", { params: quryParams}
    );
  }
  requestTotalNumberOfShelves(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<{totalNumberOfUserShelves: number}>(
      "/api/Library/TotalNumberOfShelves", { params: quryParams}
    );
  }

  requestLibraryModel(libraryGuid:string){
    let cachedLibraryCardModel = this.libraryCard_Storage().getWithGuid(libraryGuid);
    if(cachedLibraryCardModel){
      //console.log("got one library card from cache: "+cachedLibraryCardModel.title);
      return of(cachedLibraryCardModel);
    }
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<LibraryCardModel>(
      "/api/Library/LibraryModel", {params: httpParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add one library card to cache: "+JSON.stringify(res.title));
          this.libraryCard_Storage.update(ruCache=>{
            ruCache.add(res);
            return ruCache;
          });
        }
      }),
    );
  }
  requestShelfModel(shelfGuid:string){
    let cachedShelfCardModel = this.shelfCard_Storage().getWithGuid(shelfGuid);
    if(cachedShelfCardModel){
      //console.log("got one shelf card from cache: "+cachedShelfCardModel.title);
      return of(cachedShelfCardModel);
    }
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<ShelfCardModel>(
      "/api/Library/ShelfModel", {params: httpParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add one shelf card to cache: "+JSON.stringify(res.title));
          this.shelfCard_Storage.update(ruCache=>{
            ruCache.add(res);
            return ruCache;
          });
        }
      }),
    );
  }
  requestDocumentCardModel(docGuid: string){
    let cachedDocumentCardModel = this.documentCard_Storage().getWithGuid(docGuid);
    if(cachedDocumentCardModel){
      //console.log("got one document card from cache: "+cachedDocumentCardModel.title);
      return of(cachedDocumentCardModel);
    }
    let httpParams = new HttpParams().set("documentGuid", docGuid);
    return this.httpClient.get<DocumentCardModel>(
      "/api/Library/DocumentCardModel", {params: httpParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add one document card to cache: "+JSON.stringify(res.title));
          this.documentCard_Storage.update(ruCache=>{
            ruCache.add(res);
            return ruCache;
          });
        }
      }),
    );
  }
  requestDocumentPageModel(documentGuid:string){
    let cachedDocumentPageModel = this.documentPage_Storage().getWithGuid(documentGuid);
    if(cachedDocumentPageModel){
      //console.log("got one document page from cache: "+cachedDocumentPageModel.title);
      return of(cachedDocumentPageModel);
    }
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.get<DocumentPageModel>(
      "/api/Library/DocumentPageModel", {params:httpParams}
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add one documennt page to cache: "+JSON.stringify(res.title));
          this.documentPage_Storage.update(ruCache=>{
            ruCache.add(res);
            return ruCache;
          });
        }
      }),
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
    if(newShelfFormModel.libraryGuids){
      newShelfFormModel.libraryGuids.forEach((value,index)=>{
        formData.append(`LibraryGuids[${index}]`, value);
      });
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
  requestDeleteLibrary(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteLibrary", {params: httpParams}
    );
  }
  requestDeleteShelf(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteShelf", {params: httpParams}
    );
  }

  submitEditedElements(editElementFormModelArray:EditElementFormModel[]){
    return this.httpClient.post<{success:boolean, elements:DocumentElementModel[]}>(
      "/api/Library/EditElements", editElementFormModelArray
    );
  }

  editDocumentIntroduction(documentGuid:string, title:string, description:string, image?:File){
    const formData = new FormData();
    formData.append("Guid",documentGuid);
    formData.append("Title",title);
    formData.append("Description", description);
    if(image){
      formData.append("Image", image);
    }

    return this.httpClient.post<{success:boolean, introduction:{title:string,description:string,hasImage:boolean,integrityVersion:number}}>(
      "/api/Library/EditDocumentIntroduction", formData
    );
  }
  editLibraryIntroduction(libraryGuid:string, title:string, description:string, image?:File){
    const formData = new FormData();
    formData.append("Guid",libraryGuid);
    formData.append("Title",title);
    formData.append("Description", description);
    if(image){
      formData.append("Image", image);
    }

    return this.httpClient.post<{success:boolean, introduction:{title:string,description:string,hasImage:boolean,integrityVersion:number}}>(
      "/api/Library/EditLibraryIntroduction", formData
    );
  }
  editShelfIntroduction(shelfGuid:string, title:string, description:string, image?:File){
    const formData = new FormData();
    formData.append("Guid",shelfGuid);
    formData.append("Title",title);
    formData.append("Description", description);
    if(image){
      formData.append("Image", image);
    }

    return this.httpClient.post<{success:boolean, introduction:{title:string,description:string,hasImage:boolean,integrityVersion:number}}>(
      "/api/Library/EditShelfIntroduction", formData
    );
  }
  
  deleteDocumentIntroductionImage(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteDocumentIntroductionImage", {params:httpParams}
    );
  }
  deleteLibraryIntroductionImage(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteLibraryIntroductionImage", {params:httpParams}
    );
  }
  deleteShelfIntroductionImage(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteShelfIntroductionImage", {params:httpParams}
    );
  }

  editDocumentParentShelves(documentGuid:string, shelfGuids:string[]){
    return this.httpClient.post<{success:boolean, parentShelves:ParentShelfModel[]}>(
      "/api/Library/EditDocumentParentShelves", 
      {documentGuid:documentGuid, shelfGuids:shelfGuids}
    );
  }
  editShelfParentLibraries(shelfGuid:string, libraryGuids:string[]){
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/EditShelfParentLibraries", 
      {shelfGuid:shelfGuid, libraryGuids:libraryGuids}
    );
  }

  requestOwnerModel(ownerGuid:string){
    let cachedOwnerModel = this.owner_Storage().getWithGuid(ownerGuid);
    if(cachedOwnerModel){
      //console.log("got one owner from cache: "+cachedOwnerModel.username);
      return of(cachedOwnerModel);
    }
    return this.httpClient.get<OwnerModel>(
      `/api/Library/GetOwnerModel?ownerGuid=${ownerGuid}`
    ).pipe(
      tap(res=>{
        if(res){
          //console.log("add one owner to cache: "+JSON.stringify(res.username));
          this.owner_Storage.update(ruCache=>{
            ruCache.add(res);
            return ruCache;
          });
        }
      }),
    );
  }

  getLibraryImageAddress(libraryModel:{guid?:string, integrityVersion?:number, hasImage?:boolean}|null):string|null{
    //console.log(JSON.stringify(libraryModel));
    if(libraryModel?.hasImage && libraryModel.guid){
      return `/api/Library/LibraryImage?libraryGuid=${libraryModel.guid}&v=${libraryModel.integrityVersion}`;
    }
    return null;
  }
  getShelfImageAddress(shelfModel:{guid?:string, integrityVersion?:number, hasImage?:boolean}|null):string|null{
    if(shelfModel?.hasImage && shelfModel.guid){
      return `/api/Library/ShelfImage?shelfGuid=${shelfModel.guid}&v=${shelfModel.integrityVersion}`;
    }
    return null;
  }
  getDocumentImageAddress(documentModel:{guid?:string, integrityVersion?:number, hasImage?:boolean}|null):string|null{
    if(documentModel?.hasImage && documentModel.guid){
      return `/api/Library/DocumentImage?documentGuid=${documentModel.guid}&v=${documentModel.integrityVersion}`;
    }
    return null;
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
  libraryGuids?:string[]; 
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
  Guid:string = null!;
  Value?:string;
  Title?:string;
  Order?:string;
  Delete?:boolean;
}

export class OwnerModel{
  guid:string = null!;
  username:string = null!;
  hasImage:boolean = false;
  integrityVersion:number = 0;
}

export class RuCache<T extends {guid:string}>{
  private capacity:number = 50;
  private cache:T[] = [];

  getWithGuid(guid:string):T|null{
    //console.log("RuCache.getWithGuid");
    let index = this.cache.findIndex(value=>value.guid === guid);
    if(index >= 0){
      let element = this.cache[index];
      this.cache.splice(index,1);
      this.cache.unshift(element);
      return element;
    }
    else{
      return null;
    }
  }

  add(...newValues:T[]){
    newValues.forEach(newValue=>{
      let index = this.cache.findIndex(value=>value.guid === newValue.guid);
      if(index >= 0){
        this.cache.splice(index,1);
      }
    });

    if(newValues.length > this.capacity){
      newValues = newValues.slice(-this.capacity);
    }

    if((this.cache.length + newValues.length) > this.capacity){
      let numberOfExceededElements = this.cache.length + newValues.length - this.capacity;
      let exceededElementsStartIndex = this.cache.length - numberOfExceededElements;
      if(exceededElementsStartIndex < 0){
        exceededElementsStartIndex = 0;
      }
      this.cache.splice(exceededElementsStartIndex);
    }

    //console.log("RuCache.add "+newValues.length+" values");
    
    this.cache.unshift(...newValues);
  }
}

