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
import { UserProfileInfo } from '../user-account/profile/profile';
import { GenericListFilter } from '../library/generic-list/generic-list';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  singleton = inject(SingletonModes);

  //libraryCard_Storage = signal<RuCache<LibraryCardModel>>(new RuCache<LibraryCardModel>());
  //shelfCard_Storage = signal<RuCache<ShelfCardModel>>(new RuCache<ShelfCardModel>());
  //documentCard_Storage = signal<RuCache<DocumentCardModel>>(new RuCache<DocumentCardModel>());
  //documentPage_Storage = signal<RuCache<DocumentPageModel>>(new RuCache<DocumentPageModel>());
  //owner_Storage = signal<RuCache<OwnerModel>>(new RuCache<OwnerModel>());


  requestLibraryList(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<LibraryCardModel[]>(
      "/api/Library/List", { params: quryParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          //console.log("add library card list to cache: "+JSON.stringify(res.map(lib=>lib.title)));
          this.libraryCard_Storage().add(...res);
        }
      }),
    )*/;
  }
  requestLibraryBriefList(ownerGuid: string){
    let quryParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<{guid:string,title:string}[]>(
      "/api/Library/BriefList", { params: quryParams}
    );
  }
  requestUserLibrariesGuids(ownerGuid: string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserLibrariesGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserLibraries(ownerGuid: string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserLibraries", {params:httpParams}
    );
  }

  requestShelfList(libraryGuid: string){
    let quryParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<ShelfCardModel[]>(
      "/api/Library/ShelfList", { params: quryParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          this.shelfCard_Storage().add(...res);
        }
      }),
    )*/;
  }
  requestLibraryShelvesGuids(libraryGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("libraryGuid",libraryGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetLibraryShelvesGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfLibraryShelves(libraryGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("libraryGuid",libraryGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfLibraryShelves", {params:httpParams}
    );
  }
  requestUserShelvesGuids(ownerGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserShelvesGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserShelves(ownerGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserShelves", {params:httpParams}
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
    )/*.pipe(
      tap(res=>{
        if(res){
          this.documentCard_Storage().add(...res);
        }
      }),
    )*/;
  }
  requestShelfDocumentsGuids(shelfGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("shelfGuid",shelfGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetShelfDocumentsGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfShelfDocuments(shelfGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("shelfGuid",shelfGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfShelfDocuments", {params:httpParams}
    );
  }
  requestUserDocumentsGuids(ownerGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserDocumentsGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserDocuments(ownerGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("ownerGuid",ownerGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserDocuments", {params:httpParams}
    );
  }

  requestLibraryModel(libraryGuid:string){
    /*let cachedLibraryCardModel = this.libraryCard_Storage().getWithGuid(libraryGuid);
    if(cachedLibraryCardModel){
      return of(cachedLibraryCardModel);
    }*/
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<LibraryCardModel>(
      "/api/Library/LibraryModel", {params: httpParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          this.libraryCard_Storage().add(res);
        }
      }),
    )*/;
  }
  requestShelfModel(shelfGuid:string){
    /*let cacheModel = this.shelfCard_Storage().getWithGuid(shelfGuid);
    if(cacheModel){
      return of(cacheModel);
    }*/
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<ShelfCardModel>(
      "/api/Library/ShelfModel", {params: httpParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          this.shelfCard_Storage().add(res);
        }
      }),
    )*/;
  }
  requestDocumentCardModel(docGuid: string){
    /*let cachedDocumentCardModel = this.documentCard_Storage().getWithGuid(docGuid);
    if(cachedDocumentCardModel){
      return of(cachedDocumentCardModel);
    }*/
    let httpParams = new HttpParams().set("documentGuid", docGuid);
    return this.httpClient.get<DocumentCardModel>(
      "/api/Library/DocumentCardModel", {params: httpParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          this.documentCard_Storage().add(res);
        }
      }),
    )*/;
  }
  requestDocumentPageModel(documentGuid:string){
    /*let cachedDocumentPageModel = this.documentPage_Storage().getWithGuid(documentGuid);
    if(cachedDocumentPageModel){
      return of(cachedDocumentPageModel);
    }*/
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.get<DocumentPageModel>(
      "/api/Library/DocumentPageModel", {params:httpParams}
    )/*.pipe(
      tap(res=>{
        if(res){
          this.documentPage_Storage().add(res);
          this.documentCard_Storage().add(new DocumentCardModel({
            description: res.description,
            guid: res.guid,
            hasImage: res.hasImage,
            headers: res.elements.filter(el=>el.type == "h1" || el.type == "h2").map(el=>el.value),
            integrityVersion: res.integrityVersion,
            ownerGuid: res.owner.userGuid,
            title: res.title,
            versionName: res.version,
            createdAt: res.createdAt,
          }));
          
          for(let parentShelfGuid of res.shelves.map(shelf=>shelf.guid)){ 
            let parentShelf = this.shelfCard_Storage().getWithGuid(parentShelfGuid);
            if(parentShelf && !parentShelf.documentsGuids.includes(res.guid)){
              parentShelf!.documentsGuids.push(res.guid);
              parentShelf!.totalNumberOfShelfDocuments += 1;
            }
          }
        }
      }),
    )*/;
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
    )/*.pipe(
      tap(res=>{
        if(res && res.success){
          this.documentPage_Storage().delete(documentGuid);
          this.documentCard_Storage().delete(documentGuid);
          this.shelfCard_Storage().getArrayReference().forEach((shelfCardModel)=>{
            let index = shelfCardModel.documentsGuids.indexOf(documentGuid);
            if(index >= 0){
              shelfCardModel.documentsGuids.splice(index,1);
              shelfCardModel.totalNumberOfShelfDocuments -= 1;
            }
          });
        }
      }),
    )*/;
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
    )/*.pipe(
      tap(res=>{
        if(res && res.success){
          this.libraryCard_Storage().delete(libraryGuid);
        }
      }),
    )*/;
  }
  requestDeleteShelf(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.delete<{success:boolean}>(
      "/api/Library/DeleteShelf", {params: httpParams}
    )/*.pipe(
      tap(res=>{
        if(res && res.success){
          this.shelfCard_Storage().delete(shelfGuid);
        }
      }),
    )*/;
  }

  submitEditedElements(parentDocumentGuid:string, editElementFormModelArray:EditElementFormModel[]){
    let httpParams = new HttpParams().set("parentDocumentGuid",parentDocumentGuid);
    return this.httpClient.post<{success:boolean, elements:DocumentElementModel[]}>(
      "/api/Library/EditElements", editElementFormModelArray, {params:httpParams}
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
  removeDoumentFromParentShelf(documentGuid:string,shelfGuid:string){
    let httpParams = new HttpParams().set("documentGuid",documentGuid).set("shelfGuid",shelfGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/RemoveDocumentFromParentShelf", null, {params:httpParams}
    )/*.pipe(
      tap(res=>{
        if(res && res.success){
          //shelfCardModel
          const shelfCardModel = this.shelfCard_Storage().getWithGuid(shelfGuid);
          if(shelfCardModel){
            let index = shelfCardModel.documentsGuids.indexOf(documentGuid);
            shelfCardModel.documentsGuids.splice(index,1);
          }
          //documentPageModel
          const documentPageModel = this.documentPage_Storage().getWithGuid(documentGuid);
          if(documentPageModel){
            let index = documentPageModel.shelves.findIndex(shelf=>shelf.guid === shelfGuid)
            documentPageModel.shelves.splice(index,1);
          }
        }
      }),
    )*/;
  }
  editShelfParentLibraries(shelfGuid:string, libraryGuids:string[]){
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/EditShelfParentLibraries", 
      {shelfGuid:shelfGuid, libraryGuids:libraryGuids}
    );
  }

  //transfer it to the IdentityService
  requestOwnerModel(ownerGuid:string){
    /*let cachedOwnerModel = this.owner_Storage().getWithGuid(ownerGuid);
    if(cachedOwnerModel){
      return of(cachedOwnerModel);
    }*/
    return this.httpClient.get<OwnerModel>(
      `/api/Library/GetOwnerModel?ownerGuid=${ownerGuid}`
    )/*.pipe(
      tap(res=>{
        if(res){
          this.owner_Storage().add(res);
        }
      }),
    )*/;
  }
  requestUserProfileInfo(userGuid:string){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    return this.httpClient.get<UserProfileInfo>(
      "/api/Library/GetUserProfileInfo",{params:httpParams}
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

  //******************************* followship **********************************

  requestToFollow(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/Follow", null, {params:httpParams}
    );
  }
  requestToUnFollow(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/UnFollow", null, {params:httpParams}
    );
  }

  requestFollowers(ownerGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    httpParams = httpParams.set("bunchIndex", bunchIndex);
    if(filter && filter.trim().length >= 3){
      httpParams = httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Library/GetFollowers", {params:httpParams}
    );
  }
  requestFollowings(ownerGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    httpParams = httpParams.set("bunchIndex", bunchIndex);
    if(filter && filter.trim().length >= 3){
      httpParams = httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Library/GetFollowings", {params:httpParams}
    );
  }

  requestUsersInFavorOfLibrary(libraryGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    httpParams = httpParams.set("bunchIndex", bunchIndex);
    if(filter && filter.trim().length >= 3){
      httpParams = httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Library/GetUsersInFavorOfLibrary", {params:httpParams}
    );
  }
  requestUsersInFavorOfShelf(shelfGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    httpParams = httpParams.set("bunchIndex", bunchIndex);
    if(filter && filter.trim().length >= 3){
      httpParams = httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Library/GetUsersInFavorOfShelf", {params:httpParams}
    );
  }
  requestUsersInFavorOfDocument(documentGuid:string, bunchIndex:number, filter?:string|null){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    httpParams = httpParams.set("bunchIndex", bunchIndex);
    if(filter && filter.trim().length >= 3){
      httpParams = httpParams.set("filter", filter.trim());
    }
    return this.httpClient.get<OwnerModel[]>(
      "/api/Library/GetUsersInFavorOfDocument", {params:httpParams}
    );
  }

  requestToToggleFavoriteLibrary(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/ToggleFavoriteLibrary", null, {params:httpParams}
    );
  }
  requestToToggleFavoriteShelf(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/ToggleFavoriteShelf", null, {params:httpParams}
    );
  }
  requestToToggleFavoriteDocument(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.post<{success:boolean}>(
      "/api/Library/ToggleFavoriteDocument", null, {params:httpParams}
    );
  }

  isMyFavoriteLibrary(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid",libraryGuid);
    return this.httpClient.get<{isMyFavorite:boolean}>(
      "/api/Library/IsMyFavoriteLibrary",{params:httpParams}
    );
  }
  isMyFavoriteShelf(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid",shelfGuid);
    return this.httpClient.get<{isMyFavorite:boolean}>(
      "/api/Library/IsMyFavoriteShelf",{params:httpParams}
    );
  }
  isMyFavoriteDocument(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid",documentGuid);
    return this.httpClient.get<{isMyFavorite:boolean}>(
      "/api/Library/IsMyFavoriteDocument",{params:httpParams}
    );
  }

  requestEditDocumentTags(tags:string[], documentGuid:string){
    const formData = new FormData();
    formData.append("DocumentGuid", documentGuid);
    tags.forEach((value,index)=>{
      formData.append(`Tags[${index}]`, value);
    });
    return this.httpClient.post<string[]>(
      "/api/Library/EditDocumentTags", formData
    )
  }

  requestTags(partialName:string){
    let httpParams = new HttpParams().set("partialName", partialName);
    return this.httpClient.get<string[]>(
      "/api/Library/GetTags", {params:httpParams}
    )
  }
  requestUserTags(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserTags",{params:httpParams}
    );
  }
  requestUserFavoriteLibrariesTags(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserFavoriteLibrariesTags",{params:httpParams}
    );
  }
  requestUserFavoriteShelvesTags(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserFavoriteShelvesTags",{params:httpParams}
    );
  }
  requestUserFavoriteDocumentsTags(ownerGuid:string){
    let httpParams = new HttpParams().set("ownerGuid", ownerGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetUserFavoriteDocumentsTags",{params:httpParams}
    );
  }
  requestLibraryTags(libraryGuid:string){
    let httpParams = new HttpParams().set("libraryGuid", libraryGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetLibraryTags",{params:httpParams}
    );
  }
  requestShelfTags(shelfGuid:string){
    let httpParams = new HttpParams().set("shelfGuid", shelfGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetShelfTags",{params:httpParams}
    );
  }
  requestDocumentTags(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid", documentGuid);
    return this.httpClient.get<string[]>(
      "/api/Library/GetLibraryTags",{params:httpParams}
    );
  }

  searchDocuments(tags?:string[],title?:string|null,sortBy?:"newest"|"popular", pageIndex?:number, pageSize?:number){
    if((!tags || tags.length == 0) && 
      (!title || title.trim().length < this.singleton.introductionTitle_MinLength())
    ){
      return of([""]);
    }
    
    let httpParams = new HttpParams();
    
    if(tags && tags.length > 0){
      tags.forEach((value,index)=>{
        httpParams = httpParams.set(`tags[${index}]`,value);
      });
    }
    if(title && title.trim().length >= this.singleton.introductionTitle_MinLength()){
      httpParams = httpParams.set("title",title.trim());
    }
    httpParams = httpParams.set("sortBy",sortBy ?? "newest");
    
    if(pageIndex){
      httpParams = httpParams.set("pageIndex",pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize",pageSize);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/SearchDocuments", {params:httpParams}
    );
  }
  requestTotalNumberOfSearchDocuments(tags?:string[],title?:string|null){
    if((!tags || tags.length == 0) && 
      (!title || title.trim().length < this.singleton.introductionTitle_MinLength())
    ){
      return of({totalNumberOfItems : 0});
    }
    
    let httpParams = new HttpParams();
    
    if(tags && tags.length > 0){
      tags.forEach((value,index)=>{
        httpParams = httpParams.set(`tags[${index}]`,value);
      });
    }
    if(title && title.trim().length >= this.singleton.introductionTitle_MinLength()){
      httpParams = httpParams.set("title",title.trim());
    }

    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfSearchDocuments", {params:httpParams}
    );
  }

  requestAddVersionRelationship(baseDocumentGuid:string, newRelatedDocumentGuid:string){
    let httpParams = new HttpParams().set("baseDocumentGuid",baseDocumentGuid);
    httpParams = httpParams.set("newRelatedDocumentGuid",newRelatedDocumentGuid);
    return this.httpClient.post<{versionName:string, documentGuid:string}[]>(
      "/api/Library/AddVersionRelationship",null,{params:httpParams}
    );
  }
  requestEditVersionName(documentGuid:string, versionName:string){
    let httpParams = new HttpParams().set("documentGuid",documentGuid);
    httpParams = httpParams.set("versionName",versionName);
    return this.httpClient.post<{version:string}>(
      "/api/Library/EditDocumentVersionName",null,{params:httpParams}
    );
  }
  requestCreateNewDocumentVersion(baseDocumentGuid:string, newVersionName:string){
    let httpParams = new HttpParams().set("baseDocumentGuid",baseDocumentGuid);
    httpParams = httpParams.set("newVersionName",newVersionName);
    return this.httpClient.post<{newVersionGuid:string}>(
      "/api/Library/CreateNewDocumentVersion",null,{params:httpParams}
    );
  }
  requestDeleteVersionRelationship(documentGuid:string){
    let httpParams = new HttpParams().set("documentGuid",documentGuid);
    return this.httpClient.delete<{ success:boolean }>(
      "/api/Library/DeleteVersionRelationship",{params:httpParams}
    );
  }

  requestFavoriteLibrariesGuids(userGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetFavoriteLibrariesGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserFavoriteLibraries(userGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserFavoriteLibraries", {params:httpParams}
    );
  }
  requestFavoriteShelvesGuids(userGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetFavoriteShelvesGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserFavoriteShelves(userGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserFavoriteShelves", {params:httpParams}
    );
  }
  requestFavoriteDocumentsGuids(userGuid:string, pageIndex?:number, pageSize?:number, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(pageIndex){
      httpParams = httpParams.set("pageIndex", pageIndex);
    }
    if(pageSize){
      httpParams = httpParams.set("pageSize", pageSize);
    }
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
      httpParams = httpParams.set("sortBy",filterInfo.sortBy);
    }
    return this.httpClient.get<string[]>(
      "/api/Library/GetFavoriteDocumentsGuids", {params:httpParams}
    );
  }
  requestTotalNumberOfUserFavoriteDocuments(userGuid:string, filterInfo?:GenericListFilter){
    let httpParams = new HttpParams().set("userGuid",userGuid);
    if(filterInfo){
      if(filterInfo.tags && filterInfo.tags.length > 0){
        filterInfo.tags.forEach((value,index)=>{
          httpParams = httpParams.set(`tags[${index}]`,value);
        });
      }
      if(filterInfo.title && filterInfo.title.trim().length >= this.singleton.introductionTitle_MinLength()){
        httpParams = httpParams.set("title",filterInfo.title.trim());
      }
    }
    return this.httpClient.get<{totalNumberOfItems:number}>(
      "/api/Library/TotalNumberOfUserFavoriteDocuments", {params:httpParams}
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
  constructor(ownerModel:OwnerModel){
    this.guid = ownerModel.guid;
    this.username = ownerModel.username;
    this.hasImage = ownerModel.hasImage;
    this.integrityVersion = ownerModel.integrityVersion;
  }
  guid:string = null!;
  username:string = null!;
  hasImage:boolean = false;
  integrityVersion:number = 0;
}
/*export class RuCache<T extends {guid:string}>{
  private capacity:number = 50;
  private cache:T[] = [];

  getWithGuid(guid:string):T|null{
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
    
    this.cache.unshift(...newValues);
  }
  delete(guid:string){
    let index = this.cache.findIndex(value=>value.guid === guid);
    if(index >= 0){
      this.cache.splice(index,1);
    }
  }
  getArrayReference():T[]{
    return this.cache;
  }
}*/

