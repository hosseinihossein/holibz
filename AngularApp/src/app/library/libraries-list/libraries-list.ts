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
  imports: [LibraryCard, MatButton, MatIcon, RouterLink,
    MatSidenavModule
  ],
  templateUrl: './libraries-list.html',
  styleUrl: './libraries-list.css'
})
export class LibrariesList {
  ownerGuid = signal<string|null>(null);
  showFavorites = signal<boolean>(false);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);
  identityService = inject(IdentityService);
  singleton = inject(SingletonModes);

  libraryModels = signal<LibraryCardModel[]>([]);
  
  isMyLibraries = computed(()=>this.identityService.isAuthenticated() && 
  this.ownerGuid() === this.identityService.userModel()?.guid);

  constructor(){
    this.activatedRoute.paramMap.subscribe(params=>{
      if(params.has("userGuid")){
        this.ownerGuid.set(params.get("userGuid"));
      }
    });

    this.activatedRoute.queryParamMap.subscribe(params=>{
      if(params.has("favorites")){
        this.showFavorites.set(params.get("favorites") === "true");
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
      if(this.showFavorites()){

        this.libraryService.requestFavoriteLibrariesGuids(this.ownerGuid()!).subscribe({
          next: libGuids => {
            if(libGuids && libGuids.length > 0){
              for(let libGuid of libGuids){
                this.libraryService.requestLibraryModel(libGuid).subscribe({
                  next: libModel => {
                    if(libModel){
                      this.libraryModels.set([...this.libraryModels(), libModel]);
                    }
                  },
                });
              }
            }
          }
        });

      }
      else{

        this.libraryService.requestLibrariesGuids(this.ownerGuid()!).subscribe({
          next: res => {
            if(res){
              for(let libGuid of res){
                this.libraryService.requestLibraryModel(libGuid).subscribe({
                  next: libModel => {
                    if(libModel){
                      this.libraryModels.set([...this.libraryModels(), libModel]);
                    }
                  },
                });
              }
            }
          },
        });
  
      }
    });
  }

  

}
