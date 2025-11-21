import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { ShelfCard, ShelfCardModel } from "../shelf-card/shelf-card";
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService, OwnerModel } from '../../services/library-service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';
import { MatIcon } from '@angular/material/icon';
import { SingletonModes } from '../../services/singleton-modes';

@Component({
  selector: 'app-shelves-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, ShelfCard, MatAccordion,
  ],
  templateUrl: './shelves-list.html',
  styleUrl: './shelves-list.css'
})
export class ShelvesList {
  libraryGuid = input.required<string>();
  ownerGuid = input.required<string>();

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  identityService = inject(IdentityService);
  singleton = inject(SingletonModes);

  shelfModels = signal<ShelfCardModel[]>([]);
  ownerModel = signal<OwnerModel|null>(null);
  //ownerImgSrc = computed(()=>this.singleton.getUserImageAddress(this.ownerModel()));
  /*isMyShelfList = computed(() => this.identityService.isAuthenticated() && 
  this.identityService.userModel()?.userGuid === this.ownerModel()?.userGuid);*/

  constructor(){

    effect(() => {
      if(this.libraryGuid()){
        this.libraryService.requestShelfList(this.libraryGuid()).subscribe({
          next: res => {
            if(res){
              this.shelfModels.set(res);
            }
          },
        });
      }
    });
    
    effect(() => {
      if(this.ownerGuid()){
        this.libraryService.requestOwnerModel(this.ownerGuid()).subscribe({
          next: res => {
            this.ownerModel.set(res);
          },
        });
      }
    });
  }
}
