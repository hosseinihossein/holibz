import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { ShelfCard, ShelfCardModel } from "../shelf-card/shelf-card";
import { MatAccordion } from '@angular/material/expansion';
import { LibraryService } from '../../services/library-service';
import { ActivatedRoute } from '@angular/router';
import { IdentityService, UserProfileModel } from '../../services/identity-service';

@Component({
  selector: 'app-shelves-list',
  imports: [MatSidenavContainer, MatSidenav, MatSidenavContent, ShelfCard, MatAccordion],
  templateUrl: './shelves-list.html',
  styleUrl: './shelves-list.css'
})
export class ShelvesList {
  libraryGuid = input.required<string>();

  libraryService = inject(LibraryService);
  activatedRoute = inject(ActivatedRoute);
  identityService = inject(IdentityService);

  shelfModels = signal<ShelfCardModel[]>([]);
  //totalNumberOfUserDocuments = signal(0);
  userModel = signal<UserProfileModel|null>(null);
  userImgSrc = computed(()=>this.userModel()?.imageAddress);
  isMyShelfList = computed(() => this.identityService.isAuthenticated() && 
  this.identityService.userModel()?.guid === this.userModel()?.guid);//signal(false);

  constructor(){
    this.userModel.set(this.libraryService.currentOwnerUserModel());

    effect(() => {
      this.libraryService.requestShelfList(this.libraryGuid())?.subscribe({
        next: res => {
          if(res){
            this.shelfModels.set(res);
            //this.totalNumberOfUserDocuments.set(res.flatMap(shelf=>shelf.documentsGuids).length);
            /*if(res[0].ownerGuid){
              this.userGuid.set(res[0].ownerGuid);
            }*/
          }
        },
      });
    });
    
    effect(() => {
      if(!this.userModel() && this.shelfModels() && this.shelfModels().length > 0){
        this.identityService.requestUserModel(this.shelfModels()[0].ownerGuid!).subscribe({
          next: res => {
            this.userModel.set(res);
          },
        });
      }
    });

    effect(() => {
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
    });
  }
}
