import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { LibraryCard, LibraryCardModel } from "../library-card/library-card";
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

  singletonModes = inject(SingletonModes);
  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  identityService = inject(IdentityService);

  libraryCards = signal<LibraryCardModel[]>([]);
  totalNumberOfShelves = signal(0);
  totalNumberOfUserDocuments = signal(0);
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
      this.libraryService.requestLibraries(this.userGuid())?.subscribe({
        next: res => {
          if(res){
            this.libraryCards.set(res);
            this.totalNumberOfShelves.set(this.libraryCards().flatMap(l=>l.shelvesTitles).length);
            //console.log(JSON.stringify(this.libraryCards()));
          }
        },
      });

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
        },
      });
    });
  }

  

}
