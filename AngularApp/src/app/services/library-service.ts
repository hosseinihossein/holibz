import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IdentityService } from './identity-service';
import { LibraryModel } from '../library/library-card/library-card';
import { ShelfModel } from '../library/shelf-card/shelf-card';
import { DocumentCardModel } from '../library/document-card/document-card';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  private identityService = inject(IdentityService);
  currentLibraryModel = signal<LibraryModel|null>(null);
  currentShelfModel = signal<ShelfModel|null>(null);

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
    return this.httpClient.get<LibraryModel[]>("/api/Library/List", { params: quryParams});
  }
  requestShelfList(userGuid: string|null = null){
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
    return this.httpClient.get<ShelfModel[]>("/api/Library/ShelfList", { params: quryParams});
  }

  requestTotalNumberOfShelves(userGuid: string|null = null){
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
  }
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
    return this.httpClient.get<LibraryModel>(
      "/api/Library/LibraryModel", {params: httpParams}
    );
  }
  requestDocumentModel(docGuid: string){
    let httpParams = new HttpParams().set("docGuid", docGuid);
    return this.httpClient.get<DocumentCardModel>(
      "/api/Library/DocumentModel", {params: httpParams}
    );
  }



}

