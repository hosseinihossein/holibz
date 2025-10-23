import { Component, computed, effect, inject, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { ShelfCard, ShelfModel } from "../shelf-card/shelf-card";
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute, Router } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';

@Component({
  selector: 'app-shelves-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, ShelfCard, MatAccordion],
  templateUrl: './shelves-list.html',
  styleUrl: './shelves-list.css'
})
export class ShelvesList {
  userGuid = signal<string|null>(null);

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  identityService = inject(IdentityService);

  shelfModels = signal<ShelfModel[]>([]);
  totalNumberOfUserDocuments = signal(0);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyShelfList = signal(false);

  constructor(){
    let userGuidRouteParam = this.activatedRoute.snapshot.paramMap.get("userGuid");
    if(userGuidRouteParam){
      this.userGuid.set(userGuidRouteParam);
    }

    if(this.identityService.isAuthenticated() && 
    this.identityService.userModel()?.guid == this.userGuid()){
      this.isMyShelfList.set(true);
    }

    effect(() => {
      this.libraryService.requestShelfList(this.userGuid())?.subscribe({
        next: res => {
          if(res){
            this.shelfModels.set(res);
            this.totalNumberOfUserDocuments.set(res.flatMap(shelf=>shelf.documentsBriefs).length);
          }
        },
      });
      
      this.identityService.requestUserModel(this.userGuid()!).subscribe({
        next: res => {
          this.userModel.set(res);
        },
      });
    });


    if(this.isMyShelfList()){
      this.identityService.getCsrf().subscribe({
        next: () => {
          console.log("Csrf received successfully.");
        },
        error: err => {
          console.error("Couldn't get Csrf!");
          throw(err);
        },
      });
    }
  }
}
