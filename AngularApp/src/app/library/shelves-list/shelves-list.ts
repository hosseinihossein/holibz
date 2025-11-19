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
    this.ownerModel.set(this.libraryService.currentOwnerUserModel());

    effect(() => {
      this.libraryService.requestShelfList(this.libraryGuid(), this.ownerModel()?.guid).subscribe({
        next: res => {
          if(res){
            this.shelfModels.set(res);
          }
        },
      });
    });
    
    effect(() => {
      if(!this.ownerModel() && this.shelfModels() && this.shelfModels().length > 0){
        this.libraryService.requestOwnerModel(this.shelfModels()[0].ownerGuid!).subscribe({
          next: res => {
            this.ownerModel.set(res);
          },
        });
      }
    });
  }
}
