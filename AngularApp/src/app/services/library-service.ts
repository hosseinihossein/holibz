import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { IdentityService } from './identity-service';
import { LibraryCardModel } from '../library/library-card/library-card';

@Injectable({
  providedIn: 'root'
})
export class LibraryService {
  private httpClient = inject(HttpClient);
  private identityService = inject(IdentityService);

  requestLibraries(userGuid: string|null = null){
    let quryParams:HttpParams;
    if(userGuid){
      quryParams = new HttpParams().set("userGuid", userGuid);
    }
    else if(this.identityService.isAuthenticated()){
      quryParams = new HttpParams().set("userGuid", this.identityService.userModel()!.guid);
    }
    else{
      return null;
    }
    return this.httpClient.get<LibraryCardModel[]>("/api/Library/List", { params: quryParams});
  }
}

