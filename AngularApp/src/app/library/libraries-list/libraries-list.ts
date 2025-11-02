import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { LibraryCard, LibraryCardModel } from "../library-card/library-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatTooltip } from '@angular/material/tooltip';
import { SingletonModes } from '../../services/singleton-modes';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { JsonPipe, NgOptimizedImage } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';

@Component({
  selector: 'app-libraries-list',
  imports: [LibraryCard, MatButton, MatIcon, MatBadge, MatTooltip, RouterLink,
    MatSidenavModule
  ],
  templateUrl: './libraries-list.html',
  styleUrl: './libraries-list.css'
})
export class LibrariesList {
  userGuid = signal<string|null>(null);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  identityService = inject(IdentityService);

  libraryModels = signal<LibraryCardModel[]>([]);
  totalNumberOfUserDocuments = signal(0);
  totalNumberOfUserShelves = signal(0);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyLibraries = computed(()=>this.identityService.isAuthenticated() && 
  this.userModel()?.guid === this.identityService.userModel()?.guid);

  constructor(){
    let userGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("userGuid");
    if(userGuidRouteParam){
      this.userGuid.set(userGuidRouteParam);
    }

    if(!this.userGuid()){
      if(this.identityService.isAuthenticated()){
        this.userGuid.set(this.identityService.userModel()?.guid!);
      }
      else{
        this.router.navigateByUrl("/login");
      }
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
