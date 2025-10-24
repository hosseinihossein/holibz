import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { LibraryCard, LibraryModel } from "../library-card/library-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatTooltip } from '@angular/material/tooltip';
import { SingletonModes } from '../../services/singleton-modes';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { JsonPipe, NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-libraries-list',
  imports: [LibraryCard, MatButton, MatIcon, MatBadge,MatTooltip, NgOptimizedImage],
  templateUrl: './libraries-list.html',
  styleUrl: './libraries-list.css'
})
export class LibrariesList {
  userGuid = signal<string|null>(null);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  identityService = inject(IdentityService);

  libraryModels = signal<LibraryModel[]>([]);
  totalNumberOfUserDocuments = signal(0);
  totalNumberOfUserShelves = signal(0);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyLibraries = signal(false);

  constructor(){
    let userGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("userGuid");
    if(userGuidRouteParam){
      this.userGuid.set(userGuidRouteParam);
    }

    if(!this.userGuid()){
      if(this.identityService.isAuthenticated()){
        this.userGuid.set(this.identityService.userModel()?.guid!);
        this.isMyLibraries.set(true);
      }
      else{
        this.router.navigateByUrl("/login");
      }
    }

    if(this.isMyLibraries()){
      this.identityService.getCsrf().subscribe({
        next: () => {
          console.log("Csrf received successfully.");
        },
        error: err => {
          console.error("Couldn't get Csrf!");
          //throwError(()=>err);//doesn't pass error to the app-error-handler
          throw(err);
        },
      });
    }
    
    effect(()=>{
      this.libraryService.requestLibraryList(this.userGuid())?.subscribe({
        next: res => {
          if(res){
            this.libraryModels.set(res);
            this.totalNumberOfUserShelves.set(this.libraryModels().flatMap(lib=>lib.shelvesTitles).length);
          }
        },
      });

      /*this.libraryService.requestTotalNumberOfShelves(this.userGuid())?.subscribe({
        next: res => {
          if(res){
            this.totalNumberOfUserShelves.set(res.totalNumberOfUserShelves);
          }
        },
      });*/

      this.libraryService.requestTotalNumberOfDocuments(this.userGuid())?.subscribe({
        next: res => {
          if(res){
            this.totalNumberOfUserDocuments.set(res.totalNumberOfUserDocuments);
          }
        },
      });

      this.identityService.requestUserModel(this.userGuid()!).subscribe({
        next: res => {
          this.userModel.set(res);
          this.libraryService.currentOwnerUserModel.set(res);
        },
      });
    });
  }

  

}
