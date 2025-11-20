import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { LibraryCard, LibraryCardModel } from "../library-card/library-card";
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import { MatTooltip } from '@angular/material/tooltip';
import { SingletonModes } from '../../services/singleton-modes';
import { LibraryService, OwnerModel } from '../../services/library-service';
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
  ownerGuid = signal<string|null>(null);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  identityService = inject(IdentityService);
  singleton = inject(SingletonModes);

  libraryModels = signal<LibraryCardModel[]>([]);
  totalNumberOfUserDocuments = signal(0);
  totalNumberOfUserShelves = signal(0);
  ownerModel = signal<OwnerModel|null>(null);
  //ownerImgSrc = computed(()=>this.singleton.getUserImageAddress(this.ownerModel()));
  isMyLibraries = computed(()=>this.identityService.isAuthenticated() && 
  this.ownerModel()?.guid === this.identityService.userModel()?.guid);

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("userGuid")){
        this.ownerGuid.set(params.get("userGuid"));
      }
    });

    if(!this.ownerGuid()){
      if(this.identityService.isAuthenticated()){
        this.ownerGuid.set(this.identityService.userModel()?.guid!);
      }
      else{
        this.router.navigateByUrl("/login");
      }
    }
    
    effect(()=>{
      if(this.ownerGuid()){

        this.libraryService.requestOwnerModel(this.ownerGuid()!).subscribe({
          next: res => {
            this.ownerModel.set(res);
          },
        });

        this.libraryService.requestLibraryList(this.ownerGuid()!).subscribe({
          next: res => {
            if(res){
              this.libraryModels.set(res);
            }
          },
        });
  
        this.libraryService.requestTotalNumberOfDocuments(this.ownerGuid()!).subscribe({
          next: res => {
            if(res){
              this.totalNumberOfUserDocuments.set(res.totalNumberOfUserDocuments);
            }
          },
        });
        
        this.libraryService.requestTotalNumberOfShelves(this.ownerGuid()!).subscribe({
          next: res => {
            if(res){
              this.totalNumberOfUserShelves.set(res.totalNumberOfUserShelves);
            }
          },
        });

      }
    });
  }

  

}
